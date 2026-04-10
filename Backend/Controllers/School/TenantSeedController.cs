using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>Idempotent demo seed for the current tenant (teacher Hazem + terms الأول/الثاني).</summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "ADMIN,MANAGER")]
public class TenantSeedController : ControllerBase
{
    private readonly TenantDemoDataSeeder _demoDataSeeder;

    public TenantSeedController(TenantDemoDataSeeder demoDataSeeder)
    {
        _demoDataSeeder = demoDataSeeder;
    }

    /// <summary>Creates demo manager (if needed), teacher Hazem with login user, and Arabic terms الأول / الثاني when missing.</summary>
    [HttpPost("demo-teacher-team")]
    public async Task<ActionResult<APIResponse>> SeedDemoTeacherAndTeam(CancellationToken cancellationToken)
    {
        var result = await _demoDataSeeder.SeedAsync(cancellationToken);
        if (!result.Success)
            return BadRequest(APIResponse.Fail(result.Message, HttpStatusCode.BadRequest));

        return Ok(APIResponse.Success(new
        {
            message = result.Message,
            teacherID = result.TeacherId,
            demoTeacherUserName = result.DemoTeacherUserName
        }));
    }
}
