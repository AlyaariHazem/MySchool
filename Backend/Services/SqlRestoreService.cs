using System.Data;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Backend.Configuration;
using Backend.DTOS.School.DatabaseRestore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Backend.Services;

public class SqlRestoreService
{
    private readonly IConfiguration _configuration;
    private readonly SqlRestoreOptions _options;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<SqlRestoreService> _logger;

    public SqlRestoreService(
        IConfiguration configuration,
        IOptions<SqlRestoreOptions> options,
        IHostEnvironment hostEnvironment,
        ILogger<SqlRestoreService> logger)
    {
        _configuration = configuration;
        _options = options.Value;
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    /// <summary>
    /// Saves the .bak, restores to a temporary database, copies dbo user-table data into the existing
    /// database from SqlAdminConnection, drops the temp database, and deletes the uploaded file (if configured).
    /// </summary>
    public async Task<DatabaseImportResultDTO> ImportBackupIntoExistingDatabaseAsync(
        Stream backupStream,
        string originalFileName,
        CancellationToken cancellationToken = default)
    {
        ValidateOptions();

        var adminConnectionString = _configuration.GetConnectionString("SqlAdminConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:SqlAdminConnection is not configured.");
        var targetDbName = GetDatabaseNameFromConnectionString(adminConnectionString);
        if (string.IsNullOrWhiteSpace(targetDbName))
        {
            throw new InvalidOperationException(
                "SqlAdminConnection must specify Initial Catalog / Database so data can be imported into the active database.");
        }

        var hostRoot = ResolveHostBackupRoot();
        Directory.CreateDirectory(hostRoot);

        var safeFileName = SanitizeFileName(originalFileName);
        var uniqueName = $"{Guid.NewGuid():N}_{safeFileName}";
        var relativeUnderRoot = Path.Combine("uploads", uniqueName);
        var hostBackupPath = Path.GetFullPath(Path.Combine(hostRoot, relativeUnderRoot));
        var hostBackupDir = Path.GetDirectoryName(hostBackupPath)!;
        Directory.CreateDirectory(hostBackupDir);

        await using (var fs = new FileStream(hostBackupPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
        {
            await backupStream.CopyToAsync(fs, cancellationToken);
        }

        EnsureBackupReadableBySqlServerProcess(hostBackupPath);

        var sqlBackupPath = _options.UseContainerPathForRestoreDisk
            ? BuildSqlServerPathForBackup(relativeUnderRoot, hostRoot)
            : Path.GetFullPath(hostBackupPath);
        var masterConnectionString = BuildMasterConnectionString();

        var tempDbName = $"TempImport_{Guid.NewGuid():N}";
        var result = new DatabaseImportResultDTO
        {
            TargetDatabaseName = targetDbName,
            TemporaryDatabaseName = tempDbName,
            BackupPathOnServer = sqlBackupPath
        };

        try
        {
            var logicalFiles = await ReadFileListOnlyAsync(masterConnectionString, sqlBackupPath, cancellationToken)
                .ConfigureAwait(false);

            if (logicalFiles.Count == 0)
            {
                throw new InvalidOperationException("RESTORE FILELISTONLY returned no files. The backup may be invalid.");
            }

            if (string.Equals(tempDbName, targetDbName, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Temporary database name would collide with the target database.");
            }

            var dataPath = ResolveSqlServerDataPath();
            var moveClauses = BuildMoveClauses(tempDbName, dataPath, logicalFiles);
            var restoreSql = BuildRestoreCommand(tempDbName, sqlBackupPath, moveClauses);

            await ExecuteNonQueryAsync(masterConnectionString, restoreSql, cancellationToken).ConfigureAwait(false);

            var imported = await CopyDboTablesFromToAsync(
                    masterConnectionString,
                    tempDbName,
                    targetDbName,
                    cancellationToken)
                .ConfigureAwait(false);

            result.TablesImported = imported.Count;
            result.ImportedTableNames = imported;

            if (_options.DeleteBackupAfterSuccess)
            {
                try { File.Delete(hostBackupPath); } catch { /* best effort */ }
            }

            return result;
        }
        catch
        {
            try { if (File.Exists(hostBackupPath)) File.Delete(hostBackupPath); } catch { /* ignore */ }
            throw;
        }
        finally
        {
            try
            {
                await DropDatabaseIfExistsAsync(masterConnectionString, tempDbName, cancellationToken).ConfigureAwait(false);
                result.TemporaryDatabaseDropped = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to drop temporary database {TempDb}. Drop it manually if it still exists.", tempDbName);
                result.TemporaryDatabaseDropped = false;
            }
        }
    }

    private static string GetDatabaseNameFromConnectionString(string connectionString)
    {
        var b = new SqlConnectionStringBuilder(connectionString);
        return b.InitialCatalog;
    }

    private async Task<IReadOnlyList<string>> CopyDboTablesFromToAsync(
        string masterConnectionString,
        string sourceDb,
        string targetDb,
        CancellationToken cancellationToken)
    {
        var exclude = new HashSet<string>(
            _options.ExcludeTablesFromImport ?? Array.Empty<string>(),
            StringComparer.OrdinalIgnoreCase);

        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        var tables = await GetIntersectingDboUserTablesAsync(connection, sourceDb, targetDb, exclude, cancellationToken)
            .ConfigureAwait(false);

        if (tables.Count == 0)
        {
            _logger.LogWarning("No matching dbo user tables to import between {Source} and {Target}.", sourceDb, targetDb);
            return Array.Empty<string>();
        }

        // Order by the *target* FK graph so it matches the live DB (backup can differ or omit tables).
        var fkEdges = await GetForeignKeyEdgesAsync(connection, targetDb, tables, cancellationToken)
            .ConfigureAwait(false);

        var insertOrder = TopologicalSortTables(tables, fkEdges);
        var deleteOrder = insertOrder.AsEnumerable().Reverse().ToList();

        var allTargetDboTables = await GetAllDboUserTableNamesAsync(connection, targetDb, cancellationToken)
            .ConfigureAwait(false);

        var foreignKeys = await GetAllForeignKeyConstraintsAsync(connection, targetDb, cancellationToken)
            .ConfigureAwait(false);

        // Must use the target database as the connection context. From master, three-part ALTER/DELETE
        // can leave FK enforcement active and cause 547 when deleting referenced rows (e.g. Tenants).
        await connection.ChangeDatabaseAsync(targetDb, cancellationToken).ConfigureAwait(false);

        await using var tx = connection.BeginTransaction();
        try
        {
            foreach (var fk in foreignKeys)
            {
                var nocheck =
                    $"ALTER TABLE {QuoteSchemaTable(fk.SchemaName, fk.TableName)} NOCHECK CONSTRAINT {QuoteBracket(fk.ConstraintName)}";
                await ExecuteNonQueryTxAsync(connection, tx, nocheck, cancellationToken).ConfigureAwait(false);
            }

            foreach (var table in allTargetDboTables)
            {
                var nocheckAll = $"ALTER TABLE {QuoteSchemaTable("dbo", table)} NOCHECK CONSTRAINT ALL";
                await ExecuteNonQueryTxAsync(connection, tx, nocheckAll, cancellationToken).ConfigureAwait(false);
            }

            foreach (var table in deleteOrder)
            {
                var sql = $"DELETE FROM {QuoteSchemaTable("dbo", table)}";
                await ExecuteNonQueryTxAsync(connection, tx, sql, cancellationToken).ConfigureAwait(false);
            }

            foreach (var table in insertOrder)
            {
                await CopySingleTableAsync(connection, tx, sourceDb, table, cancellationToken)
                    .ConfigureAwait(false);
            }

            foreach (var fk in foreignKeys)
            {
                var check =
                    $"ALTER TABLE {QuoteSchemaTable(fk.SchemaName, fk.TableName)} WITH CHECK CHECK CONSTRAINT {QuoteBracket(fk.ConstraintName)}";
                await ExecuteNonQueryTxAsync(connection, tx, check, cancellationToken).ConfigureAwait(false);
            }

            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }

        return insertOrder;
    }

    private static async Task<List<string>> GetAllDboUserTableNamesAsync(
        SqlConnection connection,
        string database,
        CancellationToken cancellationToken)
    {
        var sql = $"""
            SELECT t.name
            FROM {QuoteDbBracket(database)}.sys.tables t
            INNER JOIN {QuoteDbBracket(database)}.sys.schemas s ON t.schema_id = s.schema_id
            WHERE s.name = N'dbo' AND t.type = N'U' AND t.is_ms_shipped = 0
            ORDER BY t.name
            """;

        var list = new List<string>();
        await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 0 };
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            if (reader.IsDBNull(0))
            {
                continue;
            }

            list.Add(reader.GetString(0));
        }

        return list;
    }

    private static async Task<List<string>> GetIntersectingDboUserTablesAsync(
        SqlConnection connection,
        string sourceDb,
        string targetDb,
        HashSet<string> exclude,
        CancellationToken cancellationToken)
    {
        var sql = $"""
            SELECT t.name
            FROM {QuoteDbBracket(sourceDb)}.sys.tables t
            INNER JOIN {QuoteDbBracket(sourceDb)}.sys.schemas s ON t.schema_id = s.schema_id
            WHERE s.name = N'dbo' AND t.type = N'U' AND t.is_ms_shipped = 0
            INTERSECT
            SELECT t2.name
            FROM {QuoteDbBracket(targetDb)}.sys.tables t2
            INNER JOIN {QuoteDbBracket(targetDb)}.sys.schemas s2 ON t2.schema_id = s2.schema_id
            WHERE s2.name = N'dbo' AND t2.type = N'U' AND t2.is_ms_shipped = 0
            ORDER BY 1
            """;

        var list = new List<string>();
        await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 0 };
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            if (reader.IsDBNull(0))
            {
                continue;
            }

            var name = reader.GetString(0);
            if (!exclude.Contains(name))
            {
                list.Add(name);
            }
        }

        return list;
    }

