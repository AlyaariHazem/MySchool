using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.DTOS.School.Manager;
using Backend.DTOS.School.Tenant;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School.Classes;

public class ManagerRepository : IManagerRepository
{
    private readonly TenantDbContext _tenantContext;
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TenantInfo _tenantInfo;

    public ManagerRepository(
        TenantDbContext tenantContext,
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        UserManager<ApplicationUser> userManager,
        TenantInfo tenantInfo)
    {
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _tenantInfo = tenantInfo ?? throw new ArgumentNullException(nameof(tenantInfo));
    }

    public async Task<string> AddManager(AddManagerDTO managerDTO)
    {
        var user = new ApplicationUser
        {
            UserName = managerDTO.UserName,
            Email = managerDTO.Email,
            PhoneNumber = managerDTO.PhoneNumber,
            UserType = "MANAGER",
            HireDate = DateTime.Now,
        };

        var createdUser = await _userRepository.CreateUserAsync(user, managerDTO.Password, "MANAGER");

        var tid = managerDTO.TenantID ?? _tenantInfo.TenantId
            ?? throw new InvalidOperationException("TenantID is required to add a manager.");

        var tenantRow = await _tenantRepository.GetByIdAsync(tid);
        var tenantInfo = new TenantInfo { TenantId = tid, ConnectionString = tenantRow.ConnectionString };
        var opts = new DbContextOptionsBuilder<TenantDbContext>()
            .UseSqlServer(tenantRow.ConnectionString, sql => sql.CommandTimeout(180))
            .Options;

        await using var tenantDb = new TenantDbContext(opts, tenantInfo);
        await tenantDb.Database.MigrateAsync();

        var schoolExists = await tenantDb.Schools.AnyAsync(s => s.SchoolID == managerDTO.SchoolID);
        if (!schoolExists)
            throw new InvalidOperationException(
                $"School with ID {managerDTO.SchoolID} does not exist in the tenant database.");

        var existing = await tenantDb.Managers
            .FirstOrDefaultAsync(m => m.UserID == createdUser.Id && m.SchoolID == managerDTO.SchoolID);

        if (existing != null)
        {
            existing.FullName = managerDTO.FullName;
            tenantDb.Managers.Update(existing);
        }
        else
        {
            tenantDb.Managers.Add(new Manager
            {
                FullName = managerDTO.FullName,
                UserID = createdUser.Id,
                SchoolID = managerDTO.SchoolID,
                TenantID = null
            });
        }

        await tenantDb.SaveChangesAsync();
        return "Manager added successfully.";
    }

    public async Task<GetManagerDTO?> GetManager(int id)
    {
        var manager = await _tenantContext.Managers
            .AsNoTracking()
            .Include(m => m.School)
            .FirstOrDefaultAsync(m => m.ManagerID == id);

        if (manager == null)
            return null;

        var appUser = await _userManager.FindByIdAsync(manager.UserID);
        TenantDTO? tenantMeta = null;
        if (_tenantInfo.TenantId is int tId)
        {
            try
            {
                tenantMeta = await _tenantRepository.GetByIdAsync(tId);
            }
            catch
            {
                /* ignore */
            }
        }

        return Map(manager, appUser, tenantMeta);
    }

    public async Task<List<GetManagerDTO>> GetManagers()
    {
        var managers = await _tenantContext.Managers
            .AsNoTracking()
            .Include(m => m.School)
            .ToListAsync();

        TenantDTO? tenantMeta = null;
        if (_tenantInfo.TenantId is int tId)
        {
            try
            {
                tenantMeta = await _tenantRepository.GetByIdAsync(tId);
            }
            catch
            {
                /* ignore */
            }
        }

        var userIds = managers.Select(m => m.UserID).Distinct().ToList();
        var users = new Dictionary<string, ApplicationUser?>(StringComparer.Ordinal);
        foreach (var uid in userIds)
        {
            users[uid] = await _userManager.FindByIdAsync(uid);
        }

        return managers.Select(m => Map(m, users.GetValueOrDefault(m.UserID), tenantMeta)).ToList();
    }

    private static GetManagerDTO Map(Manager manager, ApplicationUser? appUser, TenantDTO? tenantMeta)
    {
        return new GetManagerDTO
        {
            ManagerID = manager.ManagerID,
            FullName = manager.FullName,
            HireDate = appUser?.HireDate ?? DateTime.Now,
            SchoolName = manager.School?.SchoolName ?? "",
            TenantID = tenantMeta?.TenantID,
            TenantName = tenantMeta?.SchoolName,
            UserName = appUser?.UserName ?? "",
            Email = appUser?.Email,
            UserType = "MANAGER",
            PhoneNumber = appUser?.PhoneNumber
        };
    }

    public async Task UpdateManager(GetManagerDTO managerDTO)
    {
        var manager = await _tenantContext.Managers
            .Include(m => m.School)
            .FirstOrDefaultAsync(m => m.ManagerID == managerDTO.ManagerID);

        if (manager == null)
            throw new InvalidOperationException("Manager not found.");

        manager.FullName = managerDTO.FullName;

        var appUser = await _userManager.FindByIdAsync(manager.UserID);
        if (appUser != null)
        {
            appUser.UserName = managerDTO.UserName;
            appUser.Email = managerDTO.Email;
            await _userManager.UpdateAsync(appUser);
        }

        await _tenantContext.SaveChangesAsync();
    }

    public async Task DeleteManager(int id)
    {
        var manager = await _tenantContext.Managers.FirstOrDefaultAsync(m => m.ManagerID == id);
        if (manager == null)
            throw new InvalidOperationException("Manager not found.");

        await _userRepository.DeleteAsync(manager.UserID);
        _tenantContext.Managers.Remove(manager);
        await _tenantContext.SaveChangesAsync();
    }
}
