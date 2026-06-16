namespace MySchool.IdentityService.Features.Auth.Register;

public sealed class RegisterResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public string? Message { get; init; }
}
