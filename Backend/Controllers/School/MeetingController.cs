using System.Net;
using System.Security.Claims;
using Backend.DTOS.School.Meeting;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

/// <summary>Staff meetings, minutes, and action items.</summary>
[ApiController]
[Route("api/meetings")]
[Authorize(Roles = "ADMIN,MANAGER")]
public class MeetingController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmployeeProfileService _employees;

    public MeetingController(IUnitOfWork unitOfWork, IEmployeeProfileService employees)
    {
        _unitOfWork = unitOfWork;
        _employees = employees;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private string? UserTypeClaim => User.FindFirstValue("UserType");

    private async Task ApplyManagerSchoolScopeAsync(MeetingFilterDto filter, CancellationToken cancellationToken)
    {
        if (!string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase))
            return;
        filter.SchoolID = null;
        var uid = CurrentUserId;
        if (string.IsNullOrEmpty(uid))
            return;
        var sid = await _employees.GetSchoolIdForManagerUserAsync(uid, cancellationToken);
        if (sid is int id && id > 0)
            filter.SchoolID = id;
    }

    private async Task<ActionResult<APIResponse>?> EnsureManagerMayAccessMeetingAsync(int meetingId, CancellationToken cancellationToken)
    {
        if (!string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase))
            return null;
        var uid = CurrentUserId;
        if (string.IsNullOrEmpty(uid))
        {
            var u = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Unauthorized };
            u.ErrorMasseges.Add("User id not found on token.");
            return Unauthorized(u);
        }

        var managerSchool = await _employees.GetSchoolIdForManagerUserAsync(uid, cancellationToken);
        if (managerSchool is not int ms || ms <= 0)
        {
            var f = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Forbidden };
            f.ErrorMasseges.Add("No school is linked to this manager account.");
            return StatusCode((int)HttpStatusCode.Forbidden, f);
        }

        var rowSchool = await _unitOfWork.Meetings.GetSchoolIdForMeetingAsync(meetingId, cancellationToken);
        if (rowSchool is null)
        {
            var nf = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.NotFound };
            nf.ErrorMasseges.Add("Meeting was not found.");
            return NotFound(nf);
        }

        if (rowSchool.Value != ms)
        {
            var f = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Forbidden };
            f.ErrorMasseges.Add("You do not have access to this meeting.");
            return StatusCode((int)HttpStatusCode.Forbidden, f);
        }

        return null;
    }

    [HttpPost("list")]
    public async Task<ActionResult<APIResponse>> List([FromBody] MeetingFilterDto? filter, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            filter ??= new MeetingFilterDto();
            await ApplyManagerSchoolScopeAsync(filter, cancellationToken);
            response.Result = await _unitOfWork.Meetings.ListMeetingsAsync(filter, cancellationToken);
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
    public async Task<ActionResult<APIResponse>> Get(int id, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            var gate = await EnsureManagerMayAccessMeetingAsync(id, cancellationToken);
            if (gate != null) return gate;

            var row = await _unitOfWork.Meetings.GetMeetingByIdAsync(id, cancellationToken);
            if (row == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("Meeting was not found.");
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

    [HttpPost]
    public async Task<ActionResult<APIResponse>> Create([FromBody] MeetingWriteDto? dto, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            if (dto == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.BadRequest;
                response.ErrorMasseges.Add("Body is required.");
                return BadRequest(response);
            }

            if (string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase))
            {
                var uid = CurrentUserId;
                var managerSchool = await _employees.GetSchoolIdForManagerUserAsync(uid, cancellationToken);
                if (managerSchool is int ms && ms > 0 && dto.SchoolID != ms)
                {
                    var f = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Forbidden };
                    f.ErrorMasseges.Add("Managers may only create meetings for their own school.");
                    return StatusCode((int)HttpStatusCode.Forbidden, f);
                }
            }

            var id = await _unitOfWork.Meetings.CreateMeetingAsync(dto, cancellationToken);
            response.Result = id;
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

    [HttpPut("{id:int}")]
    public async Task<ActionResult<APIResponse>> Update(int id, [FromBody] MeetingWriteDto? dto, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            if (dto == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.BadRequest;
                response.ErrorMasseges.Add("Body is required.");
                return BadRequest(response);
            }

            var gate = await EnsureManagerMayAccessMeetingAsync(id, cancellationToken);
            if (gate != null) return gate;

            if (string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase))
            {
                var uid = CurrentUserId;
                var managerSchool = await _employees.GetSchoolIdForManagerUserAsync(uid, cancellationToken);
                if (managerSchool is int ms && ms > 0 && dto.SchoolID != ms)
                {
                    var f = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Forbidden };
                    f.ErrorMasseges.Add("Managers may only save meetings for their own school.");
                    return StatusCode((int)HttpStatusCode.Forbidden, f);
                }
            }

            await _unitOfWork.Meetings.UpdateMeetingAsync(id, dto, cancellationToken);
            response.Result = id;
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.NotFound;
            response.ErrorMasseges.Add(ex.Message);
            return NotFound(response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.InternalServerError;
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }

    [HttpPut("{id:int}/minutes")]
    public async Task<ActionResult<APIResponse>> UpsertMinutes(int id, [FromBody] MeetingMinutesWriteDto? dto, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            if (dto == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.BadRequest;
                response.ErrorMasseges.Add("Body is required.");
                return BadRequest(response);
            }

            var gate = await EnsureManagerMayAccessMeetingAsync(id, cancellationToken);
            if (gate != null) return gate;

            await _unitOfWork.Meetings.UpsertMinutesAsync(id, dto, cancellationToken);
            response.Result = id;
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

    [HttpPut("{id:int}/tasks")]
    public async Task<ActionResult<APIResponse>> ReplaceTasks(int id, [FromBody] List<MeetingTaskWriteDto>? tasks, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            tasks ??= new List<MeetingTaskWriteDto>();

            var gate = await EnsureManagerMayAccessMeetingAsync(id, cancellationToken);
            if (gate != null) return gate;

            await _unitOfWork.Meetings.ReplaceTasksAsync(id, tasks, cancellationToken);
            response.Result = id;
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
}
