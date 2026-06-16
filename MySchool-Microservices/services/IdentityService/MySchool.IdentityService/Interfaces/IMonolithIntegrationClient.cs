using MySchool.Contracts.Auth;
using MySchool.Contracts.Internal;

namespace MySchool.IdentityService.Interfaces;

public interface IMonolithIntegrationClient
{
    Task<LoginEnrichmentResponseDto> GetLoginEnrichmentAsync(
        string userId,
        string userType,
        int? requestedTenantId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserTenantSummaryDto>> GetTenantSummariesAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<UserTenantMembershipDto?> GetMembershipAsync(
        string userId,
        int tenantId,
        CancellationToken cancellationToken = default);

    Task TouchTenantAccessAsync(string userId, int tenantId, CancellationToken cancellationToken = default);

    Task EnsureUserTenantAsync(
        string userId,
        int tenantId,
        TenantRole tenantRole,
        CancellationToken cancellationToken = default);

    Task<string?> ResolveSchoolRoleKeyAsync(
        string userId,
        int tenantId,
        CancellationToken cancellationToken = default);

    Task<HttpResponseMessage> ProxyRegistrationGetAsync(
        string relativePath,
        string? authorizationHeader,
        CancellationToken cancellationToken = default);

    Task<HttpResponseMessage> ProxyRegistrationPostAsync(
        string relativePath,
        HttpContent? content,
        string? authorizationHeader,
        CancellationToken cancellationToken = default);
}
