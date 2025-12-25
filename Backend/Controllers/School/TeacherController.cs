using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Backend.Common;
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

    [HttpPut("{id}")]
    public async Task<ActionResult<APIResponse>> UpdateTeacher(int id, [FromBody] TeacherDTO teacher)
    {
        var response = new APIResponse();
        try
        {
            if (teacher == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.BadRequest;
                response.ErrorMasseges.Add("Teacher data is null");
                return BadRequest(response);
            }

            if (id != teacher.TeacherID)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.BadRequest;
                response.ErrorMasseges.Add("Teacher ID mismatch");
                return BadRequest(response);
            }

            var updatedTeacher = await _unitOfWork.Teachers.UpdateTeacherAsync(id, teacher);
            response.Result = updatedTeacher;
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

    [HttpGet("page")]
    public async Task<ActionResult<PagedResult<TeacherDTO>>> GetTeachersPage(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 8,
        CancellationToken cancellationToken = default)
    {
        // Clamp values to avoid abuse (e.g., pageSize=100000)
        const int maxPageSize = 100;
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 8;
        if (pageSize > maxPageSize) pageSize = maxPageSize;

        var (items, totalCount) = await _unitOfWork.Teachers
            .GetTeachersPageAsync(pageNumber, pageSize, cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return Ok(new PagedResult<TeacherDTO>(
            items,
            pageNumber,
            pageSize,
            totalCount,
            totalPages
        ));
    }

    [HttpPost("page")]
    public async Task<ActionResult<PagedResult<TeacherDTO>>> GetTeachersWithFilters(
        [FromBody] FilterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Clamp values to avoid abuse (e.g., pageSize=100000)
        const int maxPageSize = 100;
        if (request.PageNumber < 1) request.PageNumber = 1;
        if (request.PageSize < 1) request.PageSize = 8;
        if (request.PageSize > maxPageSize) request.PageSize = maxPageSize;

        var (items, totalCount) = await _unitOfWork.Teachers
            .GetTeachersPageWithFiltersAsync(request.PageNumber, request.PageSize, request.Filters, cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return Ok(new PagedResult<TeacherDTO>(
            items,
            request.PageNumber,
            request.PageSize,
            totalCount,
            totalPages
        ));
    }

}
