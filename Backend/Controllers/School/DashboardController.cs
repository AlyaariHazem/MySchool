using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Backend.DTOS.Dashboard;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers.School;

[Authorize(Roles = "ADMIN,MANAGER")]
[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<APIResponse>> GetDashboard()
    {
        var response = new APIResponse();
        try
        {
            var dashboard = new DashboardDTO();

            // Get summary statistics from repository
            dashboard.Summary = await _unitOfWork.Dashboard.GetDashboardSummaryAsync();

            // Get recent exams from repository
            dashboard.RecentExams = await _unitOfWork.Dashboard.GetRecentExamsAsync();

            // Get student enrollment trend from repository
            dashboard.StudentEnrollmentTrend = await _unitOfWork.Dashboard.GetStudentEnrollmentTrendAsync();

            response.Result = dashboard;
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.InternalServerError;
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }

    [HttpGet("exams")]
    public async Task<ActionResult<APIResponse>> GetExams()
    {
        var response = new APIResponse();
        try
        {
            var exams = await _unitOfWork.Dashboard.GetAllExamsAsync();

            response.Result = exams;
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.InternalServerError;
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }
}

