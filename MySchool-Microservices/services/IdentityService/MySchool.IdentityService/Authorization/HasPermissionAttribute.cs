using Microsoft.AspNetCore.Authorization;

namespace MySchool.IdentityService.Authorization;

/// <summary>Requires JWT claim <c>permission</c> with this value, or platform ADMIN.</summary>
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission) : base(policy: permission)
    {
    }
}
