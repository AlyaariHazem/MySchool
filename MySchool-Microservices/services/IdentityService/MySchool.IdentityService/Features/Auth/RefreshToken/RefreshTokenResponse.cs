namespace MySchool.IdentityService.Features.Auth.RefreshToken;

public sealed class RefreshTokenResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public string? Token { get; init; }
    public DateTime? Expiration { get; init; }
    public string? RefreshToken { get; init; }
    public DateTime? RefreshTokenExpires { get; init; }
}
