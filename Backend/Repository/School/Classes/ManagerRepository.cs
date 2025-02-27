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

namespace Backend.Repository.School.Classes
{
    public class ManagerRepository : IManagerRepository
    {
        private readonly DatabaseContext _context;
        private readonly IUserRepository _userRepository;
        private readonly ITenantRepository _tenantRepository;

        public ManagerRepository(DatabaseContext context, IUserRepository userRepository,
        ITenantRepository tenantRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantRepository = tenantRepository;
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<string> AddManager(AddManagerDTO managerDTO)
        {
            // Create the user in the primary database
            var user = new ApplicationUser
            {
                UserName = managerDTO.UserName,
                Email = managerDTO.Email,
                PhoneNumber = managerDTO.PhoneNumber,
                UserType = "MANAGER"
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
                throw new Exception("Tenant not found.");

            var builder = new DbContextOptionsBuilder<DatabaseContext>();
            builder.UseSqlServer(connectionString.ConnectionString);

            using (var tenantContext = new DatabaseContext(builder.Options))
            {
                tenantContext.Database.Migrate();

                // Create and save user in the tenant database
                var tenantUserStore = tenantContext.Set<ApplicationUser>();

                user = new ApplicationUser
                {
                    UserName = managerDTO.UserName,
                    Email = managerDTO.Email,
                    PhoneNumber = managerDTO.PhoneNumber,
                    UserType = "MANAGER"
                };

                await tenantUserStore.AddAsync(user);
                await tenantContext.SaveChangesAsync();

                // Retrieve the created user
                var createdUserTenant = await tenantContext.Set<ApplicationUser>().FirstOrDefaultAsync(u => u.Email == managerDTO.Email);
                if (createdUserTenant == null)
                {
                    throw new Exception("Failed to create user in tenant database.");
                }

                var NewTenant = new Tenant
                {
                    ConnectionString = connectionString.ConnectionString,
                    SchoolName = connectionString.SchoolName
                };

                tenantContext.Add(NewTenant);
                await tenantContext.SaveChangesAsync();

                // Create a new Manager instance to be added to the tenant database.
                var tenantManager = new Manager
                {
                    FullName = managerDTO.FullName,
                    UserID = createdUserTenant.Id,
                    SchoolID = 1,
                    TenantID = NewTenant.TenantId
                };

                try
                {
                    tenantContext.Managers.Add(tenantManager);
                    await tenantContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to add manager to the tenant database.", e);
                }
            }
            return "Manager added successfully.";
        }

        public async Task DeleteManager(int id)
        {
            var manager = await _context.Managers.FindAsync(id);
            if (manager == null)
            {
                throw new Exception("Manager not found.");
            }

            _context.Managers.Remove(manager);
            await _context.SaveChangesAsync();
        }

        public async Task<ManagerDTO> GetManager(int id)
        {
            var manager = await _context.Managers
                .Include(m => m.ApplicationUser)
                .FirstOrDefaultAsync(m => m.ManagerID == id);

            if (manager == null)
            {
                return null;
            }

            return new ManagerDTO
            {
                ManagerID = manager.ManagerID,
                FullName = manager.FullName,
                UserID = manager.UserID,
                SchoolID = manager.SchoolID,
                UserName = manager.ApplicationUser?.UserName,
                Email = manager.ApplicationUser?.Email,
                UserType = "MANAGER"
            };
        }

        public async Task<List<GetManagerDTO>> GetManagers()
        {
            var managers = await _context.Managers
                .Include(m => m.ApplicationUser)
                .Include(m => m.School) // Ensure the School entity is included to get SchoolName
                .ToListAsync();

            return managers.Select(m => new GetManagerDTO
            {
                ManagerID = m.ManagerID,
                FullName = m.FullName,
                HireDate = m.ApplicationUser?.HireDate ?? DateTime.Now, // Get HireDate from ApplicationUser if available
                SchoolName = m.School?.SchoolName, // Assuming there's a navigation property `School` in Manager
                UserName = m.ApplicationUser?.UserName,
                Email = m.ApplicationUser?.Email,
                UserType = "MANAGER",
                PhoneNumber = m.ApplicationUser?.PhoneNumber
            }).ToList();
        }


        public async Task UpdateManager(ManagerDTO managerDTO)
        {
            var manager = await _context.Managers.Include(m => m.ApplicationUser)
                .FirstOrDefaultAsync(m => m.ManagerID == managerDTO.ManagerID);

            if (manager == null)
            {
                throw new Exception("Manager not found.");
            }

            manager.FullName = managerDTO.FullName;

            if (manager.ApplicationUser != null)
            {
                manager.ApplicationUser.UserName = managerDTO.UserName;
                manager.ApplicationUser.Email = managerDTO.Email;
            }

            _context.Entry(manager).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
