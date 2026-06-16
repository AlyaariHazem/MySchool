using System.Security.Claims;
using Backend.Common;
using Backend.Interfaces;

namespace Backend.Services;

public class PermissionClaimService : IPermissionClaimService
{
    private readonly ISchoolRoleResolver _resolver;

    public PermissionClaimService(ISchoolRoleResolver resolver)
    {
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
        return list;
    }
}
