using Backend.DTOS;
using Backend.Models.Master;

namespace Backend.Interfaces;

public interface ITenantMembershipService
{
    Task<IReadOnlyList<UserTenantSummaryDto>> GetTenantSummariesAsync(string userId, CancellationToken cancellationToken = default);

    Task<int?> GetDefaultTenantIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<UserTenant?> GetMembershipAsync(string userId, int tenantId, CancellationToken cancellationToken = default);

    /// <summary>Single tenant → id; multiple → latest LastAccessedUtc; if several and none touched → null (caller should require select-tenant).</summary>
    Task<int?> ResolveTenantIdForIssuedTokenAsync(string userId, CancellationToken cancellationToken = default);
}
