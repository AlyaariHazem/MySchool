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
    [HttpGet]
    public async Task<IActionResult> GetAllReports()
    {
        var result = await _unitOfWork.Reports.GetAllReportsAsync();
        if (result.Ok)
        {
            return result.Ok
           ? Ok(APIResponse.Success(result.Value!))
           : NotFound(APIResponse.Fail(result.Error!));
        }
        return NotFound(result.Error);
    }
}
