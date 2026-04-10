using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Backend.Common;

/// <summary>
/// Detects platform-level ADMIN (JWT may use short "role" claims; <see cref="TenantBypassClaimType"/> is explicit).
/// </summary>
public static class PlatformAdminHelper
{
    public const string TenantBypassClaimType = "tenant_bypass";
    public const string TenantBypassClaimValue = "1";

    public static bool IsPlatformAdminUnrestricted(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return false;

        if (user.HasClaim(TenantBypassClaimType, TenantBypassClaimValue))
            return true;

        foreach (var c in user.Claims)
        {
            var isRoleClaim = c.Type == ClaimTypes.Role
                || string.Equals(c.Type, "role", StringComparison.OrdinalIgnoreCase)
                || c.Type.EndsWith("/role", StringComparison.Ordinal);
            if (isRoleClaim && string.Equals(c.Value, "ADMIN", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        if (user.IsInRole("ADMIN"))
            return true;

        var ut = user.FindFirstValue("UserType") ?? user.FindFirst("UserType")?.Value;
        return !string.IsNullOrWhiteSpace(ut)
            && string.Equals(ut.Trim(), "ADMIN", StringComparison.OrdinalIgnoreCase);
    }
}
