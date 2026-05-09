using System.Net;
using System.Security.Claims;
using Backend.DTOS.School.TimeCapsule;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

/// <summary>كبسولة الزمن — employee journey archive (locked until resignation + approvals).</summary>
[ApiController]
[Route("api/time-capsule")]
[Authorize]
public class TimeCapsuleController : ControllerBase
{
    private readonly ITimeCapsuleService _capsule;

    public TimeCapsuleController(ITimeCapsuleService capsule)
    {
        _capsule = capsule;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private string? UserTypeClaim => User.FindFirstValue("UserType");

    private bool IsAdmin => User.IsInRole("ADMIN");

    [HttpGet("status/{employeeId:int}")]
    public async Task<ActionResult<APIResponse>> GetStatus(int employeeId, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            response.Result = await _capsule.GetCapsuleStatusAsync(
                employeeId,
                CurrentUserId,
                UserTypeClaim,
                IsAdmin,
                cancellationToken);
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.Forbidden;
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.Forbidden, response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.InternalServerError;
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }

    [HttpGet("{capsuleId:int}/logs")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> GetLogs(int capsuleId, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            response.Result = await _capsule.GetAccessLogsAsync(capsuleId, CurrentUserId, UserTypeClaim, IsAdmin, cancellationToken);
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.Forbidden;
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.Forbidden, response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.BadRequest;
            response.ErrorMasseges.Add(ex.Message);
            return BadRequest(response);
        }
    }

    [HttpGet("{employeeId:int}")]
    public async Task<ActionResult<APIResponse>> GetCapsule(int employeeId, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            var detail = await _capsule.GetCapsuleAsync(
                employeeId,
                CurrentUserId,
                UserTypeClaim,
                IsAdmin,
                cancellationToken);

            if (detail == null)
            {
                var status = await _capsule.GetCapsuleStatusAsync(
                    employeeId,
                    CurrentUserId,
                    UserTypeClaim,
                    IsAdmin,
                    cancellationToken);
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                response.ErrorMasseges.Add(status.MessageAr);
                response.Result = status;
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }

            response.Result = detail;
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.Forbidden;
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.Forbidden, response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.InternalServerError;
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }

    [HttpPost("resignation")]
    public async Task<ActionResult<APIResponse>> RequestResignation(
        [FromBody] ResignationRequestCreateDto? body,
        CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            if (body == null || CurrentUserId == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.BadRequest;
                response.ErrorMasseges.Add("Request body and authenticated user are required.");
                return BadRequest(response);
            }

            response.Result = await _capsule.RequestResignationAsync(body, CurrentUserId, UserTypeClaim, IsAdmin, cancellationToken);
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.Forbidden;
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.Forbidden, response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.BadRequest;
            response.ErrorMasseges.Add(ex.Message);
            return BadRequest(response);
        }
    }

    [HttpPost("resignation/{id:int}/approve")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> ApproveResignation(
        int id,
        [FromBody] ApproveRejectNotesDto? body,
        CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            if (CurrentUserId == null)
                return Unauthorized(response);
            response.Result = await _capsule.ApproveResignationAsync(id, CurrentUserId, UserTypeClaim, IsAdmin, body?.Notes, cancellationToken);
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.Forbidden;
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.Forbidden, response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.BadRequest;
            response.ErrorMasseges.Add(ex.Message);
            return BadRequest(response);
        }
    }

    [HttpPost("resignation/{id:int}/reject")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> RejectResignation(
        int id,
        [FromBody] ApproveRejectNotesDto? body,
        CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            if (CurrentUserId == null)
                return Unauthorized(response);
            response.Result = await _capsule.RejectResignationAsync(id, CurrentUserId, UserTypeClaim, IsAdmin, body?.Notes, cancellationToken);
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.Forbidden;
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.Forbidden, response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.BadRequest;
            response.ErrorMasseges.Add(ex.Message);
            return BadRequest(response);
        }
    }

    [HttpPost("unlock/{capsuleId:int}/approve")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> ApproveUnlock(
        int capsuleId,
        [FromBody] CapsuleUnlockApproveDto? body,
        CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            if (CurrentUserId == null)
                return Unauthorized(response);
            await _capsule.ApproveCapsuleUnlockAsync(
                capsuleId,
                CurrentUserId,
                UserTypeClaim,
                IsAdmin,
                body?.UnlockReason ?? body?.Notes,
                cancellationToken);
            response.Result = new { ok = true };
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.Forbidden;
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.Forbidden, response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.BadRequest;
            response.ErrorMasseges.Add(ex.Message);
            return BadRequest(response);
        }
    }

    [HttpPost("unlock/{capsuleId:int}/reject")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> RejectUnlock(
        int capsuleId,
        [FromBody] ApproveRejectNotesDto? body,
        CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            if (CurrentUserId == null)
                return Unauthorized(response);
            await _capsule.RejectCapsuleUnlockAsync(capsuleId, CurrentUserId, UserTypeClaim, IsAdmin, body?.Notes, cancellationToken);
            response.Result = new { ok = true };
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.Forbidden;
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.Forbidden, response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.BadRequest;
            response.ErrorMasseges.Add(ex.Message);
            return BadRequest(response);
        }
    }
}
