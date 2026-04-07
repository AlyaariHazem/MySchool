using System;
using System.Net;
using System.Security.Claims;
using Backend.DTOS.Dashboard;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

/// <summary>Teacher-facing dashboard: class/student/subject counts and recent course plans (timetable-linked teaching assignments).</summary>
[Authorize(Roles = "ADMIN,MANAGER,TEACHER")]
[Route("api/[controller]")]
[ApiController]
public class TeacherWorkspaceController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public TeacherWorkspaceController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet]
    public async Task<ActionResult<APIResponse>> GetWorkspace(CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Unauthorized;
                response.ErrorMasseges.Add("User id not found on token.");
                return Unauthorized(response);
            }

            var userType = User.FindFirstValue("UserType");
            var privileged =
                User.IsInRole("ADMIN") || User.IsInRole("MANAGER")
                || string.Equals(userType, "ADMIN", StringComparison.OrdinalIgnoreCase)
                || string.Equals(userType, "MANAGER", StringComparison.OrdinalIgnoreCase);
            var teacherId = await _unitOfWork.Teachers.GetTeacherIdByUserIdAsync(userId, cancellationToken);

            TeacherWorkspaceDTO workspace;
            if (teacherId.HasValue)
            {
                workspace = await _unitOfWork.Dashboard.GetTeacherWorkspaceAsync(teacherId.Value);
            }
            else if (privileged)
            {
                workspace = await _unitOfWork.Dashboard.GetSchoolTeachingWorkspaceAsync();
            }
            else
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                response.ErrorMasseges.Add("No teacher profile is linked to this account.");
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }
            response.Result = workspace;
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
