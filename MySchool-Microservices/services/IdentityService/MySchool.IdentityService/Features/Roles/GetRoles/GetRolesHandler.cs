using Microsoft.AspNetCore.Identity;

namespace MySchool.IdentityService.Features.Roles.GetRoles;

public sealed class GetRolesHandler
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public GetRolesHandler(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public Task<GetRolesResponse> HandleAsync(GetRolesQuery query, CancellationToken cancellationToken = default)
    {
        var roles = _roleManager.Roles
            .OrderBy(r => r.Name)
            .Select(r => new RoleItemDto { Id = r.Id, Name = r.Name ?? string.Empty })
            .ToList();

        return Task.FromResult(new GetRolesResponse { Roles = roles });
    }
}
