using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Backend.DTOS.School.Manager;
using Backend.Interfaces;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.Controllers.School;

[Route("api/[controller]")]
[ApiController]
public class ManagerController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ManagerController(IUnitOfWork managerRepo)
    {
        _unitOfWork = managerRepo;
    }

    // POST: api/manager (Add a new manager)
    [HttpPost]
    public async Task<ActionResult<APIResponse>> AddManager([FromBody] AddManagerDTO managerDTO)
    {
        var response = new APIResponse();
        try
        {
          var responseData=   await _unitOfWork.Managers.AddManager(managerDTO);
            response.Result = responseData;
            response.statusCode = HttpStatusCode.Created;
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

    // GET: api/manager/{id} (Get a manager by ID)
    [HttpGet("{id:int}")]
    public async Task<ActionResult<APIResponse>> GetManager(int id)
    {
        var response = new APIResponse();
        try
        {
            var manager = await _unitOfWork.Managers.GetManager(id);
            if (manager == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("Manager not found.");
                return NotFound(response);
            }

            response.Result = manager;
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

    // GET: api/manager (Get all managers)
    [HttpGet]
    public async Task<ActionResult<APIResponse>> GetManagers()
    {
        var response = new APIResponse();
        try
        {
            var managers = await _unitOfWork.Managers.GetManagers();
            response.Result = managers;
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

    // PUT: api/manager/{id} (Update a manager)
    [HttpPut("{id:int}")]
    public async Task<ActionResult<APIResponse>> UpdateManager(int id, [FromBody] GetManagerDTO managerDTO)
    {
        var response = new APIResponse();
        try
        {
            if (id != managerDTO.ManagerID)
            {
                response.statusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                response.ErrorMasseges.Add("ID mismatch.");
                return BadRequest(response);
            }

            await _unitOfWork.Managers.UpdateManager(managerDTO);
            response.Result = "Manager updated successfully.";
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

    // DELETE: api/manager/{id} (Delete a manager)
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<APIResponse>> DeleteManager(int id)
    {
        var response = new APIResponse();
        try
        {
            await _unitOfWork.Managers.DeleteManager(id);
            response.Result = "Manager deleted successfully.";
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

