using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Backend.Data
{
    public sealed class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
    {
        public TenantDbContext CreateDbContext(string[] args)
        {
            // English: Load configuration for design-time (dotnet-ef)
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // English: Pick any valid tenant database connection string for migrations generation
            var cs =
                configuration.GetConnectionString("TenantDesignTime")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? configuration.GetConnectionString("TenantConnection");

            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException(
                    "Design-time connection string not found. Add ConnectionStrings:TenantDesignTime in appsettings.json.");

            var tenant = new TenantInfo
            {
                ConnectionString = cs,
                TenantId = 0
            };

            var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();

            // English: Configure provider explicitly for design-time
            optionsBuilder.UseSqlServer(cs, sql =>
            {
                sql.CommandTimeout(180);
                // English: Optional - keep migrations in the same assembly
                sql.MigrationsAssembly(typeof(TenantDbContext).Assembly.FullName);
            });

            return new TenantDbContext(optionsBuilder.Options, tenant);
        }
    }
}
