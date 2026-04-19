using System.Net;
using System.Security.Claims;
using Backend.DTOS.School.DailyEvaluation;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

/// <summary>Daily employee performance evaluations (templates, criteria, scores, day locks, overrides).</summary>
[ApiController]
[Route("api/daily-evaluations")]
[Authorize]
public class DailyEvaluationController : ControllerBase
{
    private readonly IDailyEvaluationService _svc;
    private readonly IUnitOfWork _unitOfWork;

    public DailyEvaluationController(IDailyEvaluationService svc, IUnitOfWork unitOfWork)
    {
        _svc = svc;
        _unitOfWork = unitOfWork;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private string? UserTypeClaim => User.FindFirstValue("UserType");

    private bool IsPrivilegedAppUser =>
        User.IsInRole("ADMIN") || User.IsInRole("MANAGER")
        || string.Equals(UserTypeClaim, "ADMIN", StringComparison.OrdinalIgnoreCase)
        || string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase);

    /// <summary>Teachers may only access evaluations tied to their own HR employee profile.</summary>
    private async Task<(bool RestrictToOwnProfile, int? OwnEmployeeProfileId, ActionResult<APIResponse>? ErrorResponse)>
        GetTeacherDailyEvalScopeAsync(CancellationToken cancellationToken)
    {
        if (IsPrivilegedAppUser)
            return (false, null, null);

        if (!string.Equals(UserTypeClaim, "TEACHER", StringComparison.OrdinalIgnoreCase))
            return (false, null, null);

        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
        {
            var r = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Unauthorized };
            r.ErrorMasseges.Add("User id not found on token.");
            return (false, null, Unauthorized(r));
        }

