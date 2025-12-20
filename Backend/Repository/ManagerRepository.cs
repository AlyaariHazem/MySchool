using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Manager;
using Backend.Models;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Repository.School.Implements;
using Microsoft.AspNetCore.Identity;
using Backend.DTOS.School.Tenant;
using Microsoft.Data.SqlClient;

namespace Backend.Repository.School.Classes
{
    public class ManagerRepository : IManagerRepository
    {
        private readonly DatabaseContext _context;
        private readonly IUserRepository _userRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;


        public ManagerRepository(DatabaseContext context, IUserRepository userRepository,
        ITenantRepository tenantRepository, IPasswordHasher<ApplicationUser> passwordHasher)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantRepository = tenantRepository;
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher;
        }

        public async Task<string> AddManager(AddManagerDTO managerDTO)
        {
            // Create the user in the primary database
            var user = new ApplicationUser
            {
                UserName = managerDTO.UserName,
                Email = managerDTO.Email,
                PhoneNumber = managerDTO.PhoneNumber,
                UserType = "MANAGER",
                HireDate = DateTime.Now,
            };

            var createdUser = await _userRepository.CreateUserAsync(user, managerDTO.Password, "MANAGER");

            var Manager = new Manager
            {
                FullName = managerDTO.FullName,
                UserID = createdUser.Id,
                SchoolID = managerDTO.SchoolID,
                TenantID = managerDTO.TenantID
            };

            try
            {
                _context.Managers.Add(Manager);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to add manager to the tenant database.", e);
            }

            // Retrieve the tenant's connection string from the primary database.
            var connectionString = await _tenantRepository.GetByIdAsync(managerDTO.TenantID ?? 1);
            if (connectionString == null)
                return null!;

            // Use DatabaseContext for tenant operations (it has ApplicationUser and Manager tables)
            var builder = new DbContextOptionsBuilder<DatabaseContext>();
            builder.UseSqlServer(connectionString.ConnectionString);

            using (var tenantContext = new DatabaseContext(builder.Options))
            {
                tenantContext.Database.Migrate();

                // Verify the School exists in the tenant database
                var schoolExists = await tenantContext.Schools.AnyAsync(s => s.SchoolID == managerDTO.SchoolID);
                if (!schoolExists)
                {
                    throw new Exception($"School with ID {managerDTO.SchoolID} does not exist in the tenant database. Please ensure the school is properly provisioned.");
                }

                // Check if user already exists in tenant database (by username or email)
                var tenantUserStore = tenantContext.Set<ApplicationUser>();
                var existingUser = await tenantUserStore
                    .FirstOrDefaultAsync(u => u.UserName == managerDTO.UserName || 
                                             (!string.IsNullOrEmpty(managerDTO.Email) && u.Email == managerDTO.Email));
                
                ApplicationUser createdUserTenant;
                
                if (existingUser != null)
                {
                    // User already exists, use the existing user
                    createdUserTenant = existingUser;
                }
                else
                {
                    // Create new user in the tenant database
                    // Generate a unique ID for the user
                    var userId = Guid.NewGuid().ToString();
                    
                    user = new ApplicationUser
                    {
                        Id = userId,
                        UserName = managerDTO.UserName,
                        Email = managerDTO.Email,
                        PhoneNumber = managerDTO.PhoneNumber,
                        UserType = managerDTO.UserType ?? "MANAGER",
                        NormalizedUserName = managerDTO.UserName?.ToUpper(),
                        NormalizedEmail = managerDTO.Email?.ToUpper(),
                        EmailConfirmed = true,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        ConcurrencyStamp = Guid.NewGuid().ToString(),
                        HireDate = DateTime.UtcNow
                    };
                    user.PasswordHash = _passwordHasher.HashPassword(user, managerDTO.Password);
                    
                    try
                    {
                        await tenantUserStore.AddAsync(user);
                        await tenantContext.SaveChangesAsync();
                        
                        // Retrieve the created user
                        createdUserTenant = await tenantUserStore.FirstOrDefaultAsync(u => u.Id == userId);
                        if (createdUserTenant == null)
                            throw new Exception("Failed to retrieve created user from tenant database.");
                    }
                    catch (DbUpdateException dbEx)
                    {
                        // Check if it's a unique constraint violation
                        if (dbEx.InnerException is SqlException sqlEx && 
                            (sqlEx.Number == 2601 || sqlEx.Number == 2627)) // Unique constraint violation
                        {
                            throw new Exception($"Username '{managerDTO.UserName}' or email is already taken in the tenant database.", dbEx);
                        }
                        throw new Exception($"Failed to create user in tenant database. Error: {dbEx.Message}", dbEx);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Failed to create user in tenant database. Error: {e.Message}", e);
                    }
                }

                // Check if Manager already exists for this user and school
                var existingManager = await tenantContext.Managers
                    .FirstOrDefaultAsync(m => m.UserID == createdUserTenant.Id && m.SchoolID == managerDTO.SchoolID);
                
                if (existingManager != null)
                {
                    // Manager already exists, update it instead of creating a new one
                    existingManager.FullName = managerDTO.FullName;
                    tenantContext.Managers.Update(existingManager);
                    await tenantContext.SaveChangesAsync();
                }
                else
                {
                    // Create a new Manager instance to be added to the tenant database.
                    // Use the same SchoolID from managerDTO to match the admin database
                    // TenantID is not needed in tenant databases since we're already in that tenant's database
                    var tenantManager = new Manager
                    {
                        FullName = managerDTO.FullName,
                        UserID = createdUserTenant.Id,
                        SchoolID = managerDTO.SchoolID, // Use the actual SchoolID from managerDTO
                        TenantID = null // TenantID is not needed in tenant databases
                    };

                    try
                    {
                        tenantContext.Managers.Add(tenantManager);
                        await tenantContext.SaveChangesAsync();
                    }
                    catch (DbUpdateException dbEx)
                    {
                        // Check if it's a unique constraint violation
                        if (dbEx.InnerException is SqlException sqlEx && 
                            (sqlEx.Number == 2601 || sqlEx.Number == 2627)) // Unique constraint violation
                        {
                            throw new Exception($"Manager already exists for this user and school in the tenant database.", dbEx);
                        }
                        // Include inner exception details for better debugging
                        var errorMessage = $"Failed to add manager to the tenant database. Error: {dbEx.Message}";
                        if (dbEx.InnerException != null)
                        {
                            errorMessage += $" Inner Exception: {dbEx.InnerException.Message}";
                        }
                        throw new Exception(errorMessage, dbEx);
                    }
                    catch (Exception e)
                    {
                        // Include inner exception details for better debugging
                        var errorMessage = $"Failed to add manager to the tenant database. Error: {e.Message}";
                        if (e.InnerException != null)
                        {
                            errorMessage += $" Inner Exception: {e.InnerException.Message}";
                        }
                        throw new Exception(errorMessage, e);
                    }
                }
            }
            return "Manager added successfully.";
        }



