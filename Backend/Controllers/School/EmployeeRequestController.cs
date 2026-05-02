using System.Net;
using System.Security.Claims;
using Backend.DTOS.School.EmployeeRequest;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

/// <summary>Employee operational requests (tools, advance, support) with approvals, execution tracking, and daily summaries.</summary>
[ApiController]
[Route("api/employee-requests")]
[Authorize(Roles = "ADMIN,MANAGER")]
public class EmployeeRequestController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmployeeProfileService _employees;

    public EmployeeRequestController(IUnitOfWork unitOfWork, IEmployeeProfileService employees)
    {
        _unitOfWork = unitOfWork;
        _employees = employees;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private string? UserTypeClaim => User.FindFirstValue("UserType");

    private async Task ApplyManagerSchoolScopeAsync(EmployeeRequestFilterDto filter, CancellationToken cancellationToken)
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

    private async Task<ActionResult<APIResponse>?> EnsureManagerMayAccessRequestSchoolAsync(int requestId, CancellationToken cancellationToken)
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

        var rowSchool = await _unitOfWork.EmployeeRequests.GetSchoolIdForRequestAsync(requestId, cancellationToken);
        if (rowSchool is null)
        {
            var nf = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.NotFound };
            nf.ErrorMasseges.Add("Employee request was not found.");
            return NotFound(nf);
        }

        if (rowSchool.Value != ms)
        {
            var f = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Forbidden };
            f.ErrorMasseges.Add("You do not have access to this request.");
            return StatusCode((int)HttpStatusCode.Forbidden, f);
        }

        return null;
    }

    [HttpPost("types/list")]
    public async Task<ActionResult<APIResponse>> ListTypes([FromBody] EmployeeRequestTypesFilterDto? body, CancellationToken cancellationToken)
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
                    f.ErrorMasseges.Add("Managers may only load types for their own school.");
                    return StatusCode((int)HttpStatusCode.Forbidden, f);
                }
            }

            response.Result = await _unitOfWork.EmployeeRequests.ListTypesAsync(body.SchoolID, cancellationToken);
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

    [HttpPost("list")]
    public async Task<ActionResult<APIResponse>> List([FromBody] EmployeeRequestFilterDto? filter, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            filter ??= new EmployeeRequestFilterDto();
            await ApplyManagerSchoolScopeAsync(filter, cancellationToken);
            response.Result = await _unitOfWork.EmployeeRequests.ListAsync(filter, cancellationToken);
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
            var gate = await EnsureManagerMayAccessRequestSchoolAsync(id, cancellationToken);
            if (gate != null) return gate;

            var row = await _unitOfWork.EmployeeRequests.GetByIdAsync(id, cancellationToken);
            if (row == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("Employee request was not found.");
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
    public async Task<ActionResult<APIResponse>> Create([FromBody] EmployeeRequestWriteDto? dto, CancellationToken cancellationToken)
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
                    f.ErrorMasseges.Add("Managers may only create requests for their own school.");
                    return StatusCode((int)HttpStatusCode.Forbidden, f);
                }
            }

            var id = await _unitOfWork.EmployeeRequests.CreateAsync(dto, cancellationToken);
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
    public async Task<ActionResult<APIResponse>> Update(int id, [FromBody] EmployeeRequestWriteDto? dto, CancellationToken cancellationToken)
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

            var gate = await EnsureManagerMayAccessRequestSchoolAsync(id, cancellationToken);
            if (gate != null) return gate;

            if (string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase))
            {
                var uid = CurrentUserId;
                var managerSchool = await _employees.GetSchoolIdForManagerUserAsync(uid, cancellationToken);
                if (managerSchool is int ms && ms > 0 && dto.SchoolID != ms)
                {
                    var f = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Forbidden };
                    f.ErrorMasseges.Add("Managers may only save requests for their own school.");
                    return StatusCode((int)HttpStatusCode.Forbidden, f);
                }
            }

            await _unitOfWork.EmployeeRequests.UpdateAsync(id, dto, cancellationToken);
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

    [HttpPost("{id:int}/executions")]
    public async Task<ActionResult<APIResponse>> AddExecution(int id, [FromBody] EmployeeRequestExecutionWriteDto? dto, CancellationToken cancellationToken)
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

            var gate = await EnsureManagerMayAccessRequestSchoolAsync(id, cancellationToken);
            if (gate != null) return gate;

            var newId = await _unitOfWork.EmployeeRequests.AddExecutionAsync(id, dto, cancellationToken);
            response.Result = newId;
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

    [HttpPost("{id:int}/daily-summaries")]
    public async Task<ActionResult<APIResponse>> AddDailySummary(int id, [FromBody] EmployeeRequestDailySummaryWriteDto? dto, CancellationToken cancellationToken)
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

            var gate = await EnsureManagerMayAccessRequestSchoolAsync(id, cancellationToken);
            if (gate != null) return gate;

            var newId = await _unitOfWork.EmployeeRequests.AddDailySummaryAsync(id, dto, cancellationToken);
            response.Result = newId;
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

    [HttpPost("{id:int}/approval-steps")]
    public async Task<ActionResult<APIResponse>> AddApprovalStep(int id, [FromBody] EmployeeRequestApprovalStepWriteDto? dto, CancellationToken cancellationToken)
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

            var gate = await EnsureManagerMayAccessRequestSchoolAsync(id, cancellationToken);
            if (gate != null) return gate;

            var newId = await _unitOfWork.EmployeeRequests.AddApprovalStepAsync(id, dto, cancellationToken);
            response.Result = newId;
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

    [HttpPost("{id:int}/approval-steps/{stepId:int}/decide")]
    public async Task<ActionResult<APIResponse>> DecideApprovalStep(int id, int stepId, [FromBody] EmployeeRequestApprovalDecideDto? dto, CancellationToken cancellationToken)
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

            var gate = await EnsureManagerMayAccessRequestSchoolAsync(id, cancellationToken);
            if (gate != null) return gate;

            await _unitOfWork.EmployeeRequests.DecideApprovalStepAsync(id, stepId, dto, cancellationToken);
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
