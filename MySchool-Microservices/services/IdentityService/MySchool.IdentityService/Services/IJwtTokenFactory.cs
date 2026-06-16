using System.Security.Claims;

namespace MySchool.IdentityService.Services;

public interface IJwtTokenFactory
{
    (string Token, DateTime Expiry) CreateAccessToken(IEnumerable<Claim> claims, TimeSpan lifetime);
}
