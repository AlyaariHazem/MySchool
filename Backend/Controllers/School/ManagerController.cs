using System.Net;
using Backend.Common;
using Backend.Controllers;
using Backend.DTOS.School.Manager;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers.School;

/// <summary>
/// Manager APIs: <c>POST api/Manager/page</c> (paged list) and <c>POST api/Manager/all</c> read managers.
/// To <b>create</b> a manager (Identity user + tenant <c>Managers</c> row + master <c>UserTenants</c> link), use
/// <c>POST api/Manager/add</c> with <see cref="AddManagerDTO"/> (SchoolID + TenantID + credentials).
/// </summary>
[Route("api/[controller]")]
public class ManagerController : GenericCrudController<GetManagerDTO, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public ManagerController(
        IGenericCrudRepository<GetManagerDTO, int> repository,
        IUnitOfWork unitOfWork)
        : base(repository)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>GET / — Legacy list shape for the admin UI.</summary>
    [HttpGet]
    public async Task<ActionResult<APIResponse>> GetManagers()
    {
        var response = new APIResponse();
        try
        {
            var managers = await _repository.GetAllAsync(new GenericQueryRequest());
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

    /// <summary>GET /{id} — Legacy wrapper.</summary>
    [HttpGet("{id:int}")]
    public override async Task<IActionResult> GetById(int id)
    {
        var manager = await _repository.GetByIdAsync(id);
        if (manager == null)
        {
            return NotFound(new APIResponse
            {
                IsSuccess = false,
                statusCode = HttpStatusCode.NotFound,
                ErrorMasseges = { "Manager not found." }
            });
        }

        return Ok(new APIResponse
        {
            Result = manager,
            statusCode = HttpStatusCode.OK
        });
    }

    /// <summary>POST / — Blocked; use POST add.</summary>
    [HttpPost]
    public override Task<IActionResult> Create([FromBody] GetManagerDTO entity) =>
        Task.FromResult<IActionResult>(BadRequest(new APIResponse
        {
            IsSuccess = false,
            statusCode = HttpStatusCode.BadRequest,
            ErrorMasseges =
            {
                "Use POST api/Manager/add with AddManagerDTO to create a manager."
            }
        }));

    /// <summary>POST /add — Create manager (users + tenant provisioning).</summary>
    [HttpPost("add")]
    public async Task<ActionResult<APIResponse>> AddManager([FromBody] AddManagerDTO managerDTO)
    {
        var response = new APIResponse();
        try
        {
            var responseData = await _unitOfWork.Managers.AddManager(managerDTO);
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

    /// <summary>PUT /{id} — Legacy route (same body as PUT /).</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<APIResponse>> UpdateManagerById(int id, [FromBody] GetManagerDTO managerDTO)
    {
        var response = new APIResponse();
        try
        {
            if (managerDTO.ManagerID != id)
            {
                response.statusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                response.ErrorMasseges.Add("ID mismatch.");
                return BadRequest(response);
            }

            await _repository.UpdateAsync(managerDTO);
            response.Result = "Manager updated successfully.";
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

    /// <summary>DELETE /{id} — Prefer 409 over 500 when FK/delete fails.</summary>
    [HttpDelete("{id:int}")]
    public override async Task<IActionResult> Delete(int id)
    {
        try
        {
            return await base.Delete(id);
        }
        catch (DbUpdateException)
        {
            var response = new APIResponse
            {
                IsSuccess = false,
                statusCode = HttpStatusCode.Conflict,
                ErrorMasseges =
                {
                    "Cannot delete this manager because related data still exists or the operation failed in the database."
                }
            };
            return Conflict(response);
        }
    }
}
