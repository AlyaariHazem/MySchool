using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MySchool.IdentityService.Features.Roles.GetRoles;

namespace MySchool.IdentityService.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "ADMIN,MANAGER")]
public sealed class RolesController : ControllerBase
{
    private readonly GetRolesHandler _getRolesHandler;

    public RolesController(GetRolesHandler getRolesHandler)
    {
        _getRolesHandler = getRolesHandler;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var result = await _getRolesHandler.HandleAsync(new GetRolesQuery());
        var roles = result.Roles
            .Select(r => new { Id = r.Id, Name = r.Name })
            .ToList();
        return Ok(roles);
    }
}
