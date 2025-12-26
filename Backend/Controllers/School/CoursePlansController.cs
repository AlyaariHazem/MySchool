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
        catch (DbUpdateException dbEx) when (dbEx.InnerException is SqlException sqlEx)
        {
            // Check for primary key constraint violation
            if (sqlEx.Number == 2627 || sqlEx.Message.Contains("PRIMARY KEY constraint") || sqlEx.Message.Contains("duplicate key"))
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.Conflict;
                response.ErrorMasseges.Add("A course plan with the same combination of Year, Class, Division, Subject, and Term already exists. Please use a different combination.");
                return Conflict(response);
            }
            
            // Other database errors
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.BadRequest;
            response.ErrorMasseges.Add("An error occurred while saving the course plan. Please check your data and try again.");
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

    [HttpGet("{yearID}/{teacherID}/{classID}/{divisionID}/{subjectID}")]
    public async Task<ActionResult<APIResponse>> Get(int yearID, int teacherID, int classID, int divisionID, int subjectID)
    {
        var response = new APIResponse();
        var item = await _unitOfWork.CoursePlans.GetByIdAsync(yearID, teacherID, classID, divisionID, subjectID);
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

    [HttpPut("{oldYearID}/{oldTeacherID}/{oldClassID}/{oldDivisionID}/{oldSubjectID}")]
    public async Task<ActionResult<APIResponse>> Update(int oldYearID, int oldTeacherID, int oldClassID, int oldDivisionID, int oldSubjectID, [FromBody] CoursePlanDTO dto)
    {
        var response = new APIResponse();
        try
        {
            await _unitOfWork.CoursePlans.UpdateAsync(dto, oldYearID, oldTeacherID, oldClassID, oldDivisionID, oldSubjectID);
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

    [HttpDelete("{yearID}/{teacherID}/{classID}/{divisionID}/{subjectID}")]
    public async Task<ActionResult<APIResponse>> Delete(int yearID, int teacherID, int classID, int divisionID, int subjectID)
    {
        var response = new APIResponse();
        try
        {
            await _unitOfWork.CoursePlans.DeleteAsync(yearID, teacherID, classID, divisionID, subjectID);
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
