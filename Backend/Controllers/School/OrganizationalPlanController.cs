using System.Net;
using System.Security.Claims;
using Backend.DTOS.School.OrganizationalPlan;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

/// <summary>Strategic, annual, and department performance planning (distinct from academic course plans).</summary>
[ApiController]
[Route("api/organizational-plans")]
[Authorize(Roles = "ADMIN,MANAGER")]
public class OrganizationalPlanController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmployeeProfileService _employees;

    public OrganizationalPlanController(IUnitOfWork unitOfWork, IEmployeeProfileService employees)
    {
        _unitOfWork = unitOfWork;
        _employees = employees;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private string? UserTypeClaim => User.FindFirstValue("UserType");

    private async Task ApplyManagerSchoolScopeAsync(Action<int> setSchoolId, CancellationToken cancellationToken)
    {
        if (!string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase))
            return;
        var uid = CurrentUserId;
        if (string.IsNullOrEmpty(uid))
            return;
        var sid = await _employees.GetSchoolIdForManagerUserAsync(uid, cancellationToken);
        if (sid is int id && id > 0)
            setSchoolId(id);
    }

    private async Task<ActionResult<APIResponse>?> EnsureManagerSchoolAsync(int entitySchoolId, CancellationToken cancellationToken)
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
        if (entitySchoolId != ms)
        {
            var f = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Forbidden };
            f.ErrorMasseges.Add("You do not have access to this resource.");
            return StatusCode((int)HttpStatusCode.Forbidden, f);
        }
        return null;
    }

    // --- Strategic goals ---

    [HttpPost("strategic-goals/list")]
    public async Task<ActionResult<APIResponse>> ListStrategicGoals([FromBody] StrategicGoalFilterDto? filter, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            filter ??= new StrategicGoalFilterDto();
            await ApplyManagerSchoolScopeAsync(id => filter.SchoolID = id, cancellationToken);
            response.Result = await _unitOfWork.OrganizationalPlans.ListStrategicGoalsAsync(filter, cancellationToken);
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

    [HttpGet("strategic-goals/{id:int}")]
    public async Task<ActionResult<APIResponse>> GetStrategicGoal(int id, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            var row = await _unitOfWork.OrganizationalPlans.GetStrategicGoalByIdAsync(id, cancellationToken);
            if (row == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("Strategic goal was not found.");
                return NotFound(response);
            }
            var gate = await EnsureManagerSchoolAsync(row.SchoolID, cancellationToken);
            if (gate != null) return gate;
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

    [HttpPost("strategic-goals")]
    public async Task<ActionResult<APIResponse>> CreateStrategicGoal([FromBody] StrategicGoalWriteDto? dto, CancellationToken cancellationToken)
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
                    f.ErrorMasseges.Add("Managers may only create records for their own school.");
                    return StatusCode((int)HttpStatusCode.Forbidden, f);
                }
            }
            var id = await _unitOfWork.OrganizationalPlans.CreateStrategicGoalAsync(dto, cancellationToken);
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

    [HttpPut("strategic-goals/{id:int}")]
    public async Task<ActionResult<APIResponse>> UpdateStrategicGoal(int id, [FromBody] StrategicGoalWriteDto? dto, CancellationToken cancellationToken)
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
            var schoolId = await _unitOfWork.OrganizationalPlans.GetSchoolIdForStrategicGoalAsync(id, cancellationToken);
            if (schoolId is null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("Strategic goal was not found.");
                return NotFound(response);
            }
            var gate = await EnsureManagerSchoolAsync(schoolId.Value, cancellationToken);
            if (gate != null) return gate;
            if (string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase))
            {
                var uid = CurrentUserId;
                var managerSchool = await _employees.GetSchoolIdForManagerUserAsync(uid, cancellationToken);
                if (managerSchool is int ms && ms > 0 && dto.SchoolID != ms)
                {
                    var f = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Forbidden };
                    f.ErrorMasseges.Add("Managers may only save records for their own school.");
                    return StatusCode((int)HttpStatusCode.Forbidden, f);
                }
            }
            await _unitOfWork.OrganizationalPlans.UpdateStrategicGoalAsync(id, dto, cancellationToken);
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

    // --- Annual goals ---

    [HttpPost("annual-goals/list")]
    public async Task<ActionResult<APIResponse>> ListAnnualGoals([FromBody] AnnualGoalFilterDto? filter, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            filter ??= new AnnualGoalFilterDto();
            await ApplyManagerSchoolScopeAsync(id => filter.SchoolID = id, cancellationToken);
            response.Result = await _unitOfWork.OrganizationalPlans.ListAnnualGoalsAsync(filter, cancellationToken);
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

    [HttpGet("annual-goals/{id:int}")]
    public async Task<ActionResult<APIResponse>> GetAnnualGoal(int id, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            var row = await _unitOfWork.OrganizationalPlans.GetAnnualGoalByIdAsync(id, cancellationToken);
            if (row == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("Annual goal was not found.");
                return NotFound(response);
            }
            var gate = await EnsureManagerSchoolAsync(row.SchoolID, cancellationToken);
            if (gate != null) return gate;
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

    [HttpPost("annual-goals")]
    public async Task<ActionResult<APIResponse>> CreateAnnualGoal([FromBody] AnnualGoalWriteDto? dto, CancellationToken cancellationToken)
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
                    f.ErrorMasseges.Add("Managers may only create records for their own school.");
                    return StatusCode((int)HttpStatusCode.Forbidden, f);
                }
            }
            var id = await _unitOfWork.OrganizationalPlans.CreateAnnualGoalAsync(dto, cancellationToken);
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

    [HttpPut("annual-goals/{id:int}")]
    public async Task<ActionResult<APIResponse>> UpdateAnnualGoal(int id, [FromBody] AnnualGoalWriteDto? dto, CancellationToken cancellationToken)
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
            var schoolId = await _unitOfWork.OrganizationalPlans.GetSchoolIdForAnnualGoalAsync(id, cancellationToken);
            if (schoolId is null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("Annual goal was not found.");
                return NotFound(response);
            }
            var gate = await EnsureManagerSchoolAsync(schoolId.Value, cancellationToken);
            if (gate != null) return gate;
            if (string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase))
            {
                var uid = CurrentUserId;
                var managerSchool = await _employees.GetSchoolIdForManagerUserAsync(uid, cancellationToken);
                if (managerSchool is int ms && ms > 0 && dto.SchoolID != ms)
                {
                    var f = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Forbidden };
                    f.ErrorMasseges.Add("Managers may only save records for their own school.");
                    return StatusCode((int)HttpStatusCode.Forbidden, f);
                }
            }
            await _unitOfWork.OrganizationalPlans.UpdateAnnualGoalAsync(id, dto, cancellationToken);
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

    // --- Department goals ---

    [HttpPost("department-goals/list")]
    public async Task<ActionResult<APIResponse>> ListDepartmentGoals([FromBody] DepartmentGoalFilterDto? filter, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            filter ??= new DepartmentGoalFilterDto();
            await ApplyManagerSchoolScopeAsync(id => filter.SchoolID = id, cancellationToken);
            response.Result = await _unitOfWork.OrganizationalPlans.ListDepartmentGoalsAsync(filter, cancellationToken);
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

    [HttpGet("department-goals/{id:int}")]
    public async Task<ActionResult<APIResponse>> GetDepartmentGoal(int id, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            var row = await _unitOfWork.OrganizationalPlans.GetDepartmentGoalByIdAsync(id, cancellationToken);
            if (row == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("Department goal was not found.");
                return NotFound(response);
            }
            var gate = await EnsureManagerSchoolAsync(row.SchoolID, cancellationToken);
            if (gate != null) return gate;
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

    [HttpPost("department-goals")]
    public async Task<ActionResult<APIResponse>> CreateDepartmentGoal([FromBody] DepartmentGoalWriteDto? dto, CancellationToken cancellationToken)
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
                    f.ErrorMasseges.Add("Managers may only create records for their own school.");
                    return StatusCode((int)HttpStatusCode.Forbidden, f);
                }
            }
            var id = await _unitOfWork.OrganizationalPlans.CreateDepartmentGoalAsync(dto, cancellationToken);
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

    [HttpPut("department-goals/{id:int}")]
    public async Task<ActionResult<APIResponse>> UpdateDepartmentGoal(int id, [FromBody] DepartmentGoalWriteDto? dto, CancellationToken cancellationToken)
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
            var schoolId = await _unitOfWork.OrganizationalPlans.GetSchoolIdForDepartmentGoalAsync(id, cancellationToken);
            if (schoolId is null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("Department goal was not found.");
                return NotFound(response);
            }
            var gate = await EnsureManagerSchoolAsync(schoolId.Value, cancellationToken);
            if (gate != null) return gate;
            if (string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase))
            {
                var uid = CurrentUserId;
                var managerSchool = await _employees.GetSchoolIdForManagerUserAsync(uid, cancellationToken);
                if (managerSchool is int ms && ms > 0 && dto.SchoolID != ms)
                {
                    var f = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Forbidden };
                    f.ErrorMasseges.Add("Managers may only save records for their own school.");
                    return StatusCode((int)HttpStatusCode.Forbidden, f);
                }
            }
            await _unitOfWork.OrganizationalPlans.UpdateDepartmentGoalAsync(id, dto, cancellationToken);
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
