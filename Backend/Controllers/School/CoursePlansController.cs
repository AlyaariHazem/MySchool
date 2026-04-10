using System;
using System.Net;
using Backend.DTOS.School.CoursePlan;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Backend.Controllers.School;

[Route("api/[controller]")]
[ApiController]
public class CoursePlansController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public CoursePlansController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost]
    public async Task<ActionResult<APIResponse>> Create([FromBody] CoursePlanDTO dto)
    {
        var response = new APIResponse();
        try
        {
            var created = await _unitOfWork.CoursePlans.AddAsync(dto);
            await _unitOfWork.CompleteAsync();
            response.Result = "CoursePlan created successfully";
            response.statusCode = HttpStatusCode.Created;
            return Ok(response);
        }
        catch (DbUpdateException dbEx) when (GetSqlException(dbEx) is { } sqlEx)
        {
            if (sqlEx.Number == 2627 || sqlEx.Message.Contains("PRIMARY KEY constraint", StringComparison.OrdinalIgnoreCase) || sqlEx.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase))
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Conflict;
                response.ErrorMasseges.Add(
                    "A course plan with the same year, teacher, class, division, subject, and term already exists.");
                return Conflict(response);
            }

            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.BadRequest;
            response.ErrorMasseges.Add(sqlEx.Message);
            return BadRequest(response);
        }
        catch (InvalidOperationException ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.BadRequest;
            response.ErrorMasseges.Add(ex.Message);
            return BadRequest(response);
        }
        catch (ArgumentException ex)
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
            response.ErrorMasseges.Add($"An unexpected error occurred: {ex.Message}");
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }

    private static SqlException? GetSqlException(Exception ex)
    {
        var cur = ex;
        while (cur != null)
        {
            if (cur is SqlException sql)
                return sql;
            cur = cur.InnerException;
        }
        return null;
    }

    [HttpGet]
    public async Task<ActionResult<APIResponse>> GetAll()
    {
        var response = new APIResponse();
        try
        {
            var result = await _unitOfWork.CoursePlans.GetAllAsync();
            response.Result = result;
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

    [HttpGet("subjects")]
    public async Task<ActionResult<APIResponse>> GetAllSubjects()
    {
        var response = new APIResponse();
        var result = await _unitOfWork.CoursePlans.GetAllSubjectsAsync();
        response.Result = result;
        response.statusCode = HttpStatusCode.OK;
        return Ok(response);
    }

    [HttpGet("{yearID}/{teacherID}/{classID}/{divisionID}/{subjectID}/{termID}")]
    public async Task<ActionResult<APIResponse>> Get(int yearID, int teacherID, int classID, int divisionID, int subjectID, int termID)
    {
        var response = new APIResponse();
        var item = await _unitOfWork.CoursePlans.GetByIdAsync(yearID, teacherID, classID, divisionID, subjectID, termID);
        if (item == null)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.NotFound;
            response.ErrorMasseges.Add("Course plan not found.");
            return NotFound(response);
        }

        response.Result = item;
        response.statusCode = HttpStatusCode.OK;
        return Ok(response);
    }

    [HttpPut("{oldYearID}/{oldTeacherID}/{oldClassID}/{oldDivisionID}/{oldSubjectID}/{oldTermID}")]
    public async Task<ActionResult<APIResponse>> Update(int oldYearID, int oldTeacherID, int oldClassID, int oldDivisionID, int oldSubjectID, int oldTermID, [FromBody] CoursePlanDTO dto)
    {
        var response = new APIResponse();
        try
        {
            await _unitOfWork.CoursePlans.UpdateAsync(dto, oldYearID, oldTeacherID, oldClassID, oldDivisionID, oldSubjectID, oldTermID);
            await _unitOfWork.CompleteAsync();
            response.Result = "Updated successfully";
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.NotFound;
            response.ErrorMasseges.Add(ex.Message);
            return NotFound(response);
        }
        catch (InvalidOperationException ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.BadRequest;
            response.ErrorMasseges.Add(ex.Message);
            return BadRequest(response);
        }
        catch (DbUpdateException dbEx)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.BadRequest;
            var errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;
            response.ErrorMasseges.Add($"Database error: {errorMessage}");
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.InternalServerError;
            var errorMessage = ex.InnerException?.Message ?? ex.Message;
            response.ErrorMasseges.Add(errorMessage);
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }

    [HttpDelete("{yearID}/{teacherID}/{classID}/{divisionID}/{subjectID}/{termID}")]
    public Task<ActionResult<APIResponse>> Delete(int yearID, int teacherID, int classID, int divisionID, int subjectID, int termID) =>
        DeleteCoursePlanCoreAsync(yearID, teacherID, classID, divisionID, subjectID, termID);

    /// <summary>Same delete as <see cref="Delete"/>, but uses POST + JSON body (avoids clients/proxies that mishandle DELETE with long paths).</summary>
    [HttpPost("delete")]
    public async Task<ActionResult<APIResponse>> DeleteByKey([FromBody] CoursePlanKeyDTO? key)
    {
        if (key == null)
        {
            var bad = new APIResponse
            {
                IsSuccess = false,
                statusCode = HttpStatusCode.BadRequest
            };
            bad.ErrorMasseges.Add("Request body is required.");
            return BadRequest(bad);
        }

        return await DeleteCoursePlanCoreAsync(key.YearID, key.TeacherID, key.ClassID, key.DivisionID, key.SubjectID, key.TermID);
    }

    private async Task<ActionResult<APIResponse>> DeleteCoursePlanCoreAsync(
        int yearID, int teacherID, int classID, int divisionID, int subjectID, int termID)
    {
        var response = new APIResponse();
        try
        {
            await _unitOfWork.CoursePlans.DeleteAsync(yearID, teacherID, classID, divisionID, subjectID, termID);
            await _unitOfWork.CompleteAsync();
            response.Result = "Deleted successfully";
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
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
