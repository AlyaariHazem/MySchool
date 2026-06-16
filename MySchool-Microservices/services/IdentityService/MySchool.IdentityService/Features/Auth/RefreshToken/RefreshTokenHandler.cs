using Microsoft.AspNetCore.Identity;
using MySchool.IdentityService.Entities;
using MySchool.IdentityService.Interfaces;
using MySchool.IdentityService.Services;

namespace MySchool.IdentityService.Features.Auth.RefreshToken;

public sealed class RefreshTokenHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMonolithIntegrationClient _monolith;
    private readonly IPermissionClaimService _permissionClaimService;
    private readonly IUserClaimsBuilder _claimsBuilder;
    private readonly IJwtTokenFactory _jwtTokenFactory;
    private readonly IRefreshTokenService _refreshTokenService;

    public RefreshTokenHandler(
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

    public async Task<RefreshTokenResponse> HandleAsync(RefreshTokenCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
            return new RefreshTokenResponse { Success = false, ErrorMessage = "Refresh token is required." };

        var rotated = await _refreshTokenService.RotateRefreshTokenAsync(command.RefreshToken, cancellationToken);
        if (rotated is null)
            return new RefreshTokenResponse { Success = false, ErrorMessage = "Invalid or expired refresh token." };

        var (user, newRaw, newExpires) = rotated.Value;
        var enrichment = await _monolith.GetLoginEnrichmentAsync(user.Id, user.UserType, cancellationToken: cancellationToken);
        var userRoles = await _userManager.GetRolesAsync(user);
        var claims = await _claimsBuilder.BuildBaseClaimsAsync(user, userRoles, enrichment.TenantId, cancellationToken);
        claims.AddRange(await _permissionClaimService.BuildPermissionClaimsAsync(
            user.Id, user.UserType, enrichment.TenantId, cancellationToken));

        var (accessToken, accessExpiry) = _jwtTokenFactory.CreateAccessToken(claims, TimeSpan.FromMinutes(15));

        return new RefreshTokenResponse
        {
            Success = true,
            Token = accessToken,
            Expiration = accessExpiry,
            RefreshToken = newRaw,
            RefreshTokenExpires = newExpires
        };
    }
}
