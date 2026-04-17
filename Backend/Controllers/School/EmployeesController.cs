using System.Net;
using Backend.DTOS.School.Employees;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

/// <summary>
/// HR employee profiles (aggregate) for the School Performance Analysis System — separate from legacy <see cref="EmployeeController"/> (<c>/api/Employee</c>).
/// </summary>
[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeProfileService _employees;

    public EmployeesController(IEmployeeProfileService employees)
    {
        _employees = employees;
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> GetAll([FromQuery] EmployeeProfileListFilterDto? filter, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            response.Result = await _employees.GetAllAsync(filter, cancellationToken);
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

    [HttpGet("{id:int}")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            var row = await _employees.GetByIdAsync(id, cancellationToken);
            if (row == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add($"Employee profile {id} was not found.");
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

    [HttpGet("{id:int}/full-profile")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> GetFullProfile(int id, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            response.Result = await _employees.GetFullProfileAsync(id, cancellationToken);
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

    /// <summary>Active and inactive job types for recruitment filters and display; any authenticated school user.</summary>
    [HttpGet("job-types")]
    [AllowAnonymous]
    public async Task<ActionResult<APIResponse>> GetJobTypes(CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            response.Result = await _employees.GetJobTypesAsync(cancellationToken);
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

    [HttpPost]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> Create([FromBody] EmployeeProfileCreateDto body, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        if (!ModelState.IsValid)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.BadRequest;
            response.ErrorMasseges.AddRange(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return BadRequest(response);
        }

        try
        {
            response.Result = await _employees.CreateAsync(body, cancellationToken);
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
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
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> Update(int id, [FromBody] EmployeeProfileUpdateDto body, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        if (!ModelState.IsValid)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.BadRequest;
            response.ErrorMasseges.AddRange(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return BadRequest(response);
        }

        try
        {
            response.Result = await _employees.UpdateAsync(id, body, cancellationToken);
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
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }

    /// <summary>Soft-deactivate profile (<see cref="EmployeeProfile.IsActive"/> = false).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> Deactivate(int id, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            var ok = await _employees.DeactivateAsync(id, cancellationToken);
            if (!ok)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add($"Employee profile {id} was not found.");
                return NotFound(response);
            }

            response.statusCode = HttpStatusCode.NoContent;
            return StatusCode((int)HttpStatusCode.NoContent);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.InternalServerError;
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }

    [HttpPost("{id:int}/qualifications")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> AddQualification(int id, [FromBody] EmployeeQualificationDto body, CancellationToken cancellationToken) =>
        await RunChildAddAsync(() => _employees.AddQualificationAsync(id, body, cancellationToken));

    [HttpPost("{id:int}/specializations")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> AddSpecialization(int id, [FromBody] EmployeeSpecializationDto body, CancellationToken cancellationToken) =>
        await RunChildAddAsync(() => _employees.AddSpecializationAsync(id, body, cancellationToken));

    [HttpPost("{id:int}/history")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> AddHistory(int id, [FromBody] EmployeeHistoryDto body, CancellationToken cancellationToken) =>
        await RunChildAddAsync(() => _employees.AddHistoryAsync(id, body, cancellationToken));

    [HttpPost("{id:int}/documents")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> AddDocument(int id, [FromBody] EmployeeDocumentDto body, CancellationToken cancellationToken) =>
        await RunChildAddAsync(() => _employees.AddDocumentAsync(id, body, cancellationToken));

    [HttpPost("{id:int}/leaves")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> AddLeave(int id, [FromBody] EmployeeLeaveDto body, CancellationToken cancellationToken) =>
        await RunChildAddAsync(() => _employees.AddLeaveAsync(id, body, cancellationToken));

    [HttpPost("{id:int}/performance-summaries")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> AddPerformanceSummary(int id, [FromBody] EmployeePerformanceSummaryDto body, CancellationToken cancellationToken) =>
        await RunChildAddAsync(() => _employees.AddPerformanceSummaryAsync(id, body, cancellationToken));

    private async Task<ActionResult<APIResponse>> RunChildAddAsync<T>(Func<Task<T>> action)
    {
        var response = new APIResponse();
        if (!ModelState.IsValid)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.BadRequest;
            response.ErrorMasseges.AddRange(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return BadRequest(response);
        }

        try
        {
            response.Result = await action();
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
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }
}
