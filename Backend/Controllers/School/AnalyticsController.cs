using System.Net;
using System.Security.Claims;
using Backend.DTOS.School.Analytics;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

[ApiController]
[Route("api/analytics")]
[Authorize(Roles = "ADMIN,MANAGER")]
public class AnalyticsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmployeeProfileService _employees;

    public AnalyticsController(IUnitOfWork unitOfWork, IEmployeeProfileService employees)
    {
        _unitOfWork = unitOfWork;
        _employees = employees;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
    private string? UserTypeClaim => User.FindFirstValue("UserType");

    [HttpPost("dashboard")]
    public async Task<ActionResult<APIResponse>> GetDashboard(
        [FromBody] AnalyticsDashboardQueryDto? query,
        CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            query ??= new AnalyticsDashboardQueryDto();

            if (string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase))
            {
                var sid = await _employees.GetSchoolIdForManagerUserAsync(CurrentUserId, cancellationToken);
                if (sid is not int managerSchoolId || managerSchoolId <= 0)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.Forbidden;
                    response.ErrorMasseges.Add("No school is linked to this manager account.");
                    return StatusCode((int)HttpStatusCode.Forbidden, response);
                }

                query.SchoolID = managerSchoolId;
            }

            var dashboard = await _unitOfWork.Analytics.GetDashboardAsync(query, cancellationToken);

            response.Result = new
            {
                cards = dashboard.Cards,
                snapshots = dashboard.Snapshots,
                trends = dashboard.Trends,
                departments = dashboard.Departments,
                teachers = dashboard.Teachers,
                school = dashboard.School
            };
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
