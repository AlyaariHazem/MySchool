namespace MySchool.IdentityService.Features.Auth.Login;

public sealed class LoginCommand
{
    public string UserName { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string UserType { get; init; } = default!;
    public int? TenantId { get; init; }
}
