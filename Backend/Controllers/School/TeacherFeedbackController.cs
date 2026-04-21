using System.Net;
using System.Security.Claims;
using Backend.DTOS.School.TeacherFeedback;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

/// <summary>Teacher performance feedback cycles answered by students and parents (Guardians).</summary>
[ApiController]
[Route("api/teacher-feedback")]
public class TeacherFeedbackController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmployeeProfileService _employees;

    public TeacherFeedbackController(IUnitOfWork unitOfWork, IEmployeeProfileService employees)
    {
        _unitOfWork = unitOfWork;
        _employees = employees;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private string? UserTypeClaim => User.FindFirstValue("UserType");

    private async Task ApplyManagerSchoolScopeAsync(TeacherFeedbackCycleFilterDto filter, CancellationToken cancellationToken)
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

    private async Task<ActionResult<APIResponse>?> EnsureManagerMayAccessCycleSchoolAsync(int cycleId, CancellationToken cancellationToken)
    {
        if (!string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase))
            return null;
        var uid = CurrentUserId;
        if (string.IsNullOrEmpty(uid))
            return Unauthorized(APIResponse.Fail("User id not found on token.", HttpStatusCode.Unauthorized));

        var managerSchool = await _employees.GetSchoolIdForManagerUserAsync(uid, cancellationToken);
        if (managerSchool is not int ms || ms <= 0)
            return StatusCode((int)HttpStatusCode.Forbidden, APIResponse.Fail("No school is linked to this manager account.", HttpStatusCode.Forbidden));

        var cycleSchool = await _unitOfWork.TeacherFeedback.GetSchoolIdForCycleAsync(cycleId, cancellationToken);
        if (cycleSchool is null)
            return NotFound(APIResponse.Fail("Cycle was not found.", HttpStatusCode.NotFound));
        if (cycleSchool.Value != ms)
            return StatusCode((int)HttpStatusCode.Forbidden, APIResponse.Fail("You do not have access to this cycle.", HttpStatusCode.Forbidden));

        return null;
    }

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpPost("cycles/list")]
    public async Task<ActionResult<APIResponse>> ListCycles([FromBody] TeacherFeedbackCycleFilterDto? filter, CancellationToken cancellationToken)
    {
        filter ??= new TeacherFeedbackCycleFilterDto();
        await ApplyManagerSchoolScopeAsync(filter, cancellationToken);
        var rows = await _unitOfWork.TeacherFeedback.ListCyclesAsync(filter, cancellationToken);
        return Ok(APIResponse.Success(rows));
    }

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpGet("cycles/{id:int}")]
    public async Task<ActionResult<APIResponse>> GetCycle(int id, CancellationToken cancellationToken)
    {
        var denied = await EnsureManagerMayAccessCycleSchoolAsync(id, cancellationToken);
        if (denied != null) return denied;

        var row = await _unitOfWork.TeacherFeedback.GetCycleByIdAsync(id, cancellationToken);
        if (row == null)
            return NotFound(APIResponse.Fail("Cycle not found.", HttpStatusCode.NotFound));
        return Ok(APIResponse.Success(row));
    }

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpPost("cycles")]
    public async Task<ActionResult<APIResponse>> CreateCycle([FromBody] TeacherFeedbackCycleWriteDto? dto, CancellationToken cancellationToken)
    {
        if (dto == null)
            return BadRequest(APIResponse.Fail("Body is required.", HttpStatusCode.BadRequest));
        if (string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase))
        {
            var uid = CurrentUserId;
            var managerSchool = await _employees.GetSchoolIdForManagerUserAsync(uid!, cancellationToken);
            if (managerSchool is int ms && ms > 0 && dto.SchoolID != ms)
                return StatusCode((int)HttpStatusCode.Forbidden, APIResponse.Fail("Managers may only create cycles for their own school.", HttpStatusCode.Forbidden));
        }
        try
        {
            var id = await _unitOfWork.TeacherFeedback.CreateCycleAsync(dto, cancellationToken);
            await _unitOfWork.CompleteAsync();
            return Ok(APIResponse.Success(new { teacherFeedbackCycleID = id }));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(APIResponse.Fail(ex.Message, HttpStatusCode.BadRequest));
        }
    }

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpPut("cycles/{id:int}")]
    public async Task<ActionResult<APIResponse>> UpdateCycle(int id, [FromBody] TeacherFeedbackCycleWriteDto? dto, CancellationToken cancellationToken)
    {
        if (dto == null)
            return BadRequest(APIResponse.Fail("Body is required.", HttpStatusCode.BadRequest));
        var denied = await EnsureManagerMayAccessCycleSchoolAsync(id, cancellationToken);
        if (denied != null) return denied;
        if (string.Equals(UserTypeClaim, "MANAGER", StringComparison.OrdinalIgnoreCase))
        {
            var uid = CurrentUserId;
            var managerSchool = await _employees.GetSchoolIdForManagerUserAsync(uid!, cancellationToken);
            if (managerSchool is int ms && ms > 0 && dto.SchoolID != ms)
                return StatusCode((int)HttpStatusCode.Forbidden, APIResponse.Fail("Managers may only update cycles for their own school.", HttpStatusCode.Forbidden));
        }
        try
        {
            await _unitOfWork.TeacherFeedback.UpdateCycleAsync(id, dto, cancellationToken);
            await _unitOfWork.CompleteAsync();
            return Ok(APIResponse.Success("updated"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(APIResponse.Fail(ex.Message, HttpStatusCode.BadRequest));
        }
    }

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpDelete("cycles/{id:int}")]
    public async Task<ActionResult<APIResponse>> DeleteCycle(int id, CancellationToken cancellationToken)
    {
        var denied = await EnsureManagerMayAccessCycleSchoolAsync(id, cancellationToken);
        if (denied != null) return denied;
        var ok = await _unitOfWork.TeacherFeedback.DeleteCycleAsync(id, cancellationToken);
        await _unitOfWork.CompleteAsync();
        if (!ok)
            return NotFound(APIResponse.Fail("Cycle not found.", HttpStatusCode.NotFound));
        return Ok(APIResponse.Success("deleted"));
    }

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpPost("cycles/{id:int}/recompute-summaries")]
    public async Task<ActionResult<APIResponse>> RecomputeSummaries(int id, CancellationToken cancellationToken)
    {
        var denied = await EnsureManagerMayAccessCycleSchoolAsync(id, cancellationToken);
        if (denied != null) return denied;
        await _unitOfWork.TeacherFeedback.RecomputeSummariesAsync(id, cancellationToken);
        await _unitOfWork.CompleteAsync();
        return Ok(APIResponse.Success("summaries updated"));
    }

    [Authorize(Roles = "STUDENT")]
    [HttpGet("student/open-cycles")]
    public async Task<ActionResult<APIResponse>> StudentOpenCycles(CancellationToken cancellationToken)
    {
        var uid = CurrentUserId;
        if (string.IsNullOrEmpty(uid))
            return Unauthorized(APIResponse.Fail("User id not found on token.", HttpStatusCode.Unauthorized));
        var studentId = await _unitOfWork.Homework.GetStudentIdByUserIdAsync(uid, cancellationToken);
        if (studentId is null or <= 0)
            return StatusCode((int)HttpStatusCode.Forbidden, APIResponse.Fail("No student profile for this user.", HttpStatusCode.Forbidden));
        var rows = await _unitOfWork.TeacherFeedback.ListOpenCyclesForStudentAsync(studentId.Value, cancellationToken);
        return Ok(APIResponse.Success(rows));
    }

    [Authorize(Roles = "STUDENT")]
    [HttpGet("student/cycles/{id:int}/form")]
    public async Task<ActionResult<APIResponse>> StudentCycleForm(int id, CancellationToken cancellationToken)
    {
        var uid = CurrentUserId;
        if (string.IsNullOrEmpty(uid))
            return Unauthorized(APIResponse.Fail("User id not found on token.", HttpStatusCode.Unauthorized));
        var studentId = await _unitOfWork.Homework.GetStudentIdByUserIdAsync(uid, cancellationToken);
        if (studentId is null or <= 0)
            return StatusCode((int)HttpStatusCode.Forbidden, APIResponse.Fail("No student profile for this user.", HttpStatusCode.Forbidden));
        var form = await _unitOfWork.TeacherFeedback.GetStudentCycleFormAsync(studentId.Value, id, cancellationToken);
        if (form == null)
            return NotFound(APIResponse.Fail("Cycle not found or not open.", HttpStatusCode.NotFound));
        return Ok(APIResponse.Success(form));
    }

    [Authorize(Roles = "GUARDIAN")]
    [HttpGet("parent/open-cycles")]
    public async Task<ActionResult<APIResponse>> ParentOpenCycles(CancellationToken cancellationToken)
    {
        var uid = CurrentUserId;
        if (string.IsNullOrEmpty(uid))
            return Unauthorized(APIResponse.Fail("User id not found on token.", HttpStatusCode.Unauthorized));
        var guardianId = await _unitOfWork.Homework.GetGuardianIdByUserIdAsync(uid, cancellationToken);
        if (guardianId is null or <= 0)
            return StatusCode((int)HttpStatusCode.Forbidden, APIResponse.Fail("No guardian profile for this user.", HttpStatusCode.Forbidden));
        var rows = await _unitOfWork.TeacherFeedback.ListOpenCyclesForGuardianAsync(guardianId.Value, cancellationToken);
        return Ok(APIResponse.Success(rows));
    }

    [Authorize(Roles = "GUARDIAN")]
    [HttpGet("parent/cycles/{id:int}/form")]
    public async Task<ActionResult<APIResponse>> ParentCycleForm(int id, [FromQuery] int studentId, CancellationToken cancellationToken)
    {
        if (studentId <= 0)
            return BadRequest(APIResponse.Fail("studentId query is required.", HttpStatusCode.BadRequest));
        var uid = CurrentUserId;
        if (string.IsNullOrEmpty(uid))
            return Unauthorized(APIResponse.Fail("User id not found on token.", HttpStatusCode.Unauthorized));
        var guardianId = await _unitOfWork.Homework.GetGuardianIdByUserIdAsync(uid, cancellationToken);
        if (guardianId is null or <= 0)
            return StatusCode((int)HttpStatusCode.Forbidden, APIResponse.Fail("No guardian profile for this user.", HttpStatusCode.Forbidden));
        var form = await _unitOfWork.TeacherFeedback.GetParentCycleFormAsync(guardianId.Value, id, studentId, cancellationToken);
        if (form == null)
            return NotFound(APIResponse.Fail("Cycle not found, not open, or student is not linked to you.", HttpStatusCode.NotFound));
        return Ok(APIResponse.Success(form));
    }

    [Authorize(Roles = "STUDENT")]
    [HttpPost("student/submit")]
    public async Task<ActionResult<APIResponse>> StudentSubmit([FromBody] StudentFeedbackSubmitDto? dto, CancellationToken cancellationToken)
    {
        if (dto == null)
            return BadRequest(APIResponse.Fail("Body is required.", HttpStatusCode.BadRequest));
        var uid = CurrentUserId;
        if (string.IsNullOrEmpty(uid))
            return Unauthorized(APIResponse.Fail("User id not found on token.", HttpStatusCode.Unauthorized));
        var studentId = await _unitOfWork.Homework.GetStudentIdByUserIdAsync(uid, cancellationToken);
        if (studentId is null or <= 0)
            return StatusCode((int)HttpStatusCode.Forbidden, APIResponse.Fail("No student profile for this user.", HttpStatusCode.Forbidden));
        try
        {
            await _unitOfWork.TeacherFeedback.UpsertStudentFeedbackAsync(studentId.Value, dto, cancellationToken);
            await _unitOfWork.CompleteAsync();
            return Ok(APIResponse.Success("saved"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(APIResponse.Fail(ex.Message, HttpStatusCode.BadRequest));
        }
    }

    [Authorize(Roles = "GUARDIAN")]
    [HttpPost("parent/submit")]
    public async Task<ActionResult<APIResponse>> ParentSubmit([FromBody] ParentFeedbackSubmitDto? dto, CancellationToken cancellationToken)
    {
        if (dto == null)
            return BadRequest(APIResponse.Fail("Body is required.", HttpStatusCode.BadRequest));
        var uid = CurrentUserId;
        if (string.IsNullOrEmpty(uid))
            return Unauthorized(APIResponse.Fail("User id not found on token.", HttpStatusCode.Unauthorized));
        var guardianId = await _unitOfWork.Homework.GetGuardianIdByUserIdAsync(uid, cancellationToken);
        if (guardianId is null or <= 0)
            return StatusCode((int)HttpStatusCode.Forbidden, APIResponse.Fail("No guardian profile for this user.", HttpStatusCode.Forbidden));
        try
        {
            await _unitOfWork.TeacherFeedback.UpsertParentFeedbackAsync(guardianId.Value, dto, cancellationToken);
            await _unitOfWork.CompleteAsync();
            return Ok(APIResponse.Success("saved"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(APIResponse.Fail(ex.Message, HttpStatusCode.BadRequest));
        }
    }
}
