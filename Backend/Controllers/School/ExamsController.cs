using System.Net;
using System.Security.Claims;
using Backend.DTOS.School.Exams;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ExamsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ExamsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private bool IsPrivileged() =>
        User.IsInRole("ADMIN") || User.IsInRole("MANAGER")
        || string.Equals(User.FindFirstValue("UserType"), "ADMIN", StringComparison.OrdinalIgnoreCase)
        || string.Equals(User.FindFirstValue("UserType"), "MANAGER", StringComparison.OrdinalIgnoreCase);

    private async Task<bool> CanAccessScheduledExamAsync(int scheduledExamId, CancellationToken ct)
    {
        if (IsPrivileged()) return true;
        var uid = CurrentUserId;
        if (string.IsNullOrEmpty(uid)) return false;
        var teacherId = await _unitOfWork.Teachers.GetTeacherIdByUserIdAsync(uid, ct);
        if (!teacherId.HasValue) return false;
        var se = await _unitOfWork.Exams.GetScheduledExamByIdAsync(scheduledExamId, ct);
        return se != null && se.TeacherID == teacherId.Value;
    }

    // --- Exam types (admin/manager) ---

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpGet("types")]
    public async Task<ActionResult<APIResponse>> GetExamTypes([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            response.Result = await _unitOfWork.Exams.GetExamTypesAsync(includeInactive, cancellationToken);
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

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpPut("types/{id:int}")]
    public async Task<ActionResult<APIResponse>> UpdateExamType(int id, [FromBody] ExamTypeDto body, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            var updated = await _unitOfWork.Exams.UpdateExamTypeAsync(id, body.Name, body.SortOrder, body.IsActive, cancellationToken);
            if (updated == null) return NotFound(response);
            response.Result = updated;
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

    // --- Sessions ---

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpGet("sessions")]
    public async Task<ActionResult<APIResponse>> GetSessions([FromQuery] int? yearId, [FromQuery] int? termId, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            response.Result = await _unitOfWork.Exams.GetExamSessionsAsync(yearId, termId, cancellationToken);
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

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpPost("sessions")]
    public async Task<ActionResult<APIResponse>> CreateSession([FromBody] CreateExamSessionDto dto, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            response.Result = await _unitOfWork.Exams.CreateExamSessionAsync(dto, cancellationToken);
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

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpPut("sessions")]
    public async Task<ActionResult<APIResponse>> UpdateSession([FromBody] UpdateExamSessionDto dto, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            var updated = await _unitOfWork.Exams.UpdateExamSessionAsync(dto, cancellationToken);
            if (updated == null) return NotFound(response);
            response.Result = updated;
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

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpDelete("sessions/{id:int}")]
    public async Task<ActionResult<APIResponse>> DeleteSession(int id, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            await _unitOfWork.Exams.DeleteExamSessionAsync(id, cancellationToken);
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

    // --- Scheduled exams (planning) ---

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpGet("scheduled")]
    public async Task<ActionResult<APIResponse>> GetScheduled([FromQuery] ExamFilterQuery filter, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            response.Result = await _unitOfWork.Exams.GetScheduledExamsAsync(filter ?? new ExamFilterQuery(), cancellationToken);
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

    [Authorize(Roles = "ADMIN,MANAGER,TEACHER")]
    [HttpGet("scheduled/{id:int}")]
    public async Task<ActionResult<APIResponse>> GetScheduledById(int id, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            if (!await CanAccessScheduledExamAsync(id, cancellationToken))
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                response.ErrorMasseges.Add("Forbidden.");
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }

            var se = await _unitOfWork.Exams.GetScheduledExamByIdAsync(id, cancellationToken);
            if (se == null) return NotFound(response);
            response.Result = se;
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

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpPost("scheduled")]
    public async Task<ActionResult<APIResponse>> CreateScheduled([FromBody] CreateScheduledExamDto dto, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            response.Result = await _unitOfWork.Exams.CreateScheduledExamAsync(dto, cancellationToken);
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

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpPut("scheduled")]
    public async Task<ActionResult<APIResponse>> UpdateScheduled([FromBody] UpdateScheduledExamDto dto, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            var updated = await _unitOfWork.Exams.UpdateScheduledExamAsync(dto, cancellationToken);
            if (updated == null) return NotFound(response);
            response.Result = updated;
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

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpDelete("scheduled/{id:int}")]
    public async Task<ActionResult<APIResponse>> DeleteScheduled(int id, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            await _unitOfWork.Exams.DeleteScheduledExamAsync(id, cancellationToken);
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

    // --- Teacher ---

    [Authorize(Roles = "ADMIN,MANAGER,TEACHER")]
    [HttpGet("teacher/my")]
    public async Task<ActionResult<APIResponse>> GetMyTeacherExams([FromQuery] ExamFilterQuery filter, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            var uid = CurrentUserId;
            if (string.IsNullOrEmpty(uid))
                return Unauthorized(response);

            if (IsPrivileged())
            {
                response.Result = await _unitOfWork.Exams.GetScheduledExamsAsync(filter ?? new ExamFilterQuery(), cancellationToken);
            }
            else
            {
                var teacherId = await _unitOfWork.Teachers.GetTeacherIdByUserIdAsync(uid, cancellationToken);
                if (!teacherId.HasValue)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.Forbidden;
                    response.ErrorMasseges.Add("No teacher profile for this user.");
                    return StatusCode((int)HttpStatusCode.Forbidden, response);
                }

                response.Result = await _unitOfWork.Exams.GetTeacherScheduledExamsAsync(teacherId.Value, filter ?? new ExamFilterQuery(), cancellationToken);
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

    [Authorize(Roles = "ADMIN,MANAGER,TEACHER")]
    [HttpGet("scheduled/{id:int}/results")]
    public async Task<ActionResult<APIResponse>> GetResults(int id, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            if (!await CanAccessScheduledExamAsync(id, cancellationToken))
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                response.ErrorMasseges.Add("Forbidden.");
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }

            response.Result = await _unitOfWork.Exams.GetExamResultsAsync(id, cancellationToken);
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

    [Authorize(Roles = "ADMIN,MANAGER,TEACHER")]
    [HttpPut("scheduled/{id:int}/results")]
    public async Task<ActionResult<APIResponse>> SaveResults(int id, [FromBody] BulkExamResultsDto dto, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            if (!await CanAccessScheduledExamAsync(id, cancellationToken))
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                response.ErrorMasseges.Add("Forbidden.");
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }

            await _unitOfWork.Exams.SaveExamResultsAsync(id, dto, cancellationToken);
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

    [Authorize(Roles = "ADMIN,MANAGER,TEACHER")]
    [HttpPost("scheduled/{id:int}/publish-results")]
    public async Task<ActionResult<APIResponse>> PublishResults(int id, [FromQuery] bool publish = true, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            if (!await CanAccessScheduledExamAsync(id, cancellationToken))
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                response.ErrorMasseges.Add("Forbidden.");
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }

            await _unitOfWork.Exams.PublishResultsAsync(id, publish, cancellationToken);
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

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpPost("scheduled/{id:int}/publish-schedule")]
    public async Task<ActionResult<APIResponse>> PublishSchedule(int id, [FromQuery] bool publish = true, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            await _unitOfWork.Exams.PublishScheduleAsync(id, publish, cancellationToken);
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

    // --- Student ---

    [Authorize(Roles = "STUDENT,ADMIN,MANAGER")]
    [HttpGet("student/my")]
    public async Task<ActionResult<APIResponse>> GetMyStudentExams([FromQuery] bool upcomingOnly = false, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            var uid = CurrentUserId;
            if (string.IsNullOrEmpty(uid)) return Unauthorized(response);
            var studentId = await _unitOfWork.Exams.GetStudentIdByUserIdAsync(uid, cancellationToken);
            if (!studentId.HasValue)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                response.ErrorMasseges.Add("No student profile for this user.");
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }

            response.Result = await _unitOfWork.Exams.GetStudentExamsAsync(studentId.Value, upcomingOnly, cancellationToken);
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

    // --- Guardian ---

    [Authorize(Roles = "GUARDIAN,ADMIN,MANAGER")]
    [HttpGet("guardian/student/{studentId:int}")]
    public async Task<ActionResult<APIResponse>> GetGuardianStudentExams(int studentId, [FromQuery] bool upcomingOnly = false, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            var uid = CurrentUserId;
            if (string.IsNullOrEmpty(uid)) return Unauthorized(response);
            var guardianId = await _unitOfWork.Exams.GetGuardianIdByUserIdAsync(uid, cancellationToken);
            if (!guardianId.HasValue)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                response.ErrorMasseges.Add("No guardian profile for this user.");
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }

            response.Result = await _unitOfWork.Exams.GetGuardianStudentExamsAsync(guardianId.Value, studentId, upcomingOnly, cancellationToken);
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

    // --- Reports ---

    [Authorize(Roles = "ADMIN,MANAGER,TEACHER")]
    [HttpGet("reports/class-sheet/{scheduledExamId:int}")]
    public async Task<ActionResult<APIResponse>> ClassSheet(int scheduledExamId, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            if (!await CanAccessScheduledExamAsync(scheduledExamId, cancellationToken))
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                response.ErrorMasseges.Add("Forbidden.");
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }

            response.Result = await _unitOfWork.Exams.GetClassExamSheetAsync(scheduledExamId, cancellationToken);
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

    [Authorize(Roles = "ADMIN,MANAGER,TEACHER")]
    [HttpGet("reports/subject-performance")]
    public async Task<ActionResult<APIResponse>> SubjectPerformance([FromQuery] int yearId, [FromQuery] int termId, [FromQuery] int? classId, [FromQuery] int? divisionId, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            response.Result = await _unitOfWork.Exams.GetSubjectPerformanceAsync(yearId, termId, classId, divisionId, cancellationToken);
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

    [Authorize(Roles = "ADMIN,MANAGER,TEACHER")]
    [HttpGet("reports/top/{scheduledExamId:int}")]
    public async Task<ActionResult<APIResponse>> Top(int scheduledExamId, [FromQuery] int take = 10, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            if (!await CanAccessScheduledExamAsync(scheduledExamId, cancellationToken))
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                response.ErrorMasseges.Add("Forbidden.");
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }

            response.Result = await _unitOfWork.Exams.GetTopStudentsAsync(scheduledExamId, take, cancellationToken);
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

    [Authorize(Roles = "ADMIN,MANAGER,TEACHER")]
    [HttpGet("reports/weak/{scheduledExamId:int}")]
    public async Task<ActionResult<APIResponse>> Weak(int scheduledExamId, [FromQuery] int take = 10, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            if (!await CanAccessScheduledExamAsync(scheduledExamId, cancellationToken))
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                response.ErrorMasseges.Add("Forbidden.");
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }

            response.Result = await _unitOfWork.Exams.GetWeakStudentsAsync(scheduledExamId, take, cancellationToken);
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

    [Authorize(Roles = "ADMIN,MANAGER,TEACHER")]
    [HttpGet("reports/absent/{scheduledExamId:int}")]
    public async Task<ActionResult<APIResponse>> Absent(int scheduledExamId, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            if (!await CanAccessScheduledExamAsync(scheduledExamId, cancellationToken))
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                response.ErrorMasseges.Add("Forbidden.");
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }

            response.Result = await _unitOfWork.Exams.GetAbsentStudentsAsync(scheduledExamId, cancellationToken);
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
