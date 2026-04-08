using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.DTOS.School.Manager;
using Backend.DTOS.School.Tenant;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School.Classes;

public class ManagerRepository : IManagerRepository
{
    /// <summary>
    /// Encodes (tenantId, per-tenant ManagerID) into a single int for platform-admin cross-tenant APIs.
    /// Assumes each tenant has fewer than this many manager rows (identity PK).
    /// </summary>
    private const int ManagerKeyMultiplier = 100_000;

    private readonly TenantDbContext _tenantContext;
    private readonly DatabaseContext _masterDb;
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TenantInfo _tenantInfo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ManagerRepository(
        TenantDbContext tenantContext,
        DatabaseContext masterDb,
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        UserManager<ApplicationUser> userManager,
        TenantInfo tenantInfo,
        IHttpContextAccessor httpContextAccessor)
    {
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _masterDb = masterDb ?? throw new ArgumentNullException(nameof(masterDb));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _tenantInfo = tenantInfo ?? throw new ArgumentNullException(nameof(tenantInfo));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <summary>
    /// Platform admin on /api/manager with no JWT tenant: aggregate managers from all tenant DBs.
    /// </summary>
    private bool UseMasterManagerCatalog()
    {
        if (!string.IsNullOrEmpty(_tenantInfo.ConnectionString))
            return false;

        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
            return false;

        if (user.IsInRole("ADMIN"))
            return true;

        var ut = user.FindFirst("UserType")?.Value;
        return string.Equals(ut, "ADMIN", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<TenantDbContext> CreateTenantDbForTenantIdAsync(int tenantId)
    {
        var row = await _masterDb.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenantId == tenantId);
        if (row == null || string.IsNullOrWhiteSpace(row.ConnectionString))
            throw new KeyNotFoundException(
                $"Tenant {tenantId} was not found or has no connection string in the master database.");

        var ti = new TenantInfo { TenantId = tenantId, ConnectionString = row.ConnectionString };
        var ob = new DbContextOptionsBuilder<TenantDbContext>();
        ob.UseSqlServer(row.ConnectionString, sql =>
        {
            sql.CommandTimeout(180);
            sql.MigrationsAssembly(typeof(TenantDbContext).Assembly.FullName);
        });
        return new TenantDbContext(ob.Options, ti);
    }

    private static int EncodeCrossTenantManagerId(int tenantId, int localManagerId) =>
        checked(tenantId * ManagerKeyMultiplier + localManagerId);

    private static bool TryDecodeCrossTenantManagerId(int encoded, out int tenantId, out int localManagerId)
    {
        tenantId = encoded / ManagerKeyMultiplier;
        localManagerId = encoded % ManagerKeyMultiplier;
        return tenantId > 0 && localManagerId > 0;
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
        if (UseMasterManagerCatalog())
        {
            if (!TryDecodeCrossTenantManagerId(id, out var tenantId, out var localId))
                return null;

            await using var db = await CreateTenantDbForTenantIdAsync(tenantId);
            var manager = await db.Managers
                .AsNoTracking()
                .Include(m => m.School)
                .FirstOrDefaultAsync(m => m.ManagerID == localId);

            if (manager == null)
                return null;

            var appUser = await _userManager.FindByIdAsync(manager.UserID);
            TenantDTO? tenantMeta = null;
            try
            {
                tenantMeta = await _tenantRepository.GetByIdAsync(tenantId);
            }
            catch
            {
                /* ignore */
            }

            var dto = Map(manager, appUser, tenantMeta);
            dto.ManagerID = EncodeCrossTenantManagerId(tenantId, manager.ManagerID);
            return dto;
        }

        var managerRow = await _tenantContext.Managers
            .AsNoTracking()
            .Include(m => m.School)
            .FirstOrDefaultAsync(m => m.ManagerID == id);

        if (managerRow == null)
            return null;

        var user = await _userManager.FindByIdAsync(managerRow.UserID);
        TenantDTO? meta = null;
        if (_tenantInfo.TenantId is int tId)
        {
            try
            {
                meta = await _tenantRepository.GetByIdAsync(tId);
            }
            catch
            {
                /* ignore */
            }
        }

        return Map(managerRow, user, meta);
    }

    public async Task<List<GetManagerDTO>> GetManagers()
    {
        if (UseMasterManagerCatalog())
        {
            var tenants = await _masterDb.Tenants.AsNoTracking()
                .Where(t => !string.IsNullOrWhiteSpace(t.ConnectionString))
                .OrderBy(t => t.SchoolName)
                .ToListAsync();

            var result = new List<GetManagerDTO>();
            foreach (var tenant in tenants)
            {
                await using var db = await CreateTenantDbForTenantIdAsync(tenant.TenantId);
                var managers = await db.Managers
                    .AsNoTracking()
                    .Include(m => m.School)
                    .ToListAsync();

                var tenantMeta = new TenantDTO
                {
                    TenantID = tenant.TenantId,
                    SchoolName = tenant.SchoolName,
                    ConnectionString = tenant.ConnectionString
                };

                var userIds = managers.Select(m => m.UserID).Distinct().ToList();
                var users = new Dictionary<string, ApplicationUser?>(StringComparer.Ordinal);
                foreach (var uid in userIds)
                    users[uid] = await _userManager.FindByIdAsync(uid);

                foreach (var m in managers)
                {
                    var dto = Map(m, users.GetValueOrDefault(m.UserID), tenantMeta);
                    dto.ManagerID = EncodeCrossTenantManagerId(tenant.TenantId, m.ManagerID);
                    result.Add(dto);
                }
            }

            return result;
        }

        var managersList = await _tenantContext.Managers
            .AsNoTracking()
            .Include(m => m.School)
            .ToListAsync();

        TenantDTO? tenantMetaSingle = null;
        if (_tenantInfo.TenantId is int tId)
        {
            try
            {
                tenantMetaSingle = await _tenantRepository.GetByIdAsync(tId);
            }
            catch
            {
                /* ignore */
            }
        }

        var ids = managersList.Select(m => m.UserID).Distinct().ToList();
        var userMap = new Dictionary<string, ApplicationUser?>(StringComparer.Ordinal);
        foreach (var uid in ids)
            userMap[uid] = await _userManager.FindByIdAsync(uid);

        return managersList.Select(m => Map(m, userMap.GetValueOrDefault(m.UserID), tenantMetaSingle)).ToList();
    }

    public async Task UpdateManager(GetManagerDTO managerDTO)
    {
        if (UseMasterManagerCatalog())
        {
            if (managerDTO.ManagerID is not int encodedId)
                throw new InvalidOperationException("ManagerID is required.");

            if (!TryDecodeCrossTenantManagerId(encodedId, out var tenantId, out var localId))
                throw new InvalidOperationException("Invalid manager id for cross-tenant update.");

            await using var db = await CreateTenantDbForTenantIdAsync(tenantId);
            var manager = await db.Managers
                .Include(m => m.School)
                .FirstOrDefaultAsync(m => m.ManagerID == localId);

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

            await db.SaveChangesAsync();
            return;
        }

        var existing = await _tenantContext.Managers
            .Include(m => m.School)
            .FirstOrDefaultAsync(m => m.ManagerID == managerDTO.ManagerID);

        if (existing == null)
            throw new InvalidOperationException("Manager not found.");

        existing.FullName = managerDTO.FullName;

        var u = await _userManager.FindByIdAsync(existing.UserID);
        if (u != null)
        {
            u.UserName = managerDTO.UserName;
            u.Email = managerDTO.Email;
            await _userManager.UpdateAsync(u);
        }

        await _tenantContext.SaveChangesAsync();
    }

    public async Task DeleteManager(int id)
    {
        if (UseMasterManagerCatalog())
        {
            if (!TryDecodeCrossTenantManagerId(id, out var tenantId, out var localId))
                throw new InvalidOperationException("Invalid manager id for cross-tenant delete.");

            await using var db = await CreateTenantDbForTenantIdAsync(tenantId);
            var manager = await db.Managers.FirstOrDefaultAsync(m => m.ManagerID == localId);
            if (manager == null)
                throw new InvalidOperationException("Manager not found.");

            await _userRepository.DeleteAsync(manager.UserID);
            db.Managers.Remove(manager);
            await db.SaveChangesAsync();
            return;
        }

        var row = await _tenantContext.Managers.FirstOrDefaultAsync(m => m.ManagerID == id);
        if (row == null)
            throw new InvalidOperationException("Manager not found.");

        await _userRepository.DeleteAsync(row.UserID);
        _tenantContext.Managers.Remove(row);
        await _tenantContext.SaveChangesAsync();
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
}
