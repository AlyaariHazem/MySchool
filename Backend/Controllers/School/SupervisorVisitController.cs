using System.Net;
using System.Security.Claims;
using Backend.DTOS.School.SupervisorVisit;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

/// <summary>Supervisor classroom visits: observations, recommendations, follow-ups, score out of 100.</summary>
[ApiController]
[Route("api/supervisor-visits")]
[Authorize(Roles = "ADMIN,MANAGER")]
public class SupervisorVisitController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmployeeProfileService _employees;

    public SupervisorVisitController(IUnitOfWork unitOfWork, IEmployeeProfileService employees)
    {
        _unitOfWork = unitOfWork;
        _employees = employees;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private string? UserTypeClaim => User.FindFirstValue("UserType");

    private async Task ApplyManagerSchoolScopeAsync(SupervisorVisitFilterDto filter, CancellationToken cancellationToken)
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

    private async Task<ActionResult<APIResponse>?> EnsureManagerMayAccessVisitSchoolAsync(int visitId, CancellationToken cancellationToken)
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

        var visitSchool = await _unitOfWork.SupervisorVisits.GetSchoolIdForVisitAsync(visitId, cancellationToken);
        if (visitSchool is null)
        {
            var nf = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.NotFound };
            nf.ErrorMasseges.Add("Visit was not found.");
            return NotFound(nf);
        }

        if (visitSchool.Value != ms)
        {
            var f = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Forbidden };
            f.ErrorMasseges.Add("You do not have access to this visit.");
            return StatusCode((int)HttpStatusCode.Forbidden, f);
        }

        return null;
    }

    /// <summary>List visits (optional filters). Managers are scoped to their school.</summary>
    [HttpPost("list")]
    public async Task<ActionResult<APIResponse>> List([FromBody] SupervisorVisitFilterDto? filter, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            filter ??= new SupervisorVisitFilterDto();
            await ApplyManagerSchoolScopeAsync(filter, cancellationToken);
            response.Result = await _unitOfWork.SupervisorVisits.ListAsync(filter, cancellationToken);
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
    public async Task<ActionResult<APIResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            var gate = await EnsureManagerMayAccessVisitSchoolAsync(id, cancellationToken);
            if (gate != null) return gate;

            var row = await _unitOfWork.SupervisorVisits.GetByIdAsync(id, cancellationToken);
            if (row == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("Visit was not found.");
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
    public async Task<ActionResult<APIResponse>> Create([FromBody] SupervisorVisitWriteDto? dto, CancellationToken cancellationToken)
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
                    f.ErrorMasseges.Add("Managers may only create visits for their own school.");
                    return StatusCode((int)HttpStatusCode.Forbidden, f);
                }
            }

            var id = await _unitOfWork.SupervisorVisits.CreateAsync(dto, cancellationToken);
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

    [HttpPut("{id:int}")]
    public async Task<ActionResult<APIResponse>> Update(int id, [FromBody] SupervisorVisitWriteDto? dto, CancellationToken cancellationToken)
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

            var gate = await EnsureManagerMayAccessVisitSchoolAsync(id, cancellationToken);
            if (gate != null) return gate;

            if (string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase))
            {
                var uid = CurrentUserId;
                var managerSchool = await _employees.GetSchoolIdForManagerUserAsync(uid, cancellationToken);
                if (managerSchool is int ms && ms > 0 && dto.SchoolID != ms)
                {
                    var f = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Forbidden };
                    f.ErrorMasseges.Add("Managers may only save visits for their own school.");
                    return StatusCode((int)HttpStatusCode.Forbidden, f);
                }
            }

            await _unitOfWork.SupervisorVisits.UpdateAsync(id, dto, cancellationToken);
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

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<APIResponse>> Delete(int id, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            var gate = await EnsureManagerMayAccessVisitSchoolAsync(id, cancellationToken);
            if (gate != null) return gate;

            var ok = await _unitOfWork.SupervisorVisits.DeleteAsync(id, cancellationToken);
            if (!ok)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("Visit was not found.");
                return NotFound(response);
            }

            response.Result = true;
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
