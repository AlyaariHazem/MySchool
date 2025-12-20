using System;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Backend.Services;

/// <summary>
/// Service to fix existing tenant databases by removing foreign key constraints
/// to AspNetUsers that shouldn't exist in tenant databases
/// </summary>
public class TenantDatabaseFixService
{
    private readonly IConfiguration _configuration;

    public TenantDatabaseFixService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Removes foreign key constraints to AspNetUsers from the specified tenant database
    /// </summary>
    public async Task FixTenantDatabaseAsync(string connectionString)
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // Extract database name from connection string
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;

            // List of FK constraints to drop
            var constraintsToDrop = new[]
            {
                "FK_Guardians_AspNetUsers_UserID",
                "FK_Students_AspNetUsers_UserID",
                "FK_Teachers_AspNetUsers_UserID",
                "FK_Managers_AspNetUsers_UserID"
            };

            foreach (var constraintName in constraintsToDrop)
            {
                // Determine table name from constraint name
                string? tableName = constraintName switch
                {
                    "FK_Guardians_AspNetUsers_UserID" => "Guardians",
                    "FK_Students_AspNetUsers_UserID" => "Students",
                    "FK_Teachers_AspNetUsers_UserID" => "Teachers",
                    "FK_Managers_AspNetUsers_UserID" => "Managers",
                    _ => null
                };

                if (tableName == null) continue;

                // Check if constraint exists and drop it
                var checkSql = $@"
                    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = '{constraintName}')
                    BEGIN
                        ALTER TABLE [{tableName}] DROP CONSTRAINT [{constraintName}];
                        SELECT 'Dropped constraint {constraintName}' AS Result;
                    END
                    ELSE
                    BEGIN
                        SELECT 'Constraint {constraintName} does not exist' AS Result;
                    END";

                using var command = new SqlCommand(checkSql, connection);
                var result = await command.ExecuteScalarAsync();
                Console.WriteLine($"Database {databaseName}: {result}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fixing tenant database: {ex.Message}");
            throw;
        }
    }
}

