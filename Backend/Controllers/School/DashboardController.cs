using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Backend.DTOS.Dashboard;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers.School;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>Admin/manager school-wide metrics (not for students).</summary>
    [HttpGet]
    [Authorize(Roles = "ADMIN,MANAGER")]
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

    /// <summary>Logged-in student’s class/year context for a simple home screen.</summary>
    [HttpGet("student")]
    [Authorize(Roles = "STUDENT")]
    public async Task<ActionResult<APIResponse>> GetStudentDashboard()
    {
        var response = new APIResponse();
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Unauthorized;
                response.ErrorMasseges.Add("User id not found on token.");
                return Unauthorized(response);
            }

            var dto = await _unitOfWork.Dashboard.GetStudentDashboardAsync(userId);
            if (dto == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("No student profile is linked to this account in this school.");
                return NotFound(response);
            }

            response.Result = dto;
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
    [Authorize(Roles = "ADMIN,MANAGER")]
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

