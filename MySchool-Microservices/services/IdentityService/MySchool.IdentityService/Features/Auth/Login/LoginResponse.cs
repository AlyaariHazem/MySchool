using MySchool.Contracts.Auth;

namespace MySchool.IdentityService.Features.Auth.Login;

public sealed class LoginResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public string? UserName { get; init; }
    public string? Token { get; init; }
    public DateTime? Expiration { get; init; }
    public IReadOnlyList<string> Permissions { get; init; } = Array.Empty<string>();
    public string? SchoolRole { get; init; }
    public string? SchoolName { get; init; }
    public string? ManagerName { get; init; }
    public int? SchoolId { get; init; }
    public int YearId { get; init; }
    public int? TenantId { get; init; }
    public string? TenantDatabase { get; init; }
    public IReadOnlyList<UserTenantSummaryDto>? Tenants { get; init; }
    public string? RefreshToken { get; init; }
    public DateTime? RefreshTokenExpires { get; init; }
}
