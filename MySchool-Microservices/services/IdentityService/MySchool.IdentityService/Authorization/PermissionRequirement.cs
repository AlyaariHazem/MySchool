using Microsoft.AspNetCore.Authorization;

namespace MySchool.IdentityService.Authorization;

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public string PermissionName { get; }

    public PermissionRequirement(string permissionName) =>
        PermissionName = permissionName ?? throw new ArgumentNullException(nameof(permissionName));
}