        public async Task<GetManagerDTO> GetManager(int id)
        {
            var manager = await _context.Managers
                .Include(m => m.ApplicationUser)
                .Include(m => m.School)
                .Include(m => m.Tenant)
                .FirstOrDefaultAsync(m => m.ManagerID == id);

            if (manager == null)
                return null;

            return new GetManagerDTO
            {
                ManagerID = manager.ManagerID,
                FullName = manager.FullName,
                HireDate = manager.ApplicationUser?.HireDate ?? DateTime.Now, // Get HireDate from ApplicationUser if available
                SchoolName = manager.School?.SchoolName!, // Assuming there's a navigation property `School` in Manager
                TenantID = manager.TenantID,
                TenantName = manager.Tenant?.SchoolName,
                UserName = manager.ApplicationUser?.UserName!,
                Email = manager.ApplicationUser?.Email,
                UserType = "MANAGER",
                PhoneNumber = manager.ApplicationUser?.PhoneNumber
            };
        }

        public async Task<List<GetManagerDTO>> GetManagers()
        {
            var managers = await _context.Managers
                .Include(m => m.ApplicationUser)
                .Include(m => m.School) // Ensure the School entity is included to get SchoolName
                .Include(m => m.Tenant)
                .ToListAsync();

            return managers.Select(m => new GetManagerDTO
            {
                ManagerID = m.ManagerID,
                FullName = m.FullName,
                HireDate = m.ApplicationUser?.HireDate ?? DateTime.Now, // Get HireDate from ApplicationUser if available
                SchoolName = m.School?.SchoolName!, // Assuming there's a navigation property `School` in Manager
                TenantID = m.TenantID,
                TenantName = m.Tenant?.SchoolName,
                UserName = m.ApplicationUser?.UserName!,
                Email = m.ApplicationUser?.Email,
                UserType = "MANAGER",
                PhoneNumber = m.ApplicationUser?.PhoneNumber
            }).ToList();
        }


        public async Task UpdateManager(GetManagerDTO managerDTO)
        {
            var manager = await _context.Managers
                .Include(m => m.ApplicationUser)
                .Include(m => m.School)
                .Include(m => m.Tenant)
                .FirstOrDefaultAsync(m => m.ManagerID == managerDTO.ManagerID);

            if (manager == null)
                throw new Exception("Manager not found.");

            manager.FullName = managerDTO.FullName;

            if (manager.ApplicationUser != null)
            {
                manager.ApplicationUser.UserName = managerDTO.UserName;
                manager.ApplicationUser.Email = managerDTO.Email;
            }

            _context.Entry(manager).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteManager(int id)
        {
            var manager = await _context.Managers.FindAsync(id);
            if (manager is null)
                throw new Exception("Manager not found.");

            try
            {
                // Use IUserRepository to delete user (works with DatabaseContext)
                await _userRepository.DeleteAsync(manager.UserID);
                _context.Managers.Remove(manager);
                await _context.SaveChangesAsync();
            }

            catch (Exception e)
            {
                throw new Exception("Failed to delete manager.", e);
            }
        }
    }

}
