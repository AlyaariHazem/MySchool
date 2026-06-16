using Microsoft.AspNetCore.Identity;
using MySchool.Contracts.Authorization;
using MySchool.Contracts.Internal;
using MySchool.IdentityService.Entities;
using MySchool.IdentityService.Interfaces;
using MySchool.IdentityService.Services;

namespace MySchool.IdentityService.Features.Auth.Login;

public sealed class LoginHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMonolithIntegrationClient _monolith;
    private readonly IPermissionClaimService _permissionClaimService;
    private readonly IUserClaimsBuilder _claimsBuilder;
    private readonly IJwtTokenFactory _jwtTokenFactory;
    private readonly IRefreshTokenService _refreshTokenService;

    public LoginHandler(
        UserManager<ApplicationUser> userManager,
        IMonolithIntegrationClient monolith,
        IPermissionClaimService permissionClaimService,
        IUserClaimsBuilder claimsBuilder,
        IJwtTokenFactory jwtTokenFactory,
        IRefreshTokenService refreshTokenService)
    {
        _userManager = userManager;
        _monolith = monolith;
        _permissionClaimService = permissionClaimService;
        _claimsBuilder = claimsBuilder;
        _jwtTokenFactory = jwtTokenFactory;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<LoginResponse> HandleAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.UserName)
            || string.IsNullOrWhiteSpace(command.Password)
            || string.IsNullOrWhiteSpace(command.UserType))
        {
            return new LoginResponse { Success = false, ErrorMessage = "Invalid request data." };
        }

        var user = await _userManager.FindByNameAsync(command.UserName);
        if (user == null || !await _userManager.CheckPasswordAsync(user, command.Password))
            return new LoginResponse { Success = false, ErrorMessage = "Invalid username or password" };

        if (!string.Equals(user.UserType, command.UserType, StringComparison.OrdinalIgnoreCase))
            return new LoginResponse { Success = false, ErrorMessage = "Unauthorized: User type does not match." };

        var enrichment = string.Equals(user.UserType, "ADMIN", StringComparison.OrdinalIgnoreCase)
            ? new LoginEnrichmentResponseDto()
            : await _monolith.GetLoginEnrichmentAsync(user.Id, user.UserType, command.TenantId, cancellationToken);

        if (enrichment.TenantId.HasValue)
            await _monolith.TouchTenantAccessAsync(user.Id, enrichment.TenantId.Value, cancellationToken);

        var userRoles = await _userManager.GetRolesAsync(user);
        if (user.UserType == "ADMIN" && !userRoles.Contains("ADMIN"))
        {
            await _userManager.AddToRoleAsync(user, "ADMIN");
            userRoles = await _userManager.GetRolesAsync(user);
        }

        var claims = await _claimsBuilder.BuildBaseClaimsAsync(user, userRoles, enrichment.TenantId, cancellationToken);
        claims.AddRange(await _permissionClaimService.BuildPermissionClaimsAsync(
            user.Id, user.UserType, enrichment.TenantId, cancellationToken));

        var (accessToken, accessExpiry) = _jwtTokenFactory.CreateAccessToken(claims, TimeSpan.FromDays(30));
        var (rawRefresh, refreshExpires) = await _refreshTokenService.IssueRefreshTokenAsync(user.Id, cancellationToken);

        var permissionsList = claims.Where(c => c.Type == PagePermissionNames.ClaimType).Select(c => c.Value).ToList();
        var schoolRoleClaim = claims.FirstOrDefault(c => c.Type == PagePermissionNames.SchoolRoleClaimType)?.Value;

        if (user.UserType == "ADMIN")
        {
            return new LoginResponse
            {
                Success = true,
                UserName = user.UserName,
                Token = accessToken,
                Expiration = accessExpiry,
                Permissions = permissionsList,
                SchoolRole = schoolRoleClaim,
                RefreshToken = rawRefresh,
                RefreshTokenExpires = refreshExpires
            };
        }

        return new LoginResponse
        {
            Success = true,
            SchoolName = enrichment.SchoolName,
            ManagerName = enrichment.ManagerName,
            UserName = enrichment.UserName,
            SchoolId = enrichment.SchoolId,
            YearId = enrichment.YearId,
            TenantId = enrichment.TenantId,
            TenantDatabase = enrichment.TenantDatabase,
            Tenants = enrichment.Tenants,
            Token = accessToken,
            Expiration = accessExpiry,
            Permissions = permissionsList,
            SchoolRole = schoolRoleClaim,
            RefreshToken = rawRefresh,
            RefreshTokenExpires = refreshExpires
        };
    }
}
