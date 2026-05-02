using System.Net;
using System.Security.Claims;
using Backend.DTOS.School.Concern;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

/// <summary>Complaints and suggestions (concerns) with categories and action logs.</summary>
[ApiController]
[Route("api/concerns")]
[Authorize(Roles = "ADMIN,MANAGER")]
public class ConcernController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmployeeProfileService _employees;

    public ConcernController(IUnitOfWork unitOfWork, IEmployeeProfileService employees)
    {
        _unitOfWork = unitOfWork;
        _employees = employees;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private string? UserTypeClaim => User.FindFirstValue("UserType");

    private async Task ApplyManagerSchoolScopeAsync(ConcernFilterDto filter, CancellationToken cancellationToken)
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

    private async Task<ActionResult<APIResponse>?> EnsureManagerMayAccessComplaintAsync(int complaintId, CancellationToken cancellationToken)
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

        var rowSchool = await _unitOfWork.Concerns.GetSchoolIdForComplaintAsync(complaintId, cancellationToken);
        if (rowSchool is null)
        {
            var nf = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.NotFound };
            nf.ErrorMasseges.Add("Complaint was not found.");
            return NotFound(nf);
        }

        if (rowSchool.Value != ms)
        {
            var f = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Forbidden };
            f.ErrorMasseges.Add("You do not have access to this complaint.");
            return StatusCode((int)HttpStatusCode.Forbidden, f);
        }

        return null;
    }

    private async Task<ActionResult<APIResponse>?> EnsureManagerMayAccessSuggestionAsync(int suggestionId, CancellationToken cancellationToken)
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

        var rowSchool = await _unitOfWork.Concerns.GetSchoolIdForSuggestionAsync(suggestionId, cancellationToken);
        if (rowSchool is null)
        {
            var nf = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.NotFound };
            nf.ErrorMasseges.Add("Suggestion was not found.");
            return NotFound(nf);
        }

        if (rowSchool.Value != ms)
        {
            var f = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Forbidden };
            f.ErrorMasseges.Add("You do not have access to this suggestion.");
            return StatusCode((int)HttpStatusCode.Forbidden, f);
        }

        return null;
    }

    [HttpPost("categories/list")]
    public async Task<ActionResult<APIResponse>> ListCategories([FromBody] ConcernCategoriesFilterDto? body, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            if (body == null || body.SchoolID <= 0)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.BadRequest;
                response.ErrorMasseges.Add("SchoolID is required.");
                return BadRequest(response);
            }

            if (string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase))
            {
                var uid = CurrentUserId;
                var managerSchool = await _employees.GetSchoolIdForManagerUserAsync(uid, cancellationToken);
                if (managerSchool is int ms && ms > 0 && body.SchoolID != ms)
                {
                    var f = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Forbidden };
                    f.ErrorMasseges.Add("Managers may only load categories for their own school.");
                    return StatusCode((int)HttpStatusCode.Forbidden, f);
                }
            }

            response.Result = await _unitOfWork.Concerns.ListCategoriesAsync(body, cancellationToken);
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

    [HttpPost("complaints/list")]
    public async Task<ActionResult<APIResponse>> ListComplaints([FromBody] ConcernFilterDto? filter, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            filter ??= new ConcernFilterDto();
            await ApplyManagerSchoolScopeAsync(filter, cancellationToken);
            response.Result = await _unitOfWork.Concerns.ListComplaintsAsync(filter, cancellationToken);
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

    [HttpGet("complaints/{id:int}")]
    public async Task<ActionResult<APIResponse>> GetComplaint(int id, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            var gate = await EnsureManagerMayAccessComplaintAsync(id, cancellationToken);
            if (gate != null) return gate;

            var row = await _unitOfWork.Concerns.GetComplaintByIdAsync(id, cancellationToken);
            if (row == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("Complaint was not found.");
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

    [HttpPost("complaints")]
    public async Task<ActionResult<APIResponse>> CreateComplaint([FromBody] ComplaintWriteDto? dto, CancellationToken cancellationToken)
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
                    f.ErrorMasseges.Add("Managers may only create complaints for their own school.");
                    return StatusCode((int)HttpStatusCode.Forbidden, f);
                }
            }

            var actor = await _unitOfWork.Concerns.GetEmployeeProfileIdForUserInSchoolAsync(CurrentUserId, dto.SchoolID, cancellationToken);
            var id = await _unitOfWork.Concerns.CreateComplaintAsync(dto, actor, cancellationToken);
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

    [HttpPut("complaints/{id:int}")]
    public async Task<ActionResult<APIResponse>> UpdateComplaint(int id, [FromBody] ComplaintWriteDto? dto, CancellationToken cancellationToken)
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

            var gate = await EnsureManagerMayAccessComplaintAsync(id, cancellationToken);
            if (gate != null) return gate;

            if (string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase))
            {
                var uid = CurrentUserId;
                var managerSchool = await _employees.GetSchoolIdForManagerUserAsync(uid, cancellationToken);
                if (managerSchool is int ms && ms > 0 && dto.SchoolID != ms)
                {
                    var f = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Forbidden };
                    f.ErrorMasseges.Add("Managers may only save complaints for their own school.");
                    return StatusCode((int)HttpStatusCode.Forbidden, f);
                }
            }

            var actor = await _unitOfWork.Concerns.GetEmployeeProfileIdForUserInSchoolAsync(CurrentUserId, dto.SchoolID, cancellationToken);
            await _unitOfWork.Concerns.UpdateComplaintAsync(id, dto, actor, cancellationToken);
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

    [HttpPost("suggestions/list")]
    public async Task<ActionResult<APIResponse>> ListSuggestions([FromBody] ConcernFilterDto? filter, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            filter ??= new ConcernFilterDto();
            await ApplyManagerSchoolScopeAsync(filter, cancellationToken);
            response.Result = await _unitOfWork.Concerns.ListSuggestionsAsync(filter, cancellationToken);
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

    [HttpGet("suggestions/{id:int}")]
    public async Task<ActionResult<APIResponse>> GetSuggestion(int id, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            var gate = await EnsureManagerMayAccessSuggestionAsync(id, cancellationToken);
            if (gate != null) return gate;

            var row = await _unitOfWork.Concerns.GetSuggestionByIdAsync(id, cancellationToken);
            if (row == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("Suggestion was not found.");
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

    [HttpPost("suggestions")]
    public async Task<ActionResult<APIResponse>> CreateSuggestion([FromBody] SuggestionWriteDto? dto, CancellationToken cancellationToken)
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
                    f.ErrorMasseges.Add("Managers may only create suggestions for their own school.");
                    return StatusCode((int)HttpStatusCode.Forbidden, f);
                }
            }

            var actor = await _unitOfWork.Concerns.GetEmployeeProfileIdForUserInSchoolAsync(CurrentUserId, dto.SchoolID, cancellationToken);
            var id = await _unitOfWork.Concerns.CreateSuggestionAsync(dto, actor, cancellationToken);
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

    [HttpPut("suggestions/{id:int}")]
    public async Task<ActionResult<APIResponse>> UpdateSuggestion(int id, [FromBody] SuggestionWriteDto? dto, CancellationToken cancellationToken)
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

            var gate = await EnsureManagerMayAccessSuggestionAsync(id, cancellationToken);
            if (gate != null) return gate;

            if (string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase))
            {
                var uid = CurrentUserId;
                var managerSchool = await _employees.GetSchoolIdForManagerUserAsync(uid, cancellationToken);
                if (managerSchool is int ms && ms > 0 && dto.SchoolID != ms)
                {
                    var f = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Forbidden };
                    f.ErrorMasseges.Add("Managers may only save suggestions for their own school.");
                    return StatusCode((int)HttpStatusCode.Forbidden, f);
                }
            }

            var actor = await _unitOfWork.Concerns.GetEmployeeProfileIdForUserInSchoolAsync(CurrentUserId, dto.SchoolID, cancellationToken);
            await _unitOfWork.Concerns.UpdateSuggestionAsync(id, dto, actor, cancellationToken);
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
}