    private sealed record ForeignKeyConstraintRow(string SchemaName, string TableName, string ConstraintName);

    private static async Task<List<ForeignKeyConstraintRow>> GetAllForeignKeyConstraintsAsync(
        SqlConnection connection,
        string database,
        CancellationToken cancellationToken)
    {
        var sql = $"""
            SELECT ISNULL(OBJECT_SCHEMA_NAME(fk.parent_object_id), N'dbo') AS SchemaName,
                   OBJECT_NAME(fk.parent_object_id) AS TableName,
                   fk.name AS ConstraintName
            FROM {QuoteDbBracket(database)}.sys.foreign_keys fk
            """;

        var list = new List<ForeignKeyConstraintRow>();
        await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 0 };
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var schema = reader.IsDBNull(0) ? "dbo" : reader.GetString(0);
            if (reader.IsDBNull(1) || reader.IsDBNull(2))
            {
                continue;
            }

            var table = reader.GetString(1);
            var name = reader.GetString(2);
            list.Add(new ForeignKeyConstraintRow(schema, table, name));
        }

        return list;
    }

    private static async Task<List<(string Referenced, string Referencing)>> GetForeignKeyEdgesAsync(
        SqlConnection connection,
        string sourceDb,
        IReadOnlyCollection<string> tables,
        CancellationToken cancellationToken)
    {
        var tableSet = new HashSet<string>(tables, StringComparer.OrdinalIgnoreCase);
        var sql = $"""
            SELECT OBJECT_NAME(f.referenced_object_id) AS RefTable, OBJECT_NAME(f.parent_object_id) AS ChildTable
            FROM {QuoteDbBracket(sourceDb)}.sys.foreign_keys f
            WHERE OBJECT_SCHEMA_NAME(f.referenced_object_id) = N'dbo'
              AND OBJECT_SCHEMA_NAME(f.parent_object_id) = N'dbo'
            """;

        var edges = new List<(string, string)>();
        await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 0 };
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            if (reader.IsDBNull(0) || reader.IsDBNull(1))
            {
                continue;
            }

            var refT = reader.GetString(0);
            var child = reader.GetString(1);
            if (tableSet.Contains(refT) && tableSet.Contains(child))
            {
                edges.Add((refT, child));
            }
        }

        return edges;
    }

    /// <summary>Referenced table must be inserted before referencing table (edge: parent -&gt; child).</summary>
    private static List<string> TopologicalSortTables(IReadOnlyList<string> tables, IReadOnlyList<(string Referenced, string Referencing)> fkEdges)
    {
        var tableSet = new HashSet<string>(tables, StringComparer.OrdinalIgnoreCase);
        var graph = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var inDegree = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var t in tables)
        {
            graph[t] = new List<string>();
            inDegree[t] = 0;
        }

        foreach (var (referenced, referencing) in fkEdges)
        {
            if (!tableSet.Contains(referenced) || !tableSet.Contains(referencing))
            {
                continue;
            }

            graph[referenced].Add(referencing);
            inDegree[referencing]++;
        }

        var queue = new Queue<string>(inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));
        var result = new List<string>();

        while (queue.Count > 0)
        {
            var n = queue.Dequeue();
            result.Add(n);
            foreach (var next in graph[n])
            {
                inDegree[next]--;
                if (inDegree[next] == 0)
                {
                    queue.Enqueue(next);
                }
            }
        }

        if (result.Count != tables.Count)
        {
            throw new InvalidOperationException(
                "Could not determine a safe import order for tables (possible circular foreign keys). " +
                "Resolve circular dependencies or exclude tables via SqlRestore:ExcludeTablesFromImport.");
        }

        return result;
    }

    private async Task CopySingleTableAsync(
        SqlConnection connection,
        SqlTransaction tx,
        string sourceDb,
        string table,
        CancellationToken cancellationToken)
    {
        var columns = await GetCommonWritableColumnsAsync(connection, tx, sourceDb, table, cancellationToken)
            .ConfigureAwait(false);

        if (columns.Count == 0)
        {
            _logger.LogWarning("Skipping table {Table}: no common writable columns between backup and target.", table);
            return;
        }

        var hasIdentity = await TableHasIdentityAsync(connection, tx, table, cancellationToken)
            .ConfigureAwait(false);

        var colList = string.Join(", ", columns.Select(c => QuoteBracket(c)));
        var targetQualified = QuoteSchemaTable("dbo", table);
        var sourceQualified = QuoteDbTable(sourceDb, "dbo", table);

        var disableTriggers = $"ALTER TABLE {targetQualified} DISABLE TRIGGER ALL";
        await ExecuteNonQueryTxAsync(connection, tx, disableTriggers, cancellationToken).ConfigureAwait(false);

        try
        {
            if (hasIdentity)
            {
                var on = $"SET IDENTITY_INSERT {targetQualified} ON";
                await ExecuteNonQueryTxAsync(connection, tx, on, cancellationToken).ConfigureAwait(false);
            }

            var insert = $"INSERT INTO {targetQualified} ({colList}) SELECT {colList} FROM {sourceQualified}";
            await ExecuteNonQueryTxAsync(connection, tx, insert, cancellationToken).ConfigureAwait(false);

            if (hasIdentity)
            {
                var off = $"SET IDENTITY_INSERT {targetQualified} OFF";
                await ExecuteNonQueryTxAsync(connection, tx, off, cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            var enableTriggers = $"ALTER TABLE {targetQualified} ENABLE TRIGGER ALL";
            await ExecuteNonQueryTxAsync(connection, tx, enableTriggers, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>Requires <paramref name="connection"/> current database to be the import target.</summary>
    private static async Task<List<string>> GetCommonWritableColumnsAsync(
        SqlConnection connection,
        SqlTransaction tx,
        string sourceDb,
        string table,
        CancellationToken cancellationToken)
    {
        var sql = $"""
            SELECT c.name
            FROM sys.columns c
            INNER JOIN sys.tables t ON c.object_id = t.object_id
            INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
            WHERE s.name = N'dbo' AND t.name = @table
              AND c.is_computed = 0
              AND c.is_rowguidcol = 0
              AND TYPE_NAME(c.user_type_id) NOT IN (N'timestamp')
              AND EXISTS (
                  SELECT 1
                  FROM {QuoteDbBracket(sourceDb)}.sys.columns c2
                  INNER JOIN {QuoteDbBracket(sourceDb)}.sys.tables t2 ON c2.object_id = t2.object_id
                  INNER JOIN {QuoteDbBracket(sourceDb)}.sys.schemas s2 ON t2.schema_id = s2.schema_id
                  WHERE s2.name = N'dbo' AND t2.name = @table AND c2.name = c.name
                    AND c2.is_computed = 0
                    AND c2.is_rowguidcol = 0
                    AND TYPE_NAME(c2.user_type_id) NOT IN (N'timestamp')
              )
            ORDER BY c.column_id
            """;

        await using var cmd = new SqlCommand(sql, connection, tx) { CommandTimeout = 0 };
        cmd.Parameters.AddWithValue("@table", table);
        var list = new List<string>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            if (reader.IsDBNull(0))
            {
                continue;
            }

            list.Add(reader.GetString(0));
        }

        return list;
    }

    /// <summary>Requires <paramref name="connection"/> current database to be the import target.</summary>
    private static async Task<bool> TableHasIdentityAsync(
        SqlConnection connection,
        SqlTransaction tx,
        string table,
        CancellationToken cancellationToken)
    {
        var sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1
                FROM sys.identity_columns ic
                INNER JOIN sys.tables t ON ic.object_id = t.object_id
                INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = N'dbo' AND t.name = @table
            ) THEN 1 ELSE 0 END
            """;

        await using var cmd = new SqlCommand(sql, connection, tx) { CommandTimeout = 0 };
        cmd.Parameters.AddWithValue("@table", table);
        var o = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return ScalarToBool(o);
    }

    /// <summary>Maps SQL scalar (bit/int/byte/long, DBNull) to bool.</summary>
    private static bool ScalarToBool(object? value)
    {
        if (value is null || value is DBNull)
        {
            return false;
        }

        if (value is bool b)
        {
            return b;
        }

        try
        {
            return Convert.ToInt32(value, CultureInfo.InvariantCulture) != 0;
        }
        catch (InvalidCastException)
        {
            return false;
        }
    }

    private static async Task DropDatabaseIfExistsAsync(string masterConnectionString, string databaseName, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        var check = $"SELECT database_id FROM sys.databases WHERE name = N'{EscapeSqlLiteral(databaseName)}'";
        await using var cmdCheck = new SqlCommand(check, connection) { CommandTimeout = 0 };
        var exists = await cmdCheck.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (exists is null || exists is DBNull)
        {
            return;
        }

        var singleUser = $"ALTER DATABASE [{EscapeSqlBracketIdentifier(databaseName)}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
        await using (var cmd1 = new SqlCommand(singleUser, connection) { CommandTimeout = 0 })
        {
            try
            {
                await cmd1.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (SqlException)
            {
                // Already single user or race; try DROP anyway
            }
        }

        var drop = $"DROP DATABASE [{EscapeSqlBracketIdentifier(databaseName)}]";
        await using var cmd2 = new SqlCommand(drop, connection) { CommandTimeout = 0 };
        await cmd2.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task ExecuteNonQueryTxAsync(
        SqlConnection connection,
        SqlTransaction tx,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(sql, connection, tx) { CommandTimeout = 0 };
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string QuoteDbBracket(string database) => $"[{EscapeSqlBracketIdentifier(database)}]";
    private static string QuoteBracket(string identifier) => $"[{EscapeSqlBracketIdentifier(identifier)}]";
    private static string QuoteDbTable(string database, string schema, string table) =>
        $"{QuoteDbBracket(database)}.{QuoteBracket(schema)}.{QuoteBracket(table)}";

    private static string QuoteSchemaTable(string schema, string table) =>
        $"{QuoteBracket(schema)}.{QuoteBracket(table)}";

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.HostBackupRoot))
        {
            throw new InvalidOperationException(
                "SqlRestore:HostBackupRoot is not configured. Set it to a folder shared with SQL Server (Docker volume).");
        }
    }

    private void EnsureBackupReadableBySqlServerProcess(string hostBackupPath)
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        try
        {
            const UnixFileMode mode =
                UnixFileMode.UserRead | UnixFileMode.UserWrite
                | UnixFileMode.GroupRead | UnixFileMode.OtherRead;
            File.SetUnixFileMode(hostBackupPath, mode);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not chmod backup file for SQL Server read access.");
        }
    }

    private string ResolveHostBackupRoot()
    {
        var raw = _options.HostBackupRoot.Trim();
        if (LooksLikeWindowsAbsolutePath(raw) && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var fallback = Path.GetFullPath(Path.Combine(_hostEnvironment.ContentRootPath, "SqlRestore", "uploads"));
            _logger.LogWarning(
                "SqlRestore:HostBackupRoot is a Windows path ({Configured}) on a non-Windows OS. Using fallback {Fallback}. " +
                "Set SqlRestore:HostBackupRoot to a Linux path (e.g. /var/sql-restore/uploads) and ensure SQL Server can read it (shared volume).",
                raw,
                fallback);
            return fallback;
        }

        return Path.GetFullPath(raw);
    }

    private static bool LooksLikeWindowsAbsolutePath(string path)
    {
        if (path.Length >= 2 && char.IsLetter(path[0]) && path[1] == ':')
        {
            return true;
        }

        if (path.StartsWith("\\\\", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    private string ResolveSqlServerDataPath()
    {
        var raw = _options.SqlServerDataPath?.Trim() ?? "";
        if (string.IsNullOrEmpty(raw))
        {
            return NormalizeDataPath("/var/opt/mssql/data/");
        }

        if (LooksLikeWindowsAbsolutePath(raw) && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _logger.LogWarning(
                "SqlRestore:SqlServerDataPath is a Windows path on Linux. Using /var/opt/mssql/data/. Set SqlServerDataPath for your SQL container.");
            return NormalizeDataPath("/var/opt/mssql/data/");
        }

        return NormalizeDataPath(raw);
    }

    private string BuildSqlServerPathForBackup(string relativeUnderHostRoot, string resolvedHostRoot)
    {
        var root = string.IsNullOrWhiteSpace(_options.SqlServerBackupRoot)
            ? resolvedHostRoot
            : _options.SqlServerBackupRoot;

        root = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var relative = relativeUnderHostRoot.Replace(Path.DirectorySeparatorChar, '/').Replace('\\', '/');
        return $"{root.Replace('\\', '/')}/{relative}";
    }

    private static string NormalizeDataPath(string path)
    {
        path = path.Trim();
        if (string.IsNullOrEmpty(path))
        {
            return "/var/opt/mssql/data/";
        }

        return path.EndsWith('/') || path.EndsWith('\\')
            ? path.Replace('\\', '/')
            : path.Replace('\\', '/') + "/";
    }

    private string BuildMasterConnectionString()
    {
        var admin = _configuration.GetConnectionString("SqlAdminConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:SqlAdminConnection is not configured.");

        var builder = new SqlConnectionStringBuilder(admin)
        {
            InitialCatalog = "master"
        };
        return builder.ConnectionString;
    }

    private sealed record LogicalFileRow(string LogicalName, string Type);

    private static async Task<List<LogicalFileRow>> ReadFileListOnlyAsync(
        string masterConnectionString,
        string sqlBackupPath,
        CancellationToken cancellationToken)
    {
        var sql = $"RESTORE FILELISTONLY FROM DISK = N'{EscapeSqlLiteral(sqlBackupPath)}'";

        var list = new List<LogicalFileRow>();
        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 0 };
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var ordLogical = reader.GetOrdinal("LogicalName");
        var ordType = reader.GetOrdinal("Type");

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            if (reader.IsDBNull(ordLogical) || reader.IsDBNull(ordType))
            {
                continue;
            }

            var logical = reader.GetString(ordLogical);
            var type = reader.GetString(ordType);
            list.Add(new LogicalFileRow(logical, type));
        }

        return list;
    }

    private static string BuildMoveClauses(
        string databaseName,
        string dataPath,
        IReadOnlyList<LogicalFileRow> files)
    {
        var sb = new StringBuilder();
        var dataIndex = 0;

        for (var i = 0; i < files.Count; i++)
        {
            var row = files[i];
            var isLog = string.Equals(row.Type, "L", StringComparison.OrdinalIgnoreCase);
            string extension;
            if (isLog)
            {
                extension = ".ldf";
            }
            else
            {
                extension = dataIndex == 0 ? ".mdf" : ".ndf";
                dataIndex++;
            }

            var filePart = SanitizeForPhysicalFileSegment(row.LogicalName, i);
            var physical = $"{dataPath}{databaseName}_{i:000}_{filePart}{extension}";

            if (sb.Length > 0)
            {
                sb.Append(", ");
            }

            sb.Append("MOVE N'").Append(EscapeSqlLiteral(row.LogicalName)).Append("' TO N'")
                .Append(EscapeSqlLiteral(physical)).Append('\'');
        }

        return sb.ToString();
    }

    private static string BuildRestoreCommand(string databaseName, string sqlBackupPath, string moveClauses)
    {
        return $"""
            RESTORE DATABASE [{EscapeSqlBracketIdentifier(databaseName)}]
            FROM DISK = N'{EscapeSqlLiteral(sqlBackupPath)}'
            WITH {moveClauses}, REPLACE, STATS = 10
            """;
    }

    private static async Task ExecuteNonQueryAsync(string connectionString, string sql, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 0 };
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string SanitizeForPhysicalFileSegment(string logicalName, int index)
    {
        var s = Regex.Replace(logicalName, @"[^\p{L}\p{Nd}_\-]", "_");
        if (string.IsNullOrEmpty(s))
        {
            s = $"file_{index}";
        }

        return s.Length > 40 ? s[..40] : s;
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        if (string.IsNullOrEmpty(name))
        {
            return "backup.bak";
        }

        name = Regex.Replace(name, @"[^\p{L}\p{Nd}._\-]", "_");
        if (!name.EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
        {
            name += ".bak";
        }

        return name;
    }

    private static string EscapeSqlLiteral(string value) => value.Replace("'", "''");

    private static string EscapeSqlBracketIdentifier(string name) => name.Replace("]", "]]");
}
