using System;
using System.Threading.Tasks;
using Backend.Data;
using Backend.DTOS.School;
using Backend.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Backend.Services;

public class TenantProvisioningService
{
    private readonly DatabaseContext _masterDb;
    private readonly string _sqlAdminConnectionString;
    private readonly IConfiguration _configuration;

    public TenantProvisioningService(DatabaseContext masterDb, IConfiguration configuration)
    {
        _masterDb = masterDb;
        _configuration = configuration;
        _sqlAdminConnectionString = configuration.GetConnectionString("SqlAdminConnection")
            ?? throw new InvalidOperationException("SqlAdminConnection is not configured.");
    }

    /// <summary>
    /// Creates a new tenant database (school business data only), applies tenant migrations, seeds school + year, registers <see cref="Tenant"/> in the master DB.
    /// </summary>
    public async Task<Tenant> CreateSchoolDatabaseAsync(string schoolName, SchoolDTO schoolDTO)
    {
        var newDbName = $"School_{Guid.NewGuid().ToString("N")[..8]}";

        await using (var connection = new SqlConnection(_sqlAdminConnectionString))
        {
            await connection.OpenAsync();
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = $"CREATE DATABASE [{newDbName}]";
            await cmd.ExecuteNonQueryAsync();
        }

        var baseConnectionString = _configuration.GetConnectionString("SqlAdminConnection");
        var sqlBuilder = new SqlConnectionStringBuilder(baseConnectionString)
        {
            InitialCatalog = newDbName
        };
        var newDbConnString = sqlBuilder.ConnectionString;

        var tenantInfo = new TenantInfo { TenantId = null, ConnectionString = newDbConnString };
        var opts = new DbContextOptionsBuilder<TenantDbContext>()
            .UseSqlServer(newDbConnString, sql => sql.CommandTimeout(180))
            .Options;

        await using (var tenantCtx = new TenantDbContext(opts, tenantInfo))
        {
            await tenantCtx.Database.MigrateAsync();

            var school = new School
            {
                SchoolName = schoolDTO.SchoolName ?? "",
                SchoolNameEn = schoolDTO.SchoolNameEn ?? "",
                HireDate = DateTime.UtcNow,
                SchoolVison = schoolDTO.SchoolVison,
                SchoolMission = schoolDTO.SchoolMission,
                SchoolGoal = schoolDTO.SchoolGoal ?? "",
                Notes = schoolDTO.Notes,
                Country = schoolDTO.Country ?? "",
                City = schoolDTO.City ?? "",
                SchoolPhone = schoolDTO.SchoolPhone,
                Address = schoolDTO.Address,
                Mobile = schoolDTO.Mobile,
                Description = schoolDTO.Description,
                Website = schoolDTO.Website,
                Street = schoolDTO.Street ?? "",
                SchoolType = schoolDTO.SchoolType ?? "",
                SchoolCategory = schoolDTO.SchoolCategory,
                Email = schoolDTO.Email,
                fax = schoolDTO.fax,
                zone = schoolDTO.zone
            };

            await tenantCtx.Schools.AddAsync(school);
            await tenantCtx.SaveChangesAsync();

            var year = new Year
            {
                YearDateStart = DateTime.Now,
                Active = true,
                HireDate = DateTime.UtcNow,
                YearDateEnd = DateTime.Now.AddYears(1),
                SchoolID = school.SchoolID
            };

            await tenantCtx.Years.AddAsync(year);
            await tenantCtx.SaveChangesAsync();
        }

        var tenant = new Tenant
        {
            SchoolName = schoolName,
            ConnectionString = newDbConnString
        };

        await _masterDb.Tenants.AddAsync(tenant);
        await _masterDb.SaveChangesAsync();

        return tenant;
    }
}
