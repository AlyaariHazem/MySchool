using System.Net;
using Backend.Common;
using Backend.Controllers;
using Backend.DTOS.School;
using Backend.Interfaces;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers.School;

/// <summary>
/// School CRUD via <see cref="GenericCrudController{SchoolDTO,int}"/>.
/// POST (create) uses <see cref="TenantProvisioningService"/> instead of the generic repository.
/// </summary>
[Route("api/[controller]")]
public class SchoolController : GenericCrudController<SchoolDTO, int>
{
    private readonly TenantProvisioningService _tenantProvisioningService;

    public SchoolController(
        IGenericCrudRepository<SchoolDTO, int> repository,
        TenantProvisioningService tenantProvisioningService)
        : base(repository)
    {
        _tenantProvisioningService = tenantProvisioningService;
    }

    /// <summary>GET / — Legacy list shape for the admin UI (<see cref="APIResponse"/>).</summary>
    [HttpGet]
    public async Task<ActionResult<APIResponse>> GetAllSchools()
    {
        var response = new APIResponse();
        try
        {
            var schools = await _repository.GetAllAsync(new GenericQueryRequest());
            response.Result = schools;
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

    /// <summary>GET /{id} — Legacy wrapper for the admin UI.</summary>
    [HttpGet("{id}")]
    public override async Task<IActionResult> GetById(int id)
    {
        var school = await _repository.GetByIdAsync(id);
        if (school == null)
        {
            var response = new APIResponse
            {
                IsSuccess = false,
                statusCode = HttpStatusCode.NotFound,
                ErrorMasseges = { $"School with ID {id} not found." }
            };
            return NotFound(response);
        }

        return Ok(new APIResponse
        {
            Result = school,
            statusCode = HttpStatusCode.OK
        });
    }

    /// <summary>POST / — Create school + tenant database (not generic repository create).</summary>
    [HttpPost]
    public override async Task<IActionResult> Create([FromBody] SchoolDTO schoolDTO)
    {
        var response = new APIResponse();
        try
        {
            if (schoolDTO == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.BadRequest;
                response.ErrorMasseges.Add("Invalid school data.");
                return BadRequest(response);
            }

            var tenant = await _tenantProvisioningService.CreateSchoolDatabaseAsync(
                schoolDTO.SchoolName ?? string.Empty,
                schoolDTO);

            response.Result = new
            {
                TenantId = tenant.TenantId,
                SchoolName = tenant.SchoolName,
                ConnectionString = tenant.ConnectionString
            };
            response.statusCode = HttpStatusCode.Created;
            return StatusCode((int)HttpStatusCode.Created, response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.InternalServerError;
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }

    /// <summary>PUT /{id} — Matches existing Angular route (PUT …/School/{id}).</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<APIResponse>> UpdateSchool(int id, [FromBody] SchoolDTO schoolDTO)
    {
        var response = new APIResponse();
        try
        {
            if (schoolDTO == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.BadRequest;
                response.ErrorMasseges.Add("Invalid school data.");
                return BadRequest(response);
            }

            if (schoolDTO.SchoolID != id)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.BadRequest;
                response.ErrorMasseges.Add("School data is invalid or ID mismatch.");
                return BadRequest(response);
            }

            await _repository.UpdateAsync(schoolDTO);
            response.Result = "School updated successfully.";
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

    /// <summary>DELETE /{id} — Return 409 when related rows block delete instead of an unhandled 500.</summary>
    [HttpDelete("{id}")]
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
                    "لا يمكن حذف المدرسة لوجود بيانات مرتبطة بها (مثل السنوات أو المستخدمين). احذف أو انقل البيانات المرتبطة أولاً."
                }
            };
            return Conflict(response);
        }
    }
}
