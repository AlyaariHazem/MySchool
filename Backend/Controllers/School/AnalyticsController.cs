using System.Net;
using System.Security.Claims;
using Backend.DTOS.School.Analytics;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;

namespace Backend.Controllers.School;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAnalyticsService _analytics;
    private readonly IEmployeeProfileService _employees;
    private readonly TenantDbContext _db;

    public AnalyticsController(
        IUnitOfWork unitOfWork,
        IAnalyticsService analytics,
        IEmployeeProfileService employees,
        TenantDbContext db)
    {
        _unitOfWork = unitOfWork;
        _analytics = analytics;
        _employees = employees;
        _db = db;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
    private string? UserTypeClaim => User.FindFirstValue("UserType");

    private async Task<int?> ResolveManagerSchoolIdAsync(CancellationToken cancellationToken)
    {
        if (!string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase))
            return null;
        return await _employees.GetSchoolIdForManagerUserAsync(CurrentUserId, cancellationToken);
    }

    private async Task<bool> ApplyManagerSchoolOrForbiddenAsync(
        APIResponse response,
        Action<int> setSchoolId,
        CancellationToken cancellationToken)
    {
        var sid = await ResolveManagerSchoolIdAsync(cancellationToken);
        if (sid is int id && id > 0)
        {
            setSchoolId(id);
            return true;
        }

        if (string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase))
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.Forbidden;
            response.ErrorMasseges.Add("No school is linked to this manager account.");
            return false;
        }

        return true;
    }

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpPost("dashboard")]
    public async Task<ActionResult<APIResponse>> GetDashboard(
        [FromBody] AnalyticsDashboardQueryDto? query,
        CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            query ??= new AnalyticsDashboardQueryDto();
            if (!await ApplyManagerSchoolOrForbiddenAsync(response, id => query.SchoolID = id, cancellationToken))
                return StatusCode((int)HttpStatusCode.Forbidden, response);

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

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpGet("kpis")]
    public async Task<ActionResult<APIResponse>> GetKpis([FromQuery] AnalyticsListQueryDto query, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            if (!await ApplyManagerSchoolOrForbiddenAsync(response, id => query.SchoolID = id, cancellationToken))
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            response.Result = await _analytics.GetKpiDefinitionsAsync(query, cancellationToken);
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

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpGet("kpi-snapshots")]
    public async Task<ActionResult<APIResponse>> GetKpiSnapshots([FromQuery] AnalyticsListQueryDto query, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            if (!await ApplyManagerSchoolOrForbiddenAsync(response, id => query.SchoolID = id, cancellationToken))
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            response.Result = await _analytics.GetKpiSnapshotsDashboardAsync(query, cancellationToken);
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

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpPost("generate-snapshots")]
    public async Task<ActionResult<APIResponse>> GenerateSnapshots(
        [FromBody] AnalyticsGenerateRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            if (!await ApplyManagerSchoolOrForbiddenAsync(response, id => request.SchoolID = id, cancellationToken))
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            response.Result = await _analytics.GenerateSnapshotsAsync(request, cancellationToken);
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

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpGet("dashboards/executive")]
    public Task<ActionResult<APIResponse>> ExecutiveDashboard([FromQuery] AnalyticsDashboardQueryDto query, CancellationToken cancellationToken)
        => DashboardResultAsync(query, q => _analytics.GetExecutiveDashboardAsync(q, cancellationToken), cancellationToken);

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpGet("dashboards/educational-supervisor")]
    public Task<ActionResult<APIResponse>> EducationalSupervisorDashboard([FromQuery] AnalyticsDashboardQueryDto query, CancellationToken cancellationToken)
        => DashboardResultAsync(query, q => _analytics.GetEducationalSupervisorDashboardAsync(q, cancellationToken), cancellationToken);

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpGet("dashboards/administrative-supervisor")]
    public Task<ActionResult<APIResponse>> AdministrativeSupervisorDashboard([FromQuery] AnalyticsDashboardQueryDto query, CancellationToken cancellationToken)
        => DashboardResultAsync(query, q => _analytics.GetAdministrativeSupervisorDashboardAsync(q, cancellationToken), cancellationToken);

    [Authorize(Roles = "ADMIN,MANAGER,TEACHER")]
    [HttpGet("dashboards/employee/{employeeProfileId:int}")]
    public async Task<ActionResult<APIResponse>> EmployeeDashboard(
        int employeeProfileId,
        [FromQuery] AnalyticsDashboardQueryDto query,
        CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            query ??= new AnalyticsDashboardQueryDto();
            var profile = await _db.EmployeeProfiles.AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeProfileID == employeeProfileId, cancellationToken);
            if (profile == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("Employee profile was not found.");
                return NotFound(response);
            }

            query.SchoolID ??= profile.SchoolID;

            if (User.IsInRole("TEACHER") && !User.IsInRole("ADMIN") && !User.IsInRole("MANAGER"))
            {
                if (!string.Equals(profile.UserId, CurrentUserId, StringComparison.Ordinal))
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.Forbidden;
                    response.ErrorMasseges.Add("Employees may only view their own analytics dashboard.");
                    return StatusCode((int)HttpStatusCode.Forbidden, response);
                }
            }
            else if (!await ApplyManagerSchoolOrForbiddenAsync(response, id => query.SchoolID = id, cancellationToken))
                return StatusCode((int)HttpStatusCode.Forbidden, response);

            var dashboard = await _analytics.GetEmployeeDashboardAsync(employeeProfileId, query, cancellationToken);
            response.Result = PackageDashboard(dashboard);
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

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpGet("dashboards/school")]
    public Task<ActionResult<APIResponse>> SchoolDashboard([FromQuery] AnalyticsDashboardQueryDto query, CancellationToken cancellationToken)
        => DashboardResultAsync(query, q => _analytics.GetSchoolDashboardAsync(q, cancellationToken), cancellationToken);

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpGet("comparison/years")]
    public async Task<ActionResult<APIResponse>> YearComparison([FromQuery] YearComparisonQueryDto query, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            if (!await ApplyManagerSchoolOrForbiddenAsync(response, id => query.SchoolID = id, cancellationToken))
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            response.Result = await _analytics.GetYearOverYearComparisonAsync(query, cancellationToken);
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

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpGet("trends")]
    public async Task<ActionResult<APIResponse>> Trends([FromQuery] AnalyticsDashboardQueryDto query, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            query ??= new AnalyticsDashboardQueryDto();
            if (!await ApplyManagerSchoolOrForbiddenAsync(response, id => query.SchoolID = id, cancellationToken))
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            var dashboard = await _unitOfWork.Analytics.GetDashboardAsync(query, cancellationToken);
            response.Result = dashboard.Trends;
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

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpGet("teachers")]
    public async Task<ActionResult<APIResponse>> Teachers([FromQuery] AnalyticsDashboardQueryDto query, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            query ??= new AnalyticsDashboardQueryDto();
            if (!await ApplyManagerSchoolOrForbiddenAsync(response, id => query.SchoolID = id, cancellationToken))
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            var dashboard = await _unitOfWork.Analytics.GetDashboardAsync(query, cancellationToken);
            response.Result = dashboard.Teachers;
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

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpGet("departments")]
    public async Task<ActionResult<APIResponse>> Departments([FromQuery] AnalyticsDashboardQueryDto query, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            query ??= new AnalyticsDashboardQueryDto();
            if (!await ApplyManagerSchoolOrForbiddenAsync(response, id => query.SchoolID = id, cancellationToken))
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            var dashboard = await _unitOfWork.Analytics.GetDashboardAsync(query, cancellationToken);
            response.Result = dashboard.Departments;
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

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpGet("school-analytics")]
    public async Task<ActionResult<APIResponse>> SchoolAnalyticsRows([FromQuery] AnalyticsDashboardQueryDto query, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            query ??= new AnalyticsDashboardQueryDto();
            if (!await ApplyManagerSchoolOrForbiddenAsync(response, id => query.SchoolID = id, cancellationToken))
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            var dashboard = await _unitOfWork.Analytics.GetDashboardAsync(query, cancellationToken);
            response.Result = dashboard.School;
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

    private async Task<ActionResult<APIResponse>> DashboardResultAsync(
        AnalyticsDashboardQueryDto? query,
        Func<AnalyticsDashboardQueryDto, Task<AnalyticsDashboardResultDto>> factory,
        CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            query ??= new AnalyticsDashboardQueryDto();
            if (!await ApplyManagerSchoolOrForbiddenAsync(response, id => query.SchoolID = id, cancellationToken))
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            var dashboard = await factory(query);
            response.Result = PackageDashboard(dashboard);
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

    private static object PackageDashboard(AnalyticsDashboardResultDto dashboard) => new
    {
        cards = dashboard.Cards,
        snapshots = dashboard.Snapshots,
        trends = dashboard.Trends,
        departments = dashboard.Departments,
        teachers = dashboard.Teachers,
        school = dashboard.School
    };
}
