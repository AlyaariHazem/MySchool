using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MySchool.IdentityService.Data;
using MySchool.IdentityService.Entities;

namespace MySchool.IdentityService.Services;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly IdentityDbContext _db;

    public RefreshTokenService(IdentityDbContext db)
    {
        _db = db;
    }

    public string CreateRandomToken(int bytes = 64) =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(bytes));

    public string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }

    public async Task<(string RawToken, DateTime Expires)> IssueRefreshTokenAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var raw = CreateRandomToken();
        var expires = DateTime.UtcNow.AddDays(7);
        _db.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = Hash(raw),
            Expires = expires,
            UserId = userId
        });
        await _db.SaveChangesAsync(cancellationToken);
        return (raw, expires);
    }

    public async Task<(ApplicationUser User, string NewRawToken, DateTime NewExpires)?> RotateRefreshTokenAsync(
        string rawToken,
        CancellationToken cancellationToken = default)
    {
        var hash = Hash(rawToken);
        var token = await _db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash && t.Revoked == null, cancellationToken);

        if (token is null || token.Expires < DateTime.UtcNow)
            return null;

        token.Revoked = DateTime.UtcNow;
        var newRaw = CreateRandomToken();
        var newExpires = DateTime.UtcNow.AddDays(7);
        _db.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = Hash(newRaw),
            Expires = newExpires,
            UserId = token.UserId
        });
        await _db.SaveChangesAsync(cancellationToken);
        return (token.User, newRaw, newExpires);
    }
}
