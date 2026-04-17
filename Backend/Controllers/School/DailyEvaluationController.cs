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

    public DailyEvaluationController(IDailyEvaluationService svc)
    {
        _svc = svc;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

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
            var row = await _svc.GetEvaluationByIdAsync(id, cancellationToken);
            if (row == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add($"Evaluation {id} was not found.");
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

    [HttpGet("{id:int}/full")]
    public async Task<ActionResult<APIResponse>> GetEvaluationFull(int id, CancellationToken cancellationToken) =>
        await RunAsync(() => _svc.GetEvaluationFullAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<APIResponse>> CreateEvaluation([FromBody] DailyEvaluationCreateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _svc.CreateEvaluationAsync(body, cancellationToken));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<APIResponse>> UpdateEvaluation(int id, [FromBody] DailyEvaluationUpdateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _svc.UpdateEvaluationAsync(id, body, CurrentUserId, cancellationToken));

    [HttpPost("{id:int}/submit")]
    public async Task<ActionResult<APIResponse>> SubmitEvaluation(int id, CancellationToken cancellationToken) =>
        await RunAsync(() => _svc.SubmitEvaluationAsync(id, cancellationToken));

    [HttpPost("{id:int}/items")]
    public async Task<ActionResult<APIResponse>> UpsertItem(int id, [FromBody] DailyEvaluationItemCreateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _svc.UpsertItemAsync(id, body, cancellationToken));

    [HttpPut("items/{itemId:int}")]
    public async Task<ActionResult<APIResponse>> UpdateItem(int itemId, [FromBody] DailyEvaluationItemUpdateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _svc.UpdateItemAsync(itemId, body, cancellationToken));

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
