using Backend.Common;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.HasClaim(PagePermissionNames.ClaimType, requirement.PermissionName))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (context.User.IsInRole("ADMIN"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (context.User.HasClaim(PlatformAdminHelper.TenantBypassClaimType, PlatformAdminHelper.TenantBypassClaimValue))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}
