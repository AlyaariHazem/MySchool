using System.Net;
using Backend.DTOS.School.CoursePlan;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

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
        var created = await _unitOfWork.CoursePlans.AddAsync(dto);
        await _unitOfWork.CompleteAsync();
        response.Result = "CoursePlan created successfully";
        response.statusCode = HttpStatusCode.Created;
        return Ok(response);
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

    [HttpGet("{id}")]
    public async Task<ActionResult<APIResponse>> Get(int id)
    {
        var response = new APIResponse();
        var item = await _unitOfWork.CoursePlans.GetByIdAsync(id);
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

    [HttpPut("{id}")]
    public async Task<ActionResult<APIResponse>> Update(int id, [FromBody] CoursePlanDTO dto)
    {
        var response = new APIResponse();
        dto.CoursePlanID = id;
        await _unitOfWork.CoursePlans.UpdateAsync(dto);
        await _unitOfWork.CompleteAsync();
        response.Result = "Updated successfully";
        response.statusCode = HttpStatusCode.OK;
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<APIResponse>> Delete(int id)
    {
        var response = new APIResponse();
        await _unitOfWork.CoursePlans.DeleteAsync(id);
        await _unitOfWork.CompleteAsync();
        response.Result = "Deleted successfully";
        response.statusCode = HttpStatusCode.OK;
        return Ok(response);
    }
}
