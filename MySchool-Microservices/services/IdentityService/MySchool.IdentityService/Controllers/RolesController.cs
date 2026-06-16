using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MySchool.IdentityService.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "ADMIN,MANAGER")]
public sealed class RolesController : ControllerBase
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RolesController(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    [HttpGet]
    public IActionResult List()
    {
        var roles = _roleManager.Roles
            .OrderBy(r => r.Name)
            .Select(r => new { r.Id, r.Name })
            .ToList();
        return Ok(roles);
    }
}
