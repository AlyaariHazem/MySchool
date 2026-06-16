using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MySchool.Contracts.Authorization;
using MySchool.IdentityService.Entities;
using MySchool.IdentityService.Interfaces;

namespace MySchool.IdentityService.Services;

public sealed class UserClaimsBuilder : IUserClaimsBuilder
{
    private readonly IMonolithIntegrationClient _monolith;

    public UserClaimsBuilder(IMonolithIntegrationClient monolith)
    {
        _monolith = monolith;
    }

    public async Task<List<Claim>> BuildBaseClaimsAsync(
        ApplicationUser user,
        IList<string> userRoles,
        int? tenantId,
        CancellationToken cancellationToken = default)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new("UserType", user.UserType ?? string.Empty)
        };

        if (string.Equals(user.UserType, "ADMIN", StringComparison.OrdinalIgnoreCase))
            claims.Add(new Claim(PlatformAdminHelper.TenantBypassClaimType, PlatformAdminHelper.TenantBypassClaimValue));

        foreach (var role in userRoles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        if (user.UserType == "ADMIN" && !claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "ADMIN"))
            claims.Add(new Claim(ClaimTypes.Role, "ADMIN"));

        if (string.Equals(user.UserType, "TEACHER", StringComparison.OrdinalIgnoreCase)
            && !claims.Any(c => c.Type == ClaimTypes.Role && string.Equals(c.Value, "TEACHER", StringComparison.OrdinalIgnoreCase)))
            claims.Add(new Claim(ClaimTypes.Role, "TEACHER"));

        if (tenantId.HasValue)
        {
            claims.Add(new Claim("TenantId", tenantId.Value.ToString()));
            var summaries = await _monolith.GetTenantSummariesAsync(user.Id, cancellationToken);
            var match = summaries.FirstOrDefault(t => t.TenantId == tenantId.Value);
            if (match != null)
                claims.Add(new Claim("TenantRole", ((int)match.TenantRole).ToString()));
        }

        return claims;
    }
}
