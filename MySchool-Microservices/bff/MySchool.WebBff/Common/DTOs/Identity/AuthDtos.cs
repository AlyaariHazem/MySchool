using GrpcLoginResponse = MySchool.Identity.Grpc.LoginResponse;

namespace MySchool.WebBff.Common.DTOs.Identity;

public static class LoginResponseMapper
{
    public static object ToJson(GrpcLoginResponse result)
    {
        if (result.Tenants.Count == 0 && string.IsNullOrEmpty(result.SchoolName))
        {
            return new AdminLoginResponseDto
            {
                UserName = result.UserName,
                Token = result.Token,
                Expiration = result.Expiration,
                Permissions = result.Permissions,
                SchoolRole = result.SchoolRole
            };
        }

        return new EnrichedLoginResponseDto
        {
            SchoolName = result.SchoolName,
            ManagerName = result.ManagerName,
            UserName = result.UserName,
            SchoolId = result.SchoolId == 0 ? null : result.SchoolId,
            YearId = result.YearId,
            TenantId = result.TenantId == 0 ? null : result.TenantId,
            TenantDatabase = result.TenantDatabase,
            Tenants = result.Tenants.Select(t => new TenantSummaryDto
            {
                TenantId = t.TenantId,
                SchoolName = t.SchoolName,
                TenantRole = t.TenantRole,
                IsDefaultSuggestion = t.IsDefaultSuggestion
            }),
            Token = result.Token,
            Expiration = result.Expiration,
            Permissions = result.Permissions,
            SchoolRole = result.SchoolRole
        };
    }
}

public sealed class AdminLoginResponseDto
{
    public string UserName { get; init; } = default!;
    public string Token { get; init; } = default!;
    public string Expiration { get; init; } = default!;
    public IEnumerable<string> Permissions { get; init; } = Array.Empty<string>();
    public string SchoolRole { get; init; } = default!;
}

public sealed class EnrichedLoginResponseDto
{
    public string SchoolName { get; init; } = default!;
    public string ManagerName { get; init; } = default!;
    public string UserName { get; init; } = default!;
    public int? SchoolId { get; init; }
    public int YearId { get; init; }
    public int? TenantId { get; init; }
    public string TenantDatabase { get; init; } = default!;
    public IEnumerable<TenantSummaryDto> Tenants { get; init; } = Array.Empty<TenantSummaryDto>();
    public string Token { get; init; } = default!;
    public string Expiration { get; init; } = default!;
    public IEnumerable<string> Permissions { get; init; } = Array.Empty<string>();
    public string SchoolRole { get; init; } = default!;
}

public sealed class TenantSummaryDto
{
    public int TenantId { get; init; }
    public string SchoolName { get; init; } = default!;
    public int TenantRole { get; init; }
    public bool IsDefaultSuggestion { get; init; }
}

public sealed class RegisterResponseDto
{
    public string Message { get; init; } = default!;
}

public sealed class RefreshTokenResponseDto
{
    public string Token { get; init; } = default!;
    public string Expiration { get; init; } = default!;
}
