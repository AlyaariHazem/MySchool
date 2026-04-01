namespace Backend.Configuration;

/// <summary>
/// Options for restoring .bak files into SQL Server (e.g. Docker).
/// When SQL runs in Docker, <see cref="HostBackupRoot"/> must be the host folder mounted into the container,
/// and <see cref="SqlServerBackupRoot"/> must be the same location as seen by SQL Server inside the container.
/// </summary>
public class SqlRestoreOptions
{
    public const string SectionName = "SqlRestore";

    /// <summary>Directory where the API saves uploaded .bak files (host / API process path).</summary>
    public string HostBackupRoot { get; set; } = "";

    /// <summary>
    /// Directory path used in RESTORE ... FROM DISK = N'...' as SQL Server resolves it (e.g. /var/opt/mssql/backup).
    /// Only used when <see cref="UseContainerPathForRestoreDisk"/> is true. If empty, <see cref="HostBackupRoot"/> is used with normalized separators.
    /// </summary>
    public string SqlServerBackupRoot { get; set; } = "";

    /// <summary>
    /// When false (default), RESTORE uses the absolute path where the API saved the .bak (works for SQL Server on the same Windows host).
    /// When true, uses <see cref="SqlServerBackupRoot"/> + relative path (Docker: mount <see cref="HostBackupRoot"/> to that path in the SQL container).
    /// </summary>
    public bool UseContainerPathForRestoreDisk { get; set; }

    /// <summary>Directory for MOVE targets (must end with / or \). Example: /var/opt/mssql/data/ or C:\SqlData\</summary>
    public string SqlServerDataPath { get; set; } = "/var/opt/mssql/data/";

    /// <summary>Remove the uploaded .bak file after a successful restore.</summary>
    public bool DeleteBackupAfterSuccess { get; set; } = true;

    /// <summary>Tables in dbo to skip when copying from temp into the target (keeps target migration history by default).</summary>
    public string[] ExcludeTablesFromImport { get; set; } = ["__EFMigrationsHistory"];

    /// <summary>
    /// dbo tables that are not cleared before import: existing rows stay, and rows from the backup are inserted only when
    /// no row with the same primary key exists (e.g. <c>AspNetUsers</c> so current logins are preserved).
    /// </summary>
    public string[] MergeImportTables { get; set; } = ["AspNetUsers"];
}
