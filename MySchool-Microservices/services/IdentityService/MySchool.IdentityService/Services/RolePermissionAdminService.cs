using MySchool.Contracts.Authorization;
using MySchool.Contracts.Permissions;
using MySchool.IdentityService.Data;
using MySchool.IdentityService.Entities;
using Microsoft.EntityFrameworkCore;

namespace MySchool.IdentityService.Services;

public sealed class RolePermissionAdminService
{
    private readonly IdentityDbContext _db;

    public RolePermissionAdminService(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task<RolePermissionMatrixDto> GetMatrixAsync(CancellationToken cancellationToken = default)
    {
        var perms = await _db.Permissions.AsNoTracking()
            .OrderBy(p => p.Page)
            .ThenBy(p => p.Action)
            .Select(p => new PermissionItemDto { Name = p.Name, Page = p.Page, Action = p.Action })
            .ToListAsync(cancellationToken);

        var cells = await _db.RolePermissions.AsNoTracking()
            .Join(_db.Permissions.AsNoTracking(), rp => rp.PermissionId, p => p.Id,
                (rp, p) => new { rp.RoleName, Name = p.Name, rp.IsAllowed })
            .ToListAsync(cancellationToken);

        var matrix = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in cells)
            matrix[$"{c.RoleName}|{c.Name}"] = c.IsAllowed;

        return new RolePermissionMatrixDto
        {
            Roles = SchoolUserRoleKeys.AllRoles.ToList(),
            Permissions = perms,
            Matrix = matrix
        };
    }

    public async Task SaveMatrixAsync(RolePermissionMatrixUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var permIds = await _db.Permissions.AsNoTracking()
            .ToDictionaryAsync(p => p.Name, p => p.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var cell in dto.Cells)
        {
            var roleName = ResolveRoleName(dto.ScopeToRoleName, cell.RoleName);
            if (string.IsNullOrWhiteSpace(roleName))
                continue;

            if (!permIds.TryGetValue(cell.PermissionName, out var pid))
                continue;

            var row = await _db.RolePermissions
                .FirstOrDefaultAsync(
                    rp => rp.RoleName == roleName && rp.PermissionId == pid,
                    cancellationToken);
            if (row == null)
            {
                _db.RolePermissions.Add(new RolePermission
                {
                    RoleName = roleName,
                    PermissionId = pid,
                    IsAllowed = cell.IsAllowed
                });
            }
            else
            {
                row.IsAllowed = cell.IsAllowed;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static string? ResolveRoleName(string? scopeToRoleName, string? cellRoleName)
    {
        if (!string.IsNullOrWhiteSpace(scopeToRoleName))
        {
            if (!string.IsNullOrWhiteSpace(cellRoleName)
                && !string.Equals(cellRoleName, scopeToRoleName, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"Cell RoleName '{cellRoleName}' does not match ScopeToRoleName '{scopeToRoleName}'.");
            return scopeToRoleName;
        }

        return string.IsNullOrWhiteSpace(cellRoleName) ? null : cellRoleName;
    }
}
