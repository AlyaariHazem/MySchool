using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MySchool.Contracts;
using MySchool.Contracts.Auth;
using MySchool.Contracts.Authorization;
using MySchool.IdentityService.Data;
using MySchool.IdentityService.Entities;
using MySchool.IdentityService.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace MySchool.IdentityService.Controllers;

[Route("api/[controller]")]
[ApiController]
public partial class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityDbContext _db;
    private readonly IConfiguration _config;
    private readonly IMonolithIntegrationClient _monolith;
    private readonly IPermissionClaimService _permissionClaimService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IdentityDbContext db,
        IConfiguration config,
        IMonolithIntegrationClient monolith,
        IPermissionClaimService permissionClaimService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _db = db;
        _config = config;
        _monolith = monolith;
        _permissionClaimService = permissionClaimService;
        _logger = logger;
    }

    [HttpPost("Register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid request data." });

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            UserType = request.UserType
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { message = "Registration failed.", errors = result.Errors });

        if (!string.IsNullOrEmpty(request.UserType))
            await _userManager.AddToRoleAsync(user, request.UserType);

        return Ok(new { message = "User created successfully." });
    }

    [HttpPost("Login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return BadRequest(new { message = "Invalid username or password" });

        if (!string.Equals(user.UserType, request.userType, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Unauthorized: User type does not match." });

        var enrichment = string.Equals(user.UserType, "ADMIN", StringComparison.OrdinalIgnoreCase)
            ? new MySchool.Contracts.Internal.LoginEnrichmentResponseDto()
            : await _monolith.GetLoginEnrichmentAsync(
                user.Id,
                user.UserType,
                request.TenantId);

        if (enrichment.TenantId.HasValue)
            await _monolith.TouchTenantAccessAsync(user.Id, enrichment.TenantId.Value);

        var userRoles = await _userManager.GetRolesAsync(user);
        if (user.UserType == "ADMIN" && !userRoles.Contains("ADMIN"))
        {
            await _userManager.AddToRoleAsync(user, "ADMIN");
            userRoles = await _userManager.GetRolesAsync(user);
        }

        var claims = await BuildBaseClaimsAsync(user, userRoles, enrichment.TenantId);
        var permClaims = await _permissionClaimService.BuildPermissionClaimsAsync(
            user.Id, user.UserType, enrichment.TenantId);
        claims.AddRange(permClaims);

        var (accessToken, accessExpiry) = CreateAccessToken(claims, TimeSpan.FromDays(30));

        var rawRefresh = CreateRandomToken();
        _db.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = Hash(rawRefresh),
            Expires = DateTime.UtcNow.AddDays(7),
            UserId = user.Id
        });
        await _db.SaveChangesAsync();
        Response.Cookies.Append("refreshToken", rawRefresh, BuildCookieOptions(DateTime.UtcNow.AddDays(7)));

        var permissionsList = claims.Where(c => c.Type == PagePermissionNames.ClaimType).Select(c => c.Value).ToList();
        var schoolRoleClaim = claims.FirstOrDefault(c => c.Type == PagePermissionNames.SchoolRoleClaimType)?.Value;

        if (user.UserType == "ADMIN")
        {
            return Ok(new
            {
                userName = user.UserName,
                token = accessToken,
                expiration = accessExpiry,
                permissions = permissionsList,
                schoolRole = schoolRoleClaim
            });
        }

        return Ok(new
        {
            schoolName = enrichment.SchoolName,
            managerName = enrichment.ManagerName,
            userName = enrichment.UserName,
            schoolId = enrichment.SchoolId,
            yearId = enrichment.YearId,
            tenantId = enrichment.TenantId,
            tenantDatabase = enrichment.TenantDatabase,
            tenants = enrichment.Tenants,
            token = accessToken,
            expiration = accessExpiry,
            permissions = permissionsList,
            schoolRole = schoolRoleClaim
        });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh()
    {
        var rawToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrWhiteSpace(rawToken))
            return Unauthorized();

        var hash = Hash(rawToken);
        var token = await _db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash && t.Revoked == null);

        if (token is null || token.Expires < DateTime.UtcNow)
            return Unauthorized();

        token.Revoked = DateTime.UtcNow;
        var newRaw = CreateRandomToken();
        var newToken = new RefreshToken
        {
            TokenHash = Hash(newRaw),
            Expires = DateTime.UtcNow.AddDays(7),
            UserId = token.UserId
        };
        _db.RefreshTokens.Add(newToken);
        await _db.SaveChangesAsync();
        Response.Cookies.Append("refreshToken", newRaw, BuildCookieOptions(newToken.Expires));

        var enrichment = await _monolith.GetLoginEnrichmentAsync(token.User.Id, token.User.UserType);
        var userRoles = await _userManager.GetRolesAsync(token.User);
        var claims = await BuildBaseClaimsAsync(token.User, userRoles, enrichment.TenantId);
        claims.AddRange(await _permissionClaimService.BuildPermissionClaimsAsync(
            token.User.Id, token.User.UserType, enrichment.TenantId));

        var (accessToken, accessExpiry) = CreateAccessToken(claims, TimeSpan.FromMinutes(15));
        return Ok(new { token = accessToken, expiration = accessExpiry });
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        await TryRevokeRefreshTokensAsync(userId);
        ClearRefreshTokenCookie();

        return Ok(new { message = "Logged out on all devices" });
    }

    private async Task TryRevokeRefreshTokensAsync(string userId)
    {
        try
        {
            var tokens = await _db.RefreshTokens.Where(t => t.UserId == userId).ToListAsync();
            if (tokens.Count > 0)
            {
                _db.RefreshTokens.RemoveRange(tokens);
                await _db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            // Cookie + client token cleanup still succeed; DB may be offline during dev.
            _logger.LogWarning(ex, "Could not revoke refresh tokens for user {UserId}.", userId);
        }
    }

    private void ClearRefreshTokenCookie()
    {
        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UnixEpoch
        });
    }

    [HttpGet("my-tenants")]
    [Authorize]
    public async Task<IActionResult> MyTenants()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var list = await _monolith.GetTenantSummariesAsync(userId);
        if (list.Count == 0)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && !string.Equals(user.UserType, "ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                await _monolith.GetLoginEnrichmentAsync(userId, user.UserType);
                list = await _monolith.GetTenantSummariesAsync(userId);
            }
        }

        return Ok(list);
    }

    [HttpPost("select-tenant")]
    [Authorize]
    public async Task<IActionResult> SelectTenant([FromBody] SelectTenantDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var mem = await _monolith.GetMembershipAsync(userId, dto.TenantId);
        if (mem == null)
        {
            var userForTenant = await _userManager.FindByIdAsync(userId);
            if (userForTenant == null)
                return Unauthorized();

            await _monolith.GetLoginEnrichmentAsync(userId, userForTenant.UserType, dto.TenantId);
            mem = await _monolith.GetMembershipAsync(userId, dto.TenantId);
        }

        if (mem == null)
            return Forbid();

        await _monolith.TouchTenantAccessAsync(userId, dto.TenantId);

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        var userRoles = await _userManager.GetRolesAsync(user);
        var claims = await BuildBaseClaimsAsync(user, userRoles, dto.TenantId);
        claims.AddRange(await _permissionClaimService.BuildPermissionClaimsAsync(user.Id, user.UserType, dto.TenantId));

        var (accessToken, accessExpiry) = CreateAccessToken(claims, TimeSpan.FromMinutes(15));
        return Ok(new { token = accessToken, tenantId = dto.TenantId, expiration = accessExpiry });
    }

    private async Task<List<Claim>> BuildBaseClaimsAsync(
        ApplicationUser user,
        IList<string> userRoles,
        int? tenantId)
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
            var summaries = await _monolith.GetTenantSummariesAsync(user.Id);
            var match = summaries.FirstOrDefault(t => t.TenantId == tenantId.Value);
            if (match != null)
                claims.Add(new Claim("TenantRole", ((int)match.TenantRole).ToString()));
        }

        return claims;
    }

    private (string Token, DateTime Expiry) CreateAccessToken(IEnumerable<Claim> claims, TimeSpan lifetime)
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

    private static string CreateRandomToken(int bytes = 64) =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(bytes));

    private static string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }

    private static CookieOptions BuildCookieOptions(DateTime expires) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
        Expires = expires
    };
}
