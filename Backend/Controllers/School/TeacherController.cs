using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Backend.DTOS.School.Teachers;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

[ApiController]
[Route("api/[controller]")]
public class TeacherController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    public TeacherController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost]
    public async Task<ActionResult<APIResponse>> AddTeacher([FromBody] TeacherDTO teacher)
    {
        var response = new APIResponse();
        try
        {
            var teachers = await _unitOfWork.Teachers.AddTeacherAsync(teacher);
            response.Result = teachers;
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
    [HttpGet]
    public async Task<ActionResult<APIResponse>> GetAllTeachers()
    {
        var response = new APIResponse();
        try
        {
            var teachers = await _unitOfWork.Teachers.GetAllTeachersAsync();
            response.Result = teachers;
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
