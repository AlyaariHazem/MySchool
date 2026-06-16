using MySchool.IdentityService.Entities;

namespace MySchool.IdentityService.Services;

public interface IRefreshTokenService
{
    string CreateRandomToken(int bytes = 64);
    string Hash(string rawToken);
    Task<(string RawToken, DateTime Expires)> IssueRefreshTokenAsync(string userId, CancellationToken cancellationToken = default);
    Task<(ApplicationUser User, string NewRawToken, DateTime NewExpires)?> RotateRefreshTokenAsync(
        string rawToken,
        CancellationToken cancellationToken = default);
}
