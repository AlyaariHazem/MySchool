using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Backend.DTOS.School.Employee;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    public EmployeeController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    [HttpGet]
    public async Task<ActionResult<APIResponse>> GetAllEmployees()
    {
        var response = new APIResponse();
        try
        {
            var employees = await _unitOfWork.Employees.GetAllEmployeesAsync();
            response.Result = employees;
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
    public async Task<ActionResult<APIResponse>> AddEmployee([FromBody] EmployeeDTO employee)
    {
        var response = new APIResponse();
        try
        {
            if (employee == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.BadRequest;
                response.ErrorMasseges.Add("Employee data is null");
                return BadRequest(response);
            }
            var result = await _unitOfWork.Employees.AddEmployeeAsync(employee);
            response.Result = result;
            response.statusCode = HttpStatusCode.Created;
            return CreatedAtAction(nameof(GetAllEmployees), new { id = result }, response);
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
    public async Task<ActionResult<APIResponse>> UpdateEmployee(int id, [FromBody] EmployeeDTO employee)
    {
        var response = new APIResponse();
        try
        {
            if (employee == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.BadRequest;
                response.ErrorMasseges.Add("Employee data is null");
                return BadRequest(response);
            }
            var result = await _unitOfWork.Employees.UpdateEmployeeAsync(id, employee);
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
    //  api/employees/42?jobType=Teacher
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<APIResponse>> DeleteEmployee(int id, [FromQuery] string jobType)
    {
        var response = new APIResponse();

        try
        {
            await _unitOfWork.Employees.DeleteEmployeeAsync(id, jobType);

            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.NotFound;
            response.ErrorMasseges.Add("Employee not found");
            return NotFound(response);
            // 204, no body
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
