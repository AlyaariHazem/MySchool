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

    /* ----------  POST  ---------- */
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MonthlyGradeDTO dto)
    {
        var result = await _repo.AddAsync(dto);

        return result.Ok
            ? CreatedAtAction(nameof(GetAll),          // no single-item GET, so point to list
                              new
                              {
                                  term = dto.TermID,
                                  monthId = dto.MonthID,
                                  classId = dto.ClassID,
                                  subjectId = dto.SubjectID
                              },
                              APIResponse.Success(result.Value!, HttpStatusCode.Created))
            : BadRequest(APIResponse.Fail(result.Error!));
    }

    /* ----------  GET LIST  ---------- */
    [HttpGet("{term:int}/{monthId:int}/{classId:int}/{subjectId:int}")]
    public async Task<IActionResult> GetAll(int term, int monthId, int classId, int subjectId)
    {
        var result = await _repo.GetAllAsync(term, monthId, classId, subjectId);

        return result.Ok
            ? Ok(APIResponse.Success(result.Value!))
            : NotFound(APIResponse.Fail(result.Error!));
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
