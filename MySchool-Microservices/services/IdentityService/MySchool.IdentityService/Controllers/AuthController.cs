using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySchool.Contracts;
using MySchool.Contracts.Auth;
using MySchool.IdentityService.Data;
using MySchool.IdentityService.Entities;
using MySchool.IdentityService.Features.Auth.Login;
using MySchool.IdentityService.Features.Auth.RefreshToken;
using MySchool.IdentityService.Features.Auth.Register;
using MySchool.IdentityService.Interfaces;
using MySchool.IdentityService.Services;

namespace MySchool.IdentityService.Controllers;

[Route("api/[controller]")]
[ApiController]
public partial class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityDbContext _db;
    private readonly IMonolithIntegrationClient _monolith;
    private readonly IPermissionClaimService _permissionClaimService;
    private readonly IUserClaimsBuilder _claimsBuilder;
    private readonly IJwtTokenFactory _jwtTokenFactory;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly LoginHandler _loginHandler;
    private readonly RegisterHandler _registerHandler;
    private readonly RefreshTokenHandler _refreshTokenHandler;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IdentityDbContext db,
        IMonolithIntegrationClient monolith,
        IPermissionClaimService permissionClaimService,
        IUserClaimsBuilder claimsBuilder,
        IJwtTokenFactory jwtTokenFactory,
        IRefreshTokenService refreshTokenService,
        LoginHandler loginHandler,
        RegisterHandler registerHandler,
        RefreshTokenHandler refreshTokenHandler,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _db = db;
        _monolith = monolith;
        _permissionClaimService = permissionClaimService;
        _claimsBuilder = claimsBuilder;
        _jwtTokenFactory = jwtTokenFactory;
        _refreshTokenService = refreshTokenService;
        _loginHandler = loginHandler;
        _registerHandler = registerHandler;
        _refreshTokenHandler = refreshTokenHandler;
        _logger = logger;
    }

    [HttpPost("Register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid request data." });

        var result = await _registerHandler.HandleAsync(new RegisterCommand
        {
            UserName = request.UserName,
            Password = request.Password,
            Email = request.Email,
            UserType = request.UserType
        });

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage ?? "Registration failed." });

        return Ok(new { message = result.Message });
    }

    [HttpPost("Login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _loginHandler.HandleAsync(new LoginCommand
        {
            UserName = request.UserName,
            Password = request.Password,
            UserType = request.userType,
            TenantId = request.TenantId
        });

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        if (!string.IsNullOrEmpty(result.RefreshToken) && result.RefreshTokenExpires.HasValue)
            Response.Cookies.Append("refreshToken", result.RefreshToken, BuildCookieOptions(result.RefreshTokenExpires.Value));

        if (string.IsNullOrEmpty(result.SchoolName) && (result.Tenants is null || result.Tenants.Count == 0))
        {
            return Ok(new
            {
                userName = result.UserName,
                token = result.Token,
                expiration = result.Expiration,
                permissions = result.Permissions,
                schoolRole = result.SchoolRole
            });
        }

        return Ok(new
        {
            schoolName = result.SchoolName,
            managerName = result.ManagerName,
            userName = result.UserName,
            schoolId = result.SchoolId,
            yearId = result.YearId,
            tenantId = result.TenantId,
            tenantDatabase = result.TenantDatabase,
            tenants = result.Tenants,
            token = result.Token,
            expiration = result.Expiration,
            permissions = result.Permissions,
            schoolRole = result.SchoolRole
        });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh()
    {
        var rawToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrWhiteSpace(rawToken))
            return Unauthorized();

        var result = await _refreshTokenHandler.HandleAsync(new RefreshTokenCommand { RefreshToken = rawToken });
        if (!result.Success)
            return Unauthorized();

        if (!string.IsNullOrEmpty(result.RefreshToken) && result.RefreshTokenExpires.HasValue)
            Response.Cookies.Append("refreshToken", result.RefreshToken, BuildCookieOptions(result.RefreshTokenExpires.Value));

        return Ok(new { token = result.Token, expiration = result.Expiration });
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
        var claims = await _claimsBuilder.BuildBaseClaimsAsync(user, userRoles, dto.TenantId);
        claims.AddRange(await _permissionClaimService.BuildPermissionClaimsAsync(user.Id, user.UserType, dto.TenantId));

        var (accessToken, accessExpiry) = _jwtTokenFactory.CreateAccessToken(claims, TimeSpan.FromMinutes(15));
        return Ok(new { token = accessToken, tenantId = dto.TenantId, expiration = accessExpiry });
    }

    private static CookieOptions BuildCookieOptions(DateTime expires) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
        Expires = expires
    };
}
