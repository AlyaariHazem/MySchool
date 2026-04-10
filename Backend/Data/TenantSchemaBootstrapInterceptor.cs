using System.Collections.Concurrent;
using System.Data.Common;
using Backend.Migrations.Tenant;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Backend.Data;

/// <summary>
/// Runs <see cref="TenantSchoolsBootstrapSql"/> once per tenant database on first SQL connection open
/// (creates missing tables and renames legacy name columns to EF owned-type names).
/// </summary>
public sealed class TenantSchemaBootstrapInterceptor : DbConnectionInterceptor
{
    public static TenantSchemaBootstrapInterceptor Instance { get; } = new();

    private TenantSchemaBootstrapInterceptor()
    {
    }

    private static readonly ConcurrentDictionary<string, byte> Applied = new(StringComparer.Ordinal);
    private static readonly SemaphoreSlim Gate = new(1, 1);

    private static string DatabaseKey(DbConnection connection)
    {
        if (connection is SqlConnection sql)
        {
            try
            {
                var b = new SqlConnectionStringBuilder(sql.ConnectionString);
                return $"{b.DataSource}\0{b.InitialCatalog}";
            }
            catch
            {
                return sql.ConnectionString;
            }
        }

        return connection.ConnectionString;
    }

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        ApplyBootstrapIfNeeded(connection);
        base.ConnectionOpened(connection, eventData);
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await ApplyBootstrapIfNeededAsync(connection, cancellationToken);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    private static void ApplyBootstrapIfNeeded(DbConnection connection)
    {
        if (connection is not SqlConnection)
            return;

        var key = DatabaseKey(connection);
        if (string.IsNullOrWhiteSpace(key) || Applied.ContainsKey(key))
            return;

        Gate.Wait();
        try
        {
            if (Applied.ContainsKey(key))
                return;

            using var cmd = connection.CreateCommand();
            cmd.CommandText = TenantSchoolsBootstrapSql.CreateSchoolsIfMissingSql;
            cmd.CommandTimeout = 180;
            cmd.ExecuteNonQuery();
            Applied.TryAdd(key, 1);
        }
        finally
        {
            Gate.Release();
        }
    }

    private static async Task ApplyBootstrapIfNeededAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        if (connection is not SqlConnection)
            return;

        var key = DatabaseKey(connection);
        if (string.IsNullOrWhiteSpace(key) || Applied.ContainsKey(key))
            return;

        await Gate.WaitAsync(cancellationToken);
        try
        {
            if (Applied.ContainsKey(key))
                return;

            await using var cmd = connection.CreateCommand();
            cmd.CommandText = TenantSchoolsBootstrapSql.CreateSchoolsIfMissingSql;
            cmd.CommandTimeout = 180;
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            Applied.TryAdd(key, 1);
        }
        finally
        {
            Gate.Release();
        }
    }
}

/// <summary>Shared SQL Server setup for <see cref="TenantDbContext"/> (migrations assembly + schema bootstrap interceptor).</summary>
public static class TenantDbContextSqlExtensions
{
    public static DbContextOptionsBuilder<TenantDbContext> UseTenantSqlServer(
        this DbContextOptionsBuilder<TenantDbContext> builder,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        builder.UseSqlServer(connectionString, sql =>
        {
            sql.CommandTimeout(180);
            sql.MigrationsAssembly(typeof(TenantDbContext).Assembly.FullName);
        });
        builder.AddInterceptors(TenantSchemaBootstrapInterceptor.Instance);
        return builder;
    }

    public static DbContextOptionsBuilder UseTenantSqlServer(this DbContextOptionsBuilder builder, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        builder.UseSqlServer(connectionString, sql =>
        {
            sql.CommandTimeout(180);
            sql.MigrationsAssembly(typeof(TenantDbContext).Assembly.FullName);
        });
        builder.AddInterceptors(TenantSchemaBootstrapInterceptor.Instance);
        return builder;
    }
}
