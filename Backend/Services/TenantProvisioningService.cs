using System;
using System.Collections.Generic;
using System.Data;
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
        // 1) First, create the School in the admin database to get the SchoolID
        var adminSchool = new School
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

        await _masterDb.Schools.AddAsync(adminSchool);
        await _masterDb.SaveChangesAsync();

        // Get the SchoolID from the admin database
        int adminSchoolID = adminSchool.SchoolID;

        // Create the initial year for the school in the admin database
        var adminYear = new Year
        {
            YearDateStart = DateTime.Now,
            Active = true,
            HireDate = DateTime.UtcNow,
            YearDateEnd = DateTime.Now.AddYears(1),
            SchoolID = adminSchoolID
        };

        try
        {
            await _masterDb.Years.AddAsync(adminYear);
            await _masterDb.SaveChangesAsync();
            
            // Verify the YearID was assigned (it should be > 0 after SaveChangesAsync)
            if (adminYear.YearID == 0)
            {
                // Reload the entity to get the YearID if it wasn't assigned
                await _masterDb.Entry(adminYear).ReloadAsync();
            }
            
            // Verify the Year was actually saved by querying it back
            var savedYear = await _masterDb.Years
                .FirstOrDefaultAsync(y => y.SchoolID == adminSchoolID && y.Active == true);
            
            if (savedYear == null)
            {
                throw new InvalidOperationException($"Failed to create Year in admin database. Year was not found after SaveChangesAsync.");
            }
            
            // Use the saved Year's ID to ensure we have the correct YearID
            adminYear.YearID = savedYear.YearID;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create Year in admin database: {ex.Message}", ex);
        }

        // 2) Generate a unique database name
        var newDbName = $"School_{Guid.NewGuid().ToString("N").Substring(0, 8)}";

        // 3) CREATE DATABASE via raw SQL
        using (var connection = new SqlConnection(_sqlAdminConnectionString))
        {
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = $"CREATE DATABASE [{newDbName}]";
            await cmd.ExecuteNonQueryAsync();
        }

        // 4) Build the connection string for the new database
        var newDbConnString = $"Server=localhost\\SQLEXPRESS;Database={newDbName};Trusted_Connection=True;TrustServerCertificate=True";

        // 5) Migrate the new DB
        var builder = new DbContextOptionsBuilder<DatabaseContext>();
        builder.UseSqlServer(newDbConnString);
        School newSchool;
        using (var newDbContext = new DatabaseContext(builder.Options))
        {
            newDbContext.Database.Migrate(); // Apply migrations

            // 6) Insert School in tenant database with the same SchoolID from admin database
            // This ensures the SchoolID matches the admin database
            // Use a transaction to ensure IDENTITY_INSERT and insert happen on the same connection
            using (var transaction = await newDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Enable IDENTITY_INSERT within the transaction
                    await newDbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Schools ON;");

                    // Create the school entity with the same SchoolID from admin database
                    newSchool = new School
                    {
                        SchoolID = adminSchoolID, // Use the same SchoolID from admin database
                        SchoolName = adminSchool.SchoolName,
                        SchoolNameEn = adminSchool.SchoolNameEn,
                        HireDate = adminSchool.HireDate,
                        SchoolVison = adminSchool.SchoolVison,
                        SchoolMission = adminSchool.SchoolMission,
                        SchoolGoal = adminSchool.SchoolGoal,
                        Notes = adminSchool.Notes,
                        Country = adminSchool.Country,
                        City = adminSchool.City,
                        SchoolPhone = adminSchool.SchoolPhone,
                        Address = adminSchool.Address,
                        Mobile = adminSchool.Mobile,
                        Description = adminSchool.Description,
                        Website = adminSchool.Website,
                        Street = adminSchool.Street,
                        SchoolType = adminSchool.SchoolType,
                        SchoolCategory = adminSchool.SchoolCategory,
                        Email = adminSchool.Email,
                        fax = adminSchool.fax,
                        zone = adminSchool.zone,
                        ImageURL = adminSchool.ImageURL
                    };

                    // Add the school to the tenant database context
                    newDbContext.Schools.Add(newSchool);
                    await newDbContext.SaveChangesAsync();

                    // Disable IDENTITY_INSERT
                    await newDbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Schools OFF;");

                    // Commit the transaction
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            // Create the initial year for the school in tenant database with the same YearID from admin database
            // Use a transaction to ensure IDENTITY_INSERT and insert happen on the same connection
            using (var yearTransaction = await newDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Enable IDENTITY_INSERT for Years
                    await newDbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Years ON;");

                    // Create the year entity with the same YearID from admin database
                    var tenantYear = new Year
                    {
                        YearID = adminYear.YearID, // Use the same YearID from admin database
                        YearDateStart = adminYear.YearDateStart,
                        Active = adminYear.Active,
                        HireDate = adminYear.HireDate,
                        YearDateEnd = adminYear.YearDateEnd,
                        SchoolID = adminSchoolID // Use the admin SchoolID
                    };

                    await newDbContext.Years.AddAsync(tenantYear);
                    await newDbContext.SaveChangesAsync();

                    // Disable IDENTITY_INSERT
                    await newDbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Years OFF;");

                    // Commit the transaction
                    await yearTransaction.CommitAsync();
                }
                catch
                {
                    await yearTransaction.RollbackAsync();
                    throw;
                }
            }

        }

        // 7) Insert a record in the master database's Tenants table
        // This is the ONLY place where Tenant should be created (in admin database)
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

