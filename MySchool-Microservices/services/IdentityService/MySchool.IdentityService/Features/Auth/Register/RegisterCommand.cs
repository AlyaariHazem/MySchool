namespace MySchool.IdentityService.Features.Auth.Register;

public sealed class RegisterCommand
{
    public string UserName { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string UserType { get; init; } = "Admin";
}
