using System.Net;
using Backend.DTOS.School.MonthlyGrade;
using Backend.Interfaces;
using Backend.Models;
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
