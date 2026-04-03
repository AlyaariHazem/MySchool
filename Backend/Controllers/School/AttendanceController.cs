using System.Net;
using System.Security.Claims;
using Backend.Data;
using Backend.DTOS.School.Attendance;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantInfo _tenantInfo;

    public AttendanceController(IUnitOfWork unitOfWork, TenantInfo tenantInfo)
    {
        _unitOfWork = unitOfWork;
        _tenantInfo = tenantInfo;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    // GET api/Attendance/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<APIResponse>> GetById(Guid id)
    {
        var response = new APIResponse();
        try
        {
            var row = await _unitOfWork.Attendance.GetByIdAsync(id);
            if (row == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("Attendance record not found.");
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

    // GET api/Attendance/class/{classId}?date=2026-04-04
    [HttpGet("class/{classId:int}")]
    public async Task<ActionResult<APIResponse>> GetByClassAndDate(int classId, [FromQuery] DateOnly? date)
    {
        var response = new APIResponse();
        try
        {
            if (!date.HasValue)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.BadRequest;
                response.ErrorMasseges.Add("Query parameter 'date' is required (e.g. ?date=2026-04-04).");
                return BadRequest(response);
            }

            var list = await _unitOfWork.Attendance.GetByClassAndDateAsync(classId, date.Value);
            response.Result = list;
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

    // GET api/Attendance/student/{studentId}?from=2026-04-01&to=2026-04-30
    [HttpGet("student/{studentId:int}")]
    public async Task<ActionResult<APIResponse>> GetByStudent(int studentId, [FromQuery] DateOnly? from = null, [FromQuery] DateOnly? to = null)
    {
        var response = new APIResponse();
        try
        {
            var list = await _unitOfWork.Attendance.GetByStudentAsync(studentId, from, to);
            response.Result = list;
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

    // POST api/Attendance
    [HttpPost]
    public async Task<ActionResult<APIResponse>> Create([FromBody] CreateAttendanceDTO dto)
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
                response.statusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                response.ErrorMasseges.Add("Invalid attendance data.");
                return BadRequest(response);
            }

            var created = await _unitOfWork.Attendance.CreateAsync(dto, userId, _tenantInfo.TenantId);
            response.Result = created;
            response.statusCode = HttpStatusCode.Created;
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

    // PUT api/Attendance/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<APIResponse>> Update(Guid id, [FromBody] UpdateAttendanceDTO dto)
    {
        var response = new APIResponse();
        try
        {
            if (!ModelState.IsValid)
            {
                response.statusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                response.ErrorMasseges.Add("Invalid attendance data.");
                return BadRequest(response);
            }

            var updated = await _unitOfWork.Attendance.UpdateAsync(id, dto);
            if (updated == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("Attendance record not found.");
                return NotFound(response);
            }

            response.Result = updated;
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

    // POST api/Attendance/bulk
    [HttpPost("bulk")]
    public async Task<ActionResult<APIResponse>> BulkUpsert([FromBody] BulkAttendanceDTO dto)
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
                response.statusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                response.ErrorMasseges.Add("Invalid bulk attendance data.");
                return BadRequest(response);
            }

            var count = await _unitOfWork.Attendance.BulkUpsertAsync(dto, userId, _tenantInfo.TenantId);
            response.Result = new { updatedCount = count };
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

    // DELETE api/Attendance/{id}
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<APIResponse>> Delete(Guid id)
    {
        var response = new APIResponse();
        try
        {
            var ok = await _unitOfWork.Attendance.DeleteAsync(id);
            if (!ok)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("Attendance record not found.");
                return NotFound(response);
            }

            response.Result = "Attendance deleted successfully.";
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
