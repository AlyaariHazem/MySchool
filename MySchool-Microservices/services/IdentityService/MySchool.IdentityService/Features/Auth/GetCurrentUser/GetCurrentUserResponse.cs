using MySchool.Contracts.Users;

namespace MySchool.IdentityService.Features.Auth.GetCurrentUser;

public sealed class GetCurrentUserResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public UserAccountDto? User { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}
