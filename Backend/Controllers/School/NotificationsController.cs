using System.Net;
using System.Security.Claims;
using Backend.DTOS.School.Notifications;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>Send a notification to students, guardians, teachers, specific users, or tenant-wide audiences (admins/managers only for tenant-wide).</summary>
    [HttpPost("send")]
    [Authorize(Roles = "ADMIN,MANAGER,TEACHER")]
    public async Task<ActionResult<APIResponse>> Send([FromBody] SendNotificationRequestDTO request, CancellationToken cancellationToken)
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

            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.BadRequest;
                response.ErrorMasseges.Add("Invalid request.");
                return BadRequest(response);
            }

            var privileged = User.IsInRole("ADMIN") || User.IsInRole("MANAGER");
            var teacherId = await _unitOfWork.Teachers.GetTeacherIdByUserIdAsync(userId, cancellationToken);

            if (!privileged && User.IsInRole("TEACHER"))
            {
                if (teacherId is null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.Forbidden;
                    response.ErrorMasseges.Add("No teacher profile is linked to this account.");
                    return StatusCode((int)HttpStatusCode.Forbidden, response);
                }
            }

            var (ok, result, error) = await _unitOfWork.Notifications.SendAsync(
                request,
                userId,
                privileged,
                teacherId,
                cancellationToken);

            if (!ok)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.BadRequest;
                response.ErrorMasseges.Add(error ?? "Could not send notification.");
                return BadRequest(response);
            }

            response.Result = result;
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

    [HttpGet("inbox")]
    public async Task<ActionResult<APIResponse>> Inbox([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
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

            var page = await _unitOfWork.Notifications.GetInboxAsync(userId, pageNumber, pageSize, cancellationToken);
            response.Result = page;
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

    [HttpGet("unread-count")]
    public async Task<ActionResult<APIResponse>> UnreadCount(CancellationToken cancellationToken)
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

            var count = await _unitOfWork.Notifications.GetUnreadCountAsync(userId, cancellationToken);
            response.Result = new { count };
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

    [HttpPost("mark-read")]
    public async Task<ActionResult<APIResponse>> MarkRead([FromBody] MarkNotificationReadRequestDTO request, CancellationToken cancellationToken)
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

            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.BadRequest;
                response.ErrorMasseges.Add("Invalid request.");
                return BadRequest(response);
            }

            var updated = await _unitOfWork.Notifications.MarkReadAsync(userId, request.DeliveryId, cancellationToken);
            if (!updated)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("Notification delivery was not found.");
                return NotFound(response);
            }

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
