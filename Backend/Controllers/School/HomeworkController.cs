using System.Net;
using System.Security.Claims;
using Backend.DTOS.School.Homework;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class HomeworkController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public HomeworkController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private bool IsPrivileged() =>
        User.IsInRole("ADMIN") || User.IsInRole("MANAGER")
        || string.Equals(User.FindFirstValue("UserType"), "ADMIN", StringComparison.OrdinalIgnoreCase)
        || string.Equals(User.FindFirstValue("UserType"), "MANAGER", StringComparison.OrdinalIgnoreCase);

    private async Task<bool> CanAccessTaskAsync(int homeworkTaskId, CancellationToken ct)
    {
        if (IsPrivileged()) return true;
        var uid = CurrentUserId;
        if (string.IsNullOrEmpty(uid)) return false;
        var teacherId = await _unitOfWork.Teachers.GetTeacherIdByUserIdAsync(uid, ct);
        if (!teacherId.HasValue) return false;
        var task = await _unitOfWork.Homework.GetTaskByIdAsync(homeworkTaskId, ct);
        return task != null && task.TeacherID == teacherId.Value;
    }

    // --- Teacher (and privileged) — tasks ---

    [Authorize(Roles = "TEACHER,ADMIN,MANAGER")]
    [HttpGet("teacher/tasks")]
    public async Task<ActionResult<APIResponse>> GetTeacherTasks([FromQuery] HomeworkFilterQuery filter, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            if (IsPrivileged())
            {
                response.Result = await _unitOfWork.Homework.ListTasksPrivilegedAsync(filter ?? new HomeworkFilterQuery(), cancellationToken);
            }
            else
            {
                var uid = CurrentUserId;
                if (string.IsNullOrEmpty(uid)) return Forbid();
                var teacherId = await _unitOfWork.Teachers.GetTeacherIdByUserIdAsync(uid, cancellationToken);
                if (!teacherId.HasValue)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.Forbidden;
                    return StatusCode((int)HttpStatusCode.Forbidden, response);
                }

                response.Result = await _unitOfWork.Homework.ListTasksForTeacherAsync(teacherId.Value, filter ?? new HomeworkFilterQuery(), cancellationToken);
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

    [Authorize(Roles = "TEACHER,ADMIN,MANAGER")]
    [HttpGet("teacher/tasks/{id:int}")]
    public async Task<ActionResult<APIResponse>> GetTeacherTask(int id, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            if (!await CanAccessTaskAsync(id, cancellationToken))
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }

            var task = await _unitOfWork.Homework.GetTaskByIdAsync(id, cancellationToken);
            if (task == null) return NotFound(response);
            response.Result = task;
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

    [Authorize(Roles = "TEACHER,ADMIN,MANAGER")]
    [HttpPost("teacher/tasks")]
    public async Task<ActionResult<APIResponse>> CreateTask([FromBody] CreateHomeworkTaskDto dto, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            var privileged = IsPrivileged();
            int teacherId;
            if (privileged)
            {
                if (!dto.TeacherID.HasValue)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("TeacherID is required when creating homework as admin or manager.");
                    return BadRequest(response);
                }

                teacherId = dto.TeacherID.Value;
            }
            else
            {
                var uid = CurrentUserId;
                if (string.IsNullOrEmpty(uid)) return Forbid();
                var tid = await _unitOfWork.Teachers.GetTeacherIdByUserIdAsync(uid, cancellationToken);
                if (!tid.HasValue)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.Forbidden;
                    return StatusCode((int)HttpStatusCode.Forbidden, response);
                }

                teacherId = tid.Value;
            }

            var activeYearId = await _unitOfWork.Years.GetActiveYearIdAsync(cancellationToken);
            if (!activeYearId.HasValue)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.BadRequest;
                response.ErrorMasseges.Add("لم يتم ضبط سنة دراسية نشطة في النظام.");
                return BadRequest(response);
            }

            dto.YearID = activeYearId.Value;

            var created = await _unitOfWork.Homework.CreateTaskAsync(teacherId, dto, skipCoursePlanCheck: privileged, cancellationToken);
            response.Result = created;
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

    [Authorize(Roles = "TEACHER,ADMIN,MANAGER")]
    [HttpPut("teacher/tasks/{id:int}")]
    public async Task<ActionResult<APIResponse>> UpdateTask(int id, [FromBody] UpdateHomeworkTaskDto dto, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            dto.HomeworkTaskID = id;
            var uid = CurrentUserId;
            var privileged = IsPrivileged();
            var teacherId = privileged ? 0 : (await _unitOfWork.Teachers.GetTeacherIdByUserIdAsync(uid!, cancellationToken)) ?? 0;
            if (!privileged && teacherId == 0)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }

            var activeYearId = await _unitOfWork.Years.GetActiveYearIdAsync(cancellationToken);
            if (!activeYearId.HasValue)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.BadRequest;
                response.ErrorMasseges.Add("لم يتم ضبط سنة دراسية نشطة في النظام.");
                return BadRequest(response);
            }

            dto.YearID = activeYearId.Value;

            var updated = await _unitOfWork.Homework.UpdateTaskAsync(id, teacherId, dto, skipCoursePlanCheck: privileged, privileged, cancellationToken);
            if (updated == null) return NotFound(response);
            response.Result = updated;
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

    [Authorize(Roles = "TEACHER,ADMIN,MANAGER")]
    [HttpDelete("teacher/tasks/{id:int}")]
    public async Task<ActionResult<APIResponse>> DeleteTask(int id, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            var privileged = IsPrivileged();
            var teacherId = privileged ? 0 : (await _unitOfWork.Teachers.GetTeacherIdByUserIdAsync(CurrentUserId!, cancellationToken)) ?? 0;
            if (!privileged && teacherId == 0)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }

            var ok = await _unitOfWork.Homework.DeleteTaskAsync(id, teacherId, privileged, cancellationToken);
            if (!ok) return NotFound(response);
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

    [Authorize(Roles = "TEACHER,ADMIN,MANAGER")]
    [HttpGet("teacher/tasks/{id:int}/submissions")]
    public async Task<ActionResult<APIResponse>> GetTaskSubmissions(int id, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            if (!await CanAccessTaskAsync(id, cancellationToken))
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }

            response.Result = await _unitOfWork.Homework.ListSubmissionsForTaskAsync(id, cancellationToken);
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

    [Authorize(Roles = "TEACHER,ADMIN,MANAGER")]
    [HttpPut("teacher/submissions/{submissionId:int}")]
    public async Task<ActionResult<APIResponse>> ReviewSubmission(int submissionId, [FromBody] ReviewHomeworkSubmissionDto dto, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            var privileged = IsPrivileged();
            var teacherId = privileged ? 0 : (await _unitOfWork.Teachers.GetTeacherIdByUserIdAsync(CurrentUserId!, cancellationToken)) ?? 0;
            if (!privileged && teacherId == 0)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }

            var row = await _unitOfWork.Homework.ReviewSubmissionAsync(submissionId, teacherId, dto, privileged, cancellationToken);
            if (row == null) return NotFound(response);
            response.Result = row;
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

    // --- Student ---

    [Authorize(Roles = "STUDENT")]
    [HttpGet("student/tasks")]
    public async Task<ActionResult<APIResponse>> GetStudentTasks([FromQuery] string? filter, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            var uid = CurrentUserId;
            var studentId = await _unitOfWork.Homework.GetStudentIdByUserIdAsync(uid!, cancellationToken);
            if (!studentId.HasValue)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }

            response.Result = await _unitOfWork.Homework.ListStudentTasksAsync(studentId.Value, filter, cancellationToken);
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

    [Authorize(Roles = "STUDENT")]
    [HttpGet("student/tasks/{taskId:int}")]
    public async Task<ActionResult<APIResponse>> GetStudentTask(int taskId, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            var uid = CurrentUserId;
            var studentId = await _unitOfWork.Homework.GetStudentIdByUserIdAsync(uid!, cancellationToken);
            if (!studentId.HasValue)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }

            var detail = await _unitOfWork.Homework.GetStudentTaskDetailAsync(studentId.Value, taskId, cancellationToken);
            if (detail == null) return NotFound(response);
            response.Result = detail;
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

    [Authorize(Roles = "STUDENT")]
    [HttpPost("student/tasks/{taskId:int}/submit")]
    public async Task<ActionResult<APIResponse>> SubmitStudentTask(int taskId, [FromBody] StudentSubmitHomeworkDto dto, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            var uid = CurrentUserId;
            var studentId = await _unitOfWork.Homework.GetStudentIdByUserIdAsync(uid!, cancellationToken);
            if (!studentId.HasValue)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }

            var detail = await _unitOfWork.Homework.SubmitStudentTaskAsync(studentId.Value, taskId, dto, cancellationToken);
            if (detail == null) return NotFound(response);
            response.Result = detail;
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

    // --- Guardian ---

    [Authorize(Roles = "GUARDIAN")]
    [HttpGet("guardian/tasks")]
    public async Task<ActionResult<APIResponse>> GetGuardianAllTasks([FromQuery] string? filter, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            var uid = CurrentUserId;
            var guardianId = await _unitOfWork.Homework.GetGuardianIdByUserIdAsync(uid!, cancellationToken);
            if (!guardianId.HasValue)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }

            response.Result = await _unitOfWork.Homework.ListAllGuardianStudentTasksAsync(guardianId.Value, filter, cancellationToken);
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

    [Authorize(Roles = "GUARDIAN")]
    [HttpGet("guardian/students/{studentId:int}/tasks")]
    public async Task<ActionResult<APIResponse>> GetGuardianStudentTasks(int studentId, [FromQuery] string? filter, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            var uid = CurrentUserId;
            var guardianId = await _unitOfWork.Homework.GetGuardianIdByUserIdAsync(uid!, cancellationToken);
            if (!guardianId.HasValue)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }

            response.Result = await _unitOfWork.Homework.ListGuardianStudentTasksAsync(guardianId.Value, studentId, filter, cancellationToken);
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

    [Authorize(Roles = "GUARDIAN")]
    [HttpGet("guardian/students/{studentId:int}/tasks/{taskId:int}")]
    public async Task<ActionResult<APIResponse>> GetGuardianStudentTask(int studentId, int taskId, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            var uid = CurrentUserId;
            var guardianId = await _unitOfWork.Homework.GetGuardianIdByUserIdAsync(uid!, cancellationToken);
            if (!guardianId.HasValue)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Forbidden;
                return StatusCode((int)HttpStatusCode.Forbidden, response);
            }

            var detail = await _unitOfWork.Homework.GetGuardianStudentTaskDetailAsync(guardianId.Value, studentId, taskId, cancellationToken);
            if (detail == null) return NotFound(response);
            response.Result = detail;
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

    // --- Manager / Admin reports ---

    [Authorize(Roles = "ADMIN,MANAGER")]
    [HttpGet("reports/activity")]
    public async Task<ActionResult<APIResponse>> GetActivityReport([FromQuery] int yearId, [FromQuery] int termId, [FromQuery] int? classId, [FromQuery] int? teacherId, CancellationToken cancellationToken = default)
    {
        var response = new APIResponse();
        try
        {
            response.Result = await _unitOfWork.Homework.GetActivitySummaryAsync(yearId, termId, classId, teacherId, cancellationToken);
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
