using System.Security.Claims;
using Backend.Common;
using Backend.Data;
using Backend.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class PermissionClaimService : IPermissionClaimService
{
    private readonly DatabaseContext _db;
    private readonly ISchoolRoleResolver _resolver;

    public PermissionClaimService(DatabaseContext db, ISchoolRoleResolver resolver)
    {
        _db = db;
        _resolver = resolver;
    }

    public async Task<IReadOnlyList<Claim>> BuildPermissionClaimsAsync(
        string userId,
        string? userType,
        int? tenantId,
        CancellationToken cancellationToken = default)
    {
        var list = new List<Claim>();

        if (string.Equals(userType, "ADMIN", StringComparison.OrdinalIgnoreCase))
        {
            list.Add(new Claim(PagePermissionNames.SchoolRoleClaimType, SchoolUserRoleKeys.SystemAdmin));
            foreach (var p in PagePermissionNames.All)
                list.Add(new Claim(PagePermissionNames.ClaimType, p));
            return list;
        }

        if (!tenantId.HasValue)
            return list;

        var roleKey = await _resolver.ResolveSchoolRoleKeyAsync(userId, tenantId.Value, cancellationToken);
        if (string.IsNullOrEmpty(roleKey))
            return list;

        list.Add(new Claim(PagePermissionNames.SchoolRoleClaimType, roleKey));

        var names = await _db.RolePermissions.AsNoTracking()
            .Where(rp => rp.RoleName == roleKey && rp.IsAllowed)
            .Join(_db.Permissions.AsNoTracking(), rp => rp.PermissionId, p => p.Id, (_, p) => p.Name)
            .ToListAsync(cancellationToken);

        foreach (var n in names.Distinct(StringComparer.OrdinalIgnoreCase))
            list.Add(new Claim(PagePermissionNames.ClaimType, n));

        return list;
    }
}
