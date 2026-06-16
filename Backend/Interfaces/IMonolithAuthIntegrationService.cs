using Backend.DTOS;
using Backend.DTOS.Internal;
using Backend.Models.Master;

namespace Backend.Interfaces;

public interface IMonolithAuthIntegrationService
{
    Task<LoginEnrichmentResponseDto> GetLoginEnrichmentAsync(
        string userId,
        string userType,
        int? requestedTenantId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserTenantSummaryDto>> GetTenantSummariesAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<UserTenant?> GetMembershipAsync(
        string userId,
        int tenantId,
        CancellationToken cancellationToken = default);

    Task TouchTenantAccessAsync(
        string userId,
        int tenantId,
        CancellationToken cancellationToken = default);

    Task EnsureUserTenantAsync(
        string userId,
        int tenantId,
        TenantRole role,
        CancellationToken cancellationToken = default);

    Task<int?> ResolveTenantIdForLoginAsync(
        string userId,
        string? userType,
        CancellationToken cancellationToken = default);

    Task<string?> ResolveSchoolRoleKeyAsync(
        string userId,
        int tenantId,
        CancellationToken cancellationToken = default);

    Task<int?> ResolveTenantIdForTeacherStudentGuardianAsync(
        string userId,
        string userType,
        CancellationToken cancellationToken = default);
}
