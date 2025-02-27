using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.DTOS.School;
using Backend.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class TenantProvisioningService
{
    private readonly DatabaseContext _masterDb; // Points to the main DB with the Tenants table
    private readonly string _sqlAdminConnectionString; // Connection to your SQL Server for raw CREATE DATABASE

    public TenantProvisioningService(DatabaseContext masterDb, IConfiguration config)
    {
        _masterDb = masterDb;
        _sqlAdminConnectionString = config.GetConnectionString("SqlAdminConnection");

    }

    public async Task<Tenant> CreateSchoolDatabaseAsync(string schoolName, SchoolDTO schoolDTO)
    {
        // 1) Generate a unique database name
        var newDbName = $"School_{Guid.NewGuid().ToString("N").Substring(0, 8)}";

        // 2) CREATE DATABASE via raw SQL
        using (var connection = new SqlConnection(_sqlAdminConnectionString))
        {
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = $"CREATE DATABASE [{newDbName}]";
            await cmd.ExecuteNonQueryAsync();
        }

        // 3) Build the connection string for the new database
        var newDbConnString = $"Server=Hazem\\SQLEXPRESS01;Database={newDbName};Trusted_Connection=True;TrustServerCertificate=True";

        // 4) Migrate the new DB
        var builder = new DbContextOptionsBuilder<DatabaseContext>();
        builder.UseSqlServer(newDbConnString);
        School newSchool;
        using (var newDbContext = new DatabaseContext(builder.Options))
        {
            newDbContext.Database.Migrate(); // Apply migrations

            // 5) Insert Default Data
            newSchool = new School
            {
                SchoolName = schoolDTO.SchoolName,
                SchoolNameEn = schoolDTO.SchoolNameEn,
                HireDate = DateTime.UtcNow,
                SchoolVison = schoolDTO.SchoolVison,
                SchoolMission = schoolDTO.SchoolMission,
                SchoolGoal = schoolDTO.SchoolGoal,
                Notes = schoolDTO.Notes,
                Country = schoolDTO.Country,
                City = schoolDTO.City,
                SchoolPhone = schoolDTO.SchoolPhone,
                Address = schoolDTO.Address,
                Mobile = schoolDTO.Mobile,
                Description = schoolDTO.Description,
                Website = schoolDTO.Website,
                Street = schoolDTO.Street,
                SchoolType = schoolDTO.SchoolType,
                SchoolCategory = schoolDTO.SchoolCategory,
                Email = schoolDTO.Email,
                fax = schoolDTO.fax,
                zone = schoolDTO.zone
            };

            await newDbContext.Schools.AddAsync(newSchool);
            await newDbContext.SaveChangesAsync();

            var currecntYear = new Year
            {
                YearDateStart = DateTime.Now,
                Active = true,
                HireDate = DateTime.UtcNow,
                YearDateEnd = DateTime.Now.AddYears(1),
                SchoolID = newSchool.SchoolID
            };

            await newDbContext.Years.AddAsync(currecntYear);
            await newDbContext.SaveChangesAsync();

        }

        // 6) Insert a record in the master database's Tenants table
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