        var empId = await _unitOfWork.Teachers.GetEmployeeProfileIdForTeacherUserAsync(userId, cancellationToken);
        if (!empId.HasValue)
        {
            var r = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Forbidden };
            r.ErrorMasseges.Add("No employee profile is linked to this teacher account.");
            return (false, null, StatusCode((int)HttpStatusCode.Forbidden, r));
        }

        return (true, empId, null);
    }

    private bool IsStudentUser =>
        string.Equals(UserTypeClaim, "STUDENT", StringComparison.OrdinalIgnoreCase);

    /// <summary>Teachers: evaluated must be self. Students: must be the evaluator. Others: allowed.</summary>
    private async Task<ActionResult<APIResponse>?> EnsureEvaluationRowAccessAsync(DailyEvaluationReadDto? row, CancellationToken cancellationToken)
    {
        if (row == null)
        {
            var nf = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.NotFound };
            nf.ErrorMasseges.Add("Evaluation was not found.");
            return NotFound(nf);
        }

        if (IsPrivilegedAppUser)
            return null;

        if (string.Equals(UserTypeClaim, "TEACHER", StringComparison.OrdinalIgnoreCase))
        {
            var scope = await GetTeacherDailyEvalScopeAsync(cancellationToken);
            if (scope.ErrorResponse != null)
                return scope.ErrorResponse;
            if (scope.RestrictToOwnProfile && scope.OwnEmployeeProfileId is int ownId && row.EvaluatedEmployeeProfileID != ownId)
            {
                var f = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Forbidden };
                f.ErrorMasseges.Add("You do not have access to this evaluation.");
                return StatusCode((int)HttpStatusCode.Forbidden, f);
            }
            return null;
        }

        if (IsStudentUser)
        {
            var uid = CurrentUserId;
            if (string.IsNullOrEmpty(uid) || !string.Equals(row.EvaluatorUserId, uid, StringComparison.Ordinal))
            {
                var f = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Forbidden };
                f.ErrorMasseges.Add("You do not have access to this evaluation.");
                return StatusCode((int)HttpStatusCode.Forbidden, f);
            }
            return null;
        }

        return null;
    }

    // --- Templates ---

    [HttpGet("templates")]
    public async Task<ActionResult<APIResponse>> GetTemplates([FromQuery] DailyEvaluationTemplateFilterDto? filter, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            response.Result = await _svc.GetTemplatesAsync(filter, cancellationToken);
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

    /// <summary>Returns the current teacher user's HR employee profile id (for self-evaluations). Non-teachers receive 400.</summary>
    [HttpGet("me/employee-profile-id")]
    public async Task<ActionResult<APIResponse>> GetMyEmployeeProfileId(CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            var scope = await GetTeacherDailyEvalScopeAsync(cancellationToken);
            if (scope.ErrorResponse != null)
                return scope.ErrorResponse;

            if (!scope.RestrictToOwnProfile || scope.OwnEmployeeProfileId is not int id)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.BadRequest;
                response.ErrorMasseges.Add("This endpoint is only available for teacher accounts.");
                return BadRequest(response);
            }

            response.Result = id;
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

    /// <summary>
    /// Teachers relevant to the current student (homeroom + course plans for the division for the school's active academic year), scoped to the school.
    /// When the caller is not a student, returns all teaching staff in the school (same as legacy behaviour).
    /// </summary>
    [HttpGet("for-student/teachers")]
    public async Task<ActionResult<APIResponse>> GetTeachersForStudentEvaluation(
        [FromQuery] int schoolId,
        CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            if (schoolId <= 0)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.BadRequest;
                response.ErrorMasseges.Add("schoolId is required.");
                return BadRequest(response);
            }

            var studentUserId = IsStudentUser ? CurrentUserId : null;
            response.Result = await _svc.GetTeachersForStudentEvaluationAsync(schoolId, studentUserId, cancellationToken);
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.BadRequest;
            response.ErrorMasseges.Add(ex.Message);
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.InternalServerError;
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }

    [HttpGet("templates/{id:int}")]
    public async Task<ActionResult<APIResponse>> GetTemplateById(int id, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            var row = await _svc.GetTemplateByIdAsync(id, cancellationToken);
            if (row == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add($"Template {id} was not found.");
                return NotFound(response);
            }
            response.Result = row;
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

    [HttpPost("templates")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> CreateTemplate([FromBody] DailyEvaluationTemplateCreateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _svc.CreateTemplateAsync(body, cancellationToken));

    [HttpPut("templates/{id:int}")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> UpdateTemplate(int id, [FromBody] DailyEvaluationTemplateUpdateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _svc.UpdateTemplateAsync(id, body, cancellationToken));

    [HttpPost("templates/{id:int}/activate")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> ActivateTemplate(int id, CancellationToken cancellationToken) =>
        await RunAsync(() => _svc.ActivateTemplateAsync(id, cancellationToken));

    [HttpPost("templates/{id:int}/deactivate")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> DeactivateTemplate(int id, CancellationToken cancellationToken) =>
        await RunAsync(() => _svc.DeactivateTemplateAsync(id, cancellationToken));

    [HttpPost("templates/{id:int}/archive")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> ArchiveTemplate(int id, CancellationToken cancellationToken) =>
        await RunAsync(() => _svc.ArchiveTemplateAsync(id, cancellationToken));

    // --- Criteria ---

    [HttpGet("templates/{id:int}/criteria")]
    public async Task<ActionResult<APIResponse>> GetCriteria(int id, CancellationToken cancellationToken) =>
        await RunAsync(() => _svc.GetCriteriaForTemplateAsync(id, cancellationToken));

    [HttpPost("templates/{id:int}/criteria")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> AddCriteria(int id, [FromBody] DailyEvaluationCriteriaCreateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _svc.AddCriteriaAsync(id, body, cancellationToken));

    [HttpPut("criteria/{criteriaId:int}")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> UpdateCriteria(int criteriaId, [FromBody] DailyEvaluationCriteriaUpdateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _svc.UpdateCriteriaAsync(criteriaId, body, cancellationToken));

    // --- Evaluations ---

    [HttpGet]
    public async Task<ActionResult<APIResponse>> GetEvaluations([FromQuery] DailyEvaluationFilterDto? filter, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            var scope = await GetTeacherDailyEvalScopeAsync(cancellationToken);
            if (scope.ErrorResponse != null)
                return scope.ErrorResponse;

            filter ??= new DailyEvaluationFilterDto();
            if (scope.RestrictToOwnProfile && scope.OwnEmployeeProfileId is int ownId)
                filter.EvaluatedEmployeeProfileID = ownId;
            else if (IsStudentUser)
            {
                var uid = CurrentUserId;
                if (string.IsNullOrEmpty(uid))
                {
                    var r = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Unauthorized };
                    r.ErrorMasseges.Add("User id not found on token.");
                    return Unauthorized(r);
                }

                filter.EvaluatorUserId = uid;
            }

            response.Result = await _svc.GetEvaluationsAsync(filter, cancellationToken);
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

    [HttpGet("{id:int}")]
    public async Task<ActionResult<APIResponse>> GetEvaluation(int id, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            var scope = await GetTeacherDailyEvalScopeAsync(cancellationToken);
            if (scope.ErrorResponse != null)
                return scope.ErrorResponse;

            var row = await _svc.GetEvaluationByIdAsync(id, cancellationToken);
            var denied = await EnsureEvaluationRowAccessAsync(row, cancellationToken);
            if (denied != null)
                return denied;

            response.Result = row;
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

    [HttpGet("{id:int}/full")]
    public async Task<ActionResult<APIResponse>> GetEvaluationFull(int id, CancellationToken cancellationToken)
    {
        var scope = await GetTeacherDailyEvalScopeAsync(cancellationToken);
        if (scope.ErrorResponse != null)
            return scope.ErrorResponse;

        var summary = await _svc.GetEvaluationByIdAsync(id, cancellationToken);
        if (summary == null)
        {
            var notFound = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.NotFound };
            notFound.ErrorMasseges.Add($"Evaluation {id} was not found.");
            return NotFound(notFound);
        }

        var deniedFull = await EnsureEvaluationRowAccessAsync(summary, cancellationToken);
        if (deniedFull != null)
            return deniedFull;

        return await RunAsync(() => _svc.GetEvaluationFullAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<APIResponse>> CreateEvaluation([FromBody] DailyEvaluationCreateDto body, CancellationToken cancellationToken)
    {
        var scope = await GetTeacherDailyEvalScopeAsync(cancellationToken);
        if (scope.ErrorResponse != null)
            return scope.ErrorResponse;

        if (scope.RestrictToOwnProfile && scope.OwnEmployeeProfileId is int ownId)
            body.EvaluatedEmployeeProfileID = ownId;

        if (string.IsNullOrEmpty(body.EvaluatorUserId))
            body.EvaluatorUserId = CurrentUserId;

        if (IsStudentUser)
        {
            var uid = CurrentUserId;
            if (string.IsNullOrEmpty(uid))
            {
                var r = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Unauthorized };
                r.ErrorMasseges.Add("User id not found on token.");
                return Unauthorized(r);
            }

            var err = await _svc.ValidateStudentEvaluationCreateAsync(body, uid, cancellationToken);
            if (err != null)
            {
                var bad = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.BadRequest };
                bad.ErrorMasseges.Add(err);
                return BadRequest(bad);
            }
        }

        return await RunAsync(() => _svc.CreateEvaluationAsync(body, cancellationToken));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<APIResponse>> UpdateEvaluation(int id, [FromBody] DailyEvaluationUpdateDto body, CancellationToken cancellationToken)
    {
        var row = await _svc.GetEvaluationByIdAsync(id, cancellationToken);
        var denied = await EnsureEvaluationRowAccessAsync(row, cancellationToken);
        if (denied != null)
            return denied;
        return await RunAsync(() => _svc.UpdateEvaluationAsync(id, body, CurrentUserId, cancellationToken));
    }

    [HttpPost("{id:int}/submit")]
    public async Task<ActionResult<APIResponse>> SubmitEvaluation(int id, CancellationToken cancellationToken)
    {
        var row = await _svc.GetEvaluationByIdAsync(id, cancellationToken);
        var denied = await EnsureEvaluationRowAccessAsync(row, cancellationToken);
        if (denied != null)
            return denied;
        return await RunAsync(() => _svc.SubmitEvaluationAsync(id, cancellationToken));
    }

    [HttpPost("{id:int}/items")]
    public async Task<ActionResult<APIResponse>> UpsertItem(int id, [FromBody] DailyEvaluationItemCreateDto body, CancellationToken cancellationToken)
    {
        var row = await _svc.GetEvaluationByIdAsync(id, cancellationToken);
        var denied = await EnsureEvaluationRowAccessAsync(row, cancellationToken);
        if (denied != null)
            return denied;
        return await RunAsync(() => _svc.UpsertItemAsync(id, body, cancellationToken));
    }

    [HttpPut("items/{itemId:int}")]
    public async Task<ActionResult<APIResponse>> UpdateItem(int itemId, [FromBody] DailyEvaluationItemUpdateDto body, CancellationToken cancellationToken)
    {
        var evalId = await _svc.GetEvaluationIdForItemAsync(itemId, cancellationToken);
        if (evalId == null)
        {
            var nf = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.NotFound };
            nf.ErrorMasseges.Add("Evaluation item was not found.");
            return NotFound(nf);
        }

        var row = await _svc.GetEvaluationByIdAsync(evalId.Value, cancellationToken);
        var denied = await EnsureEvaluationRowAccessAsync(row, cancellationToken);
        if (denied != null)
            return denied;
        return await RunAsync(() => _svc.UpdateItemAsync(itemId, body, cancellationToken));
    }

    // --- Locks ---

    [HttpGet("locks/by-date")]
    public async Task<ActionResult<APIResponse>> GetLockByDate([FromQuery] int schoolId, [FromQuery] int academicYearId, [FromQuery] DateOnly date, [FromQuery] int? templateId, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            response.Result = await _svc.GetLockByDateAsync(schoolId, academicYearId, date, templateId, cancellationToken);
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

    [HttpPost("locks")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> LockDay([FromBody] EvaluationLockCreateDto body, CancellationToken cancellationToken)
    {
        var uid = CurrentUserId ?? throw new InvalidOperationException("User id required.");
        return await RunAsync(() => _svc.LockDayAsync(body, uid, cancellationToken));
    }

    [HttpPost("locks/{lockId:int}/reopen")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> ReopenLock(int lockId, [FromBody] EvaluationReopenDto body, CancellationToken cancellationToken)
    {
        var uid = CurrentUserId ?? throw new InvalidOperationException("User id required.");
        return await RunAsync(() => _svc.ReopenLockAsync(lockId, body, uid, cancellationToken));
    }

    // --- Overrides ---

    [HttpPost("{id:int}/override-update")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> OverrideUpdate(int id, [FromBody] EvaluationOverrideRequestDto body, CancellationToken cancellationToken)
    {
        var uid = CurrentUserId ?? throw new InvalidOperationException("User id required.");
        return await RunAsync(() => _svc.OverrideUpdateAfterLockAsync(id, body, uid, cancellationToken));
    }

    [HttpGet("{id:int}/override-logs")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> GetOverrideLogs(int id, CancellationToken cancellationToken) =>
        await RunAsync(() => _svc.GetOverrideLogsForEvaluationAsync(id, cancellationToken));

    private async Task<ActionResult<APIResponse>> RunAsync<T>(Func<Task<T>> action)
    {
        var response = new APIResponse();
        try
        {
            response.Result = await action();
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.NotFound;
            response.ErrorMasseges.Add(ex.Message);
            return NotFound(response);
        }
        catch (ArgumentException ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.BadRequest;
            response.ErrorMasseges.Add(ex.Message);
            return BadRequest(response);
        }
        catch (InvalidOperationException ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.Conflict;
            response.ErrorMasseges.Add(ex.Message);
            return Conflict(response);
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
