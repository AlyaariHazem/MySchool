using System.Security.Claims;

namespace Backend.Interfaces;

public interface IPermissionClaimService
{
    /// <summary>Builds <see cref="PagePermissionNames.ClaimType"/> and <see cref="PagePermissionNames.SchoolRoleClaimType"/> claims for JWT.</summary>
    Task<IReadOnlyList<Claim>> BuildPermissionClaimsAsync(string userId, string? userType, int? tenantId, CancellationToken cancellationToken = default);
}
