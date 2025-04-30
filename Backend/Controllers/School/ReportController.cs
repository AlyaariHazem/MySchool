using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    public ReportController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    [HttpGet("{yearId:int}/{termId:int}/{monthId:int}/{classId:int}/{divisionId:int}/{studentId:int}")]
    public async Task<IActionResult> MonthlyReports(int yearId, int termId, int monthId, int classId, int divisionId, int studentId = 0)
    {
        var result = await _unitOfWork.Reports.MonthlyReportsAsync(yearId, termId, monthId, classId, divisionId, studentId);
        return result.Ok
       ? Ok(APIResponse.Success(result.Value!))
       : NotFound(APIResponse.Fail(result.Error!));

    }
}
