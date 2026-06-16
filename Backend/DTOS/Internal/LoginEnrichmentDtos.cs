using Backend.DTOS;
using Backend.Models.Master;

namespace Backend.DTOS.Internal;

public sealed class LoginEnrichmentRequestDto
{
    public string UserId { get; set; } = default!;
    public string UserType { get; set; } = default!;
    public int? RequestedTenantId { get; set; }
}

public sealed class LoginEnrichmentResponseDto
{
    public string? SchoolName { get; set; }
    public string? ManagerName { get; set; }
    public string? UserName { get; set; }
    public int? SchoolId { get; set; }
    public int YearId { get; set; } = 1;
    public int? TenantId { get; set; }
    public string? TenantDatabase { get; set; }
    public TenantRole? MembershipTenantRole { get; set; }
    public IReadOnlyList<UserTenantSummaryDto>? Tenants { get; set; }
}

public sealed class TouchTenantAccessRequestDto
{
    public string UserId { get; set; } = default!;
    public int TenantId { get; set; }
}

public sealed class EnsureUserTenantRequestDto
{
    public string UserId { get; set; } = default!;
    public int TenantId { get; set; }
    public TenantRole TenantRole { get; set; }
}
