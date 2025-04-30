using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Backend.DTOS.School.TermlyGrade;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

[ApiController]
[Route("api/[controller]")]
public class TermlyGradeController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    public TermlyGradeController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    [HttpGet("{termId:int}/{yearId:int}/{classId:int}/{subjectId:int}")]
    public async Task<IActionResult> GetTermlyGrades(int termId, int yearId, int classId, int subjectId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var termlyGrades = await _unitOfWork.TermlyGrades.GetAllAsync(termId, yearId, classId, subjectId, pageNumber, pageSize);

        var totalCount = await _unitOfWork.TermlyGrades.GetTotalMonthlyGradesCountAsync(termId, yearId, classId, subjectId);

        var paginatedResult = new
        {
            data = termlyGrades.Value,
            pageNumber,
            pageSize,
            totalCount,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return termlyGrades.Ok
            ? Ok(APIResponse.Success(paginatedResult))
            : NotFound(APIResponse.Fail(termlyGrades.Error!));
    }
    [HttpPost]
    public async Task<IActionResult> CreateTermlyGrade([FromBody] TermlyGradeDTO termlyGrade)
    {
        if (!ModelState.IsValid)
            return BadRequest(APIResponse.Fail("Invalid termly grade data."));

        var result = await _unitOfWork.TermlyGrades.AddAsync(termlyGrade);

        return result.Ok
            ? StatusCode((int)HttpStatusCode.Created,
                         APIResponse.Success("Termly grade created successfully.", HttpStatusCode.Created))
            : StatusCode((int)HttpStatusCode.InternalServerError,
                         APIResponse.Fail(result.Error!));
    }
    [HttpPut]
    public async Task<IActionResult> UpdateTermlyGrade( [FromBody] TermlyGradeDTO[] termlyGrade)
    {
        if (!ModelState.IsValid)
            return BadRequest(APIResponse.Fail("Invalid termly grade data."));
        var result = await _unitOfWork.TermlyGrades.UpdateAsync(termlyGrade);
        return result.Ok
            ? StatusCode((int)HttpStatusCode.OK,
                         APIResponse.Success("Termly grade updated successfully.", HttpStatusCode.OK))
            : StatusCode((int)HttpStatusCode.InternalServerError,
                         APIResponse.Fail(result.Error!));
    }
}
