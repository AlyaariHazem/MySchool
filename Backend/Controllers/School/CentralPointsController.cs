using System.Net;
using System.Security.Claims;
using Backend.DTOS.School.CentralPoints;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

/// <summary>
/// Central staff points: sources, rules, ledger lines, balances, and postings from any HR module.
/// </summary>
[ApiController]
[Route("api/central-points")]
[Authorize(Roles = "ADMIN,MANAGER")]
public class CentralPointsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmployeeProfileService _employees;

    public CentralPointsController(IUnitOfWork unitOfWork, IEmployeeProfileService employees)
    {
        _unitOfWork = unitOfWork;
        _employees = employees;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private string? UserTypeClaim => User.FindFirstValue("UserType");

    private async Task ApplyManagerSchoolFilterAsync(PointsRuleFilterDto filter, CancellationToken cancellationToken)
    {
        if (!string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase))
            return;
        var uid = CurrentUserId;
        if (string.IsNullOrEmpty(uid))
            return;
        var sid = await _employees.GetSchoolIdForManagerUserAsync(uid, cancellationToken);
        if (sid is int id && id > 0)
            filter.SchoolID = id;
    }

    private async Task ApplyManagerSchoolFilterAsync(PointsLedgerFilterDto filter, CancellationToken cancellationToken)
    {
        if (!string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase))
            return;
        var uid = CurrentUserId;
        if (string.IsNullOrEmpty(uid))
            return;
        var sid = await _employees.GetSchoolIdForManagerUserAsync(uid, cancellationToken);
        if (sid is int id && id > 0)
            filter.SchoolID = id;
    }

    private async Task<ActionResult<APIResponse>?> EnsureManagerMayAccessSchoolAsync(int schoolId, CancellationToken cancellationToken)
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

        if (ms != schoolId)
        {
            var f = new APIResponse { IsSuccess = false, statusCode = HttpStatusCode.Forbidden };
            f.ErrorMasseges.Add("You do not have access to this school.");
            return StatusCode((int)HttpStatusCode.Forbidden, f);
        }

        return null;
    }

    [HttpGet("sources")]
    public async Task<ActionResult<APIResponse>> ListSources(CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            response.Result = await _unitOfWork.CentralPoints.ListSourcesAsync(activeOnly: true, cancellationToken);
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

    [HttpPost("rules/list")]
    public async Task<ActionResult<APIResponse>> ListRules([FromBody] PointsRuleFilterDto? filter, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            filter ??= new PointsRuleFilterDto();
            await ApplyManagerSchoolFilterAsync(filter, cancellationToken);
            response.Result = await _unitOfWork.CentralPoints.ListRulesAsync(filter, cancellationToken);
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

    [HttpPost("rules")]
    public async Task<ActionResult<APIResponse>> CreateRule([FromBody] PointsRuleWriteDto dto, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            if (string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase))
            {
                var uid = CurrentUserId;
                var ms = await _employees.GetSchoolIdForManagerUserAsync(uid, cancellationToken);
                if (ms is not int sid || sid <= 0)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.Forbidden;
                    response.ErrorMasseges.Add("No school is linked to this manager account.");
                    return StatusCode((int)HttpStatusCode.Forbidden, response);
                }

                dto.SchoolID = sid;
            }

            var id = await _unitOfWork.CentralPoints.CreateRuleAsync(dto, cancellationToken);
            response.Result = new { pointsRuleID = id };
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

    [HttpPut("rules/{id:int}")]
    public async Task<ActionResult<APIResponse>> UpdateRule([FromRoute] int id, [FromBody] PointsRuleWriteDto dto, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            int? managerSchoolOnly = null;
            if (string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase))
            {
                var uid = CurrentUserId;
                var ms = await _employees.GetSchoolIdForManagerUserAsync(uid, cancellationToken);
                if (ms is not int sid || sid <= 0)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.Forbidden;
                    response.ErrorMasseges.Add("No school is linked to this manager account.");
                    return StatusCode((int)HttpStatusCode.Forbidden, response);
                }

                dto.SchoolID = sid;
                managerSchoolOnly = sid;
            }

            await _unitOfWork.CentralPoints.UpdateRuleAsync(id, dto, managerSchoolOnly, cancellationToken);
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

    [HttpPost("ledger/list")]
    public async Task<ActionResult<APIResponse>> ListLedger([FromBody] PointsLedgerFilterDto? filter, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            filter ??= new PointsLedgerFilterDto();
            await ApplyManagerSchoolFilterAsync(filter, cancellationToken);
            var (items, total) = await _unitOfWork.CentralPoints.ListLedgerAsync(filter, cancellationToken);
            response.Result = new { items, totalCount = total };
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

    [HttpGet("balance")]
    public async Task<ActionResult<APIResponse>> GetBalance(
        [FromQuery] int employeeProfileId,
        [FromQuery] int schoolId,
        CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            var denied = await EnsureManagerMayAccessSchoolAsync(schoolId, cancellationToken);
            if (denied != null)
                return denied;

            response.Result = await _unitOfWork.CentralPoints.GetBalanceAsync(employeeProfileId, schoolId, cancellationToken);
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

    [HttpPost("post")]
    public async Task<ActionResult<APIResponse>> Post([FromBody] PostCentralPointsDto dto, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            var denied = await EnsureManagerMayAccessSchoolAsync(dto.SchoolID, cancellationToken);
            if (denied != null)
                return denied;

            int? postedBy = null;
            if (!string.IsNullOrEmpty(CurrentUserId))
                postedBy = await _unitOfWork.Concerns.GetEmployeeProfileIdForUserInSchoolAsync(CurrentUserId, dto.SchoolID, cancellationToken);

            response.Result = await _unitOfWork.CentralPoints.PostAsync(dto, postedBy, cancellationToken);
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

    [HttpPost("rebuild-balance")]
    public async Task<ActionResult<APIResponse>> RebuildBalance(
        [FromQuery] int employeeProfileId,
        [FromQuery] int schoolId,
        CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            if (!User.IsInRole("ADMIN") && !string.Equals(UserTypeClaim, "ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                response.ErrorMasseges.Add("Only administrators can rebuild balance snapshots.");
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }

            var total = await _unitOfWork.CentralPoints.RebuildBalanceSnapshotAsync(employeeProfileId, schoolId, cancellationToken);
            response.Result = new { totalPoints = total };
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
