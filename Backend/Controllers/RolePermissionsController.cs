using Backend.DTOS.Permissions;
using Backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>Matrix CRUD for page permissions per school role (<see cref="Common.SchoolUserRoleKeys"/>).</summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "ADMIN,MANAGER")]
public class RolePermissionsController : ControllerBase
{
    private readonly IRolePermissionAdminService _service;

    public RolePermissionsController(IRolePermissionAdminService service)
    {
        _service = service;
    }

    [HttpGet("matrix")]
    public async Task<ActionResult<RolePermissionMatrixDto>> GetMatrix(CancellationToken cancellationToken)
    {
        var dto = await _service.GetMatrixAsync(cancellationToken);
        return Ok(dto);
    }

    [HttpPut("matrix")]
    public async Task<IActionResult> SaveMatrix([FromBody] RolePermissionMatrixUpdateDto body, CancellationToken cancellationToken)
    {
        if (body?.Cells == null)
            return BadRequest(new { message = "Cells required." });
        try
        {
            await _service.SaveMatrixAsync(body, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        return NoContent();
    }
}
