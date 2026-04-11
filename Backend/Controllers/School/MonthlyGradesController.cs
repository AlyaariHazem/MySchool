using System.Net;
using System.Security.Claims;
using Backend.DTOS.School.MonthlyGrade;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class MonthlyGradesController : ControllerBase
{
    private readonly IMonthlyGradeRepository _repo;

    public MonthlyGradesController(IMonthlyGradeRepository repo)
    {
        _repo = repo;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>Monthly grade rows for all students linked to the guardian (optional year / term / month filters).</summary>
    [Authorize(Roles = "GUARDIAN,ADMIN,MANAGER")]
    [HttpGet("guardian/my")]
    public async Task<IActionResult> GetGuardianMy([FromQuery] int? yearId = null, [FromQuery] int? termId = null, [FromQuery] int? monthId = null)
    {
        var uid = CurrentUserId;
        if (string.IsNullOrEmpty(uid))
            return Unauthorized(APIResponse.Fail("User id not found on token."));

        var guardianId = await _repo.GetGuardianIdByUserIdAsync(uid);
        if (!guardianId.HasValue)
            return StatusCode((int)HttpStatusCode.Forbidden, APIResponse.Fail("No guardian profile for this user."));

        var list = await _repo.GetGuardianStudentsMonthlyGradesAsync(guardianId.Value, yearId, termId, monthId);
        return Ok(APIResponse.Success(list));
    }

    //api/MonthlyGrades
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MonthlyGradeDTO dto)
    {
        var result = await _repo.AddAsync(dto);

        return result.Ok
            ? StatusCode((int)HttpStatusCode.Created, APIResponse.Success(result.Value!, HttpStatusCode.Created))
            : BadRequest(APIResponse.Fail(result.Error!));
    }

    /// <summary>Filters and pagination in the JSON body (active grade types only).</summary>
    [HttpPost("page")]
    public async Task<IActionResult> GetAllPage([FromBody] MonthlyGradesQueryDTO query)
    {
        if (query == null)
            return BadRequest(APIResponse.Fail("Request body is required."));

        if (query.PageNumber < 1)
            query.PageNumber = 1;
        if (query.PageSize < 1)
            query.PageSize = 10;

        var result = await _repo.GetAllAsync(query);

        if (!result.Ok)
            return NotFound(APIResponse.Fail(result.Error!));

        var totalCount = await _repo.GetTotalMonthlyGradesCountAsync(query);

        var paginatedResult = new
        {
            data = result.Value ?? new List<MonthlyGradesReternDTO>(),
            pageNumber = query.PageNumber,
            pageSize = query.PageSize,
            totalCount,
            totalPages = query.PageSize > 0 ? (int)Math.Ceiling(totalCount / (double)query.PageSize) : 0
        };

        return Ok(APIResponse.Success(paginatedResult));
    }

    /* ----------  PUT MANY  ---------- */
    [HttpPut("UpdateMany")]
    public async Task<IActionResult> UpdateMany([FromBody] List<MonthlyGradeDTO> dtos)
    {
        var result = await _repo.UpdateManyAsync(dtos);

        return result.Ok
            ? Ok(APIResponse.Success("Monthly grades updated successfully."))
            : BadRequest(APIResponse.Fail(result.Error!));
    }


}
