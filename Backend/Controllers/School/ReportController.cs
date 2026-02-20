using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.reports;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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

    /// <summary>
    /// Get report template by code. Returns school-specific template if available, otherwise returns global template.
    /// </summary>
    [HttpGet("template/{code}")]
    public async Task<IActionResult> GetTemplate(string code, [FromQuery] int? schoolId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(APIResponse.Fail("Template code is required."));
        }

        var result = await _unitOfWork.Reports.GetTemplateByCodeAsync(code, schoolId);
        
        if (result.Ok)
        {
            return Ok(APIResponse.Success(result.Value!));
        }
        
        return NotFound(APIResponse.Fail(result.Error!));
    }

    /// <summary>
    /// Save or update report template. Creates new template if it doesn't exist, updates if it does.
    /// </summary>
    [HttpPost("save")]
    public async Task<IActionResult> SaveTemplate([FromBody] ReportTemplateSaveDTO dto, [FromQuery] int? schoolId = null)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(APIResponse.Fail("Invalid template data."));
        }

        if (string.IsNullOrWhiteSpace(dto.Code))
        {
            return BadRequest(APIResponse.Fail("Template code is required."));
        }

        // Use schoolId from query parameter if provided, otherwise use from DTO
        var finalSchoolId = schoolId ?? dto.SchoolId;

        var result = await _unitOfWork.Reports.SaveTemplateAsync(dto, finalSchoolId);
        
        if (result.Ok)
        {
            return Ok(APIResponse.Success(result.Value!));
        }
        
        return BadRequest(APIResponse.Fail(result.Error!));
    }
}
