namespace MySchool.IdentityService.Features.Auth.RefreshToken;

public sealed class RefreshTokenCommand
{
    public string RefreshToken { get; init; } = default!;
}
