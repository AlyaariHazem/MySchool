using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MySchool.Contracts;

namespace MySchool.IdentityService.Services;

public sealed class JwtTokenFactory : IJwtTokenFactory
{
    private readonly IConfiguration _config;

    public JwtTokenFactory(IConfiguration config)
    {
        _config = config;
    }

    public (string Token, DateTime Expiry) CreateAccessToken(IEnumerable<Claim> claims, TimeSpan lifetime)
    {
        var jwt = _config.GetSection("JWT").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings are not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.Add(lifetime);
        var token = new JwtSecurityToken(
            issuer: jwt.IssuerIP,
            audience: jwt.AudienceIP,
            claims: claims,
            expires: expiry,
            signingCredentials: creds);
        return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
    }
}
