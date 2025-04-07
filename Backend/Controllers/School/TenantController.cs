using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Backend.DTOS.School.Tenant;
using Backend.Interfaces;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public TenantController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/tenants (Get all tenants)
        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetAllTenants()
        {
            var response = new APIResponse();
            try
            {
                var tenants = await _unitOfWork.Tenants.GetAll();
                response.Result = tenants;
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

        // GET: api/tenants/{id} (Get tenant by ID)
        [HttpGet("{id:int}")]
        public async Task<ActionResult<APIResponse>> GetTenantById(int id)
        {
            var response = new APIResponse();
            try
            {
                var tenant = await _unitOfWork.Tenants.GetByIdAsync(id);
                if (tenant == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Tenant not found.");
                    return NotFound(response);
                }

                response.Result = tenant;
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

        // POST: api/tenants (Create a new tenant)
        [HttpPost]
        public async Task<ActionResult<APIResponse>> CreateTenant([FromBody] TenantDTO tenantDto)
        {
            var response = new APIResponse();
            try
            {
                if (tenantDto == null)
                {
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.IsSuccess = false;
                    response.ErrorMasseges.Add("Invalid tenant data.");
                    return BadRequest(response);
                }

                await _unitOfWork.Tenants.AddAsync(tenantDto);
                response.Result = tenantDto;
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

        // PUT: api/tenants/{id} (Update tenant)
        [HttpPut("{id:int}")]
        public async Task<ActionResult<APIResponse>> UpdateTenant(int id, [FromBody] TenantDTO tenantDto)
        {
            var response = new APIResponse();
            try
            {
                if (tenantDto == null || id != tenantDto.TenantID)
                {
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.IsSuccess = false;
                    response.ErrorMasseges.Add("Invalid tenant data or ID mismatch.");
                    return BadRequest(response);
                }

                var existingTenant = await _unitOfWork.Tenants.GetByIdAsync(id);
                if (existingTenant == null)
                {
                    response.statusCode = HttpStatusCode.NotFound;
                    response.IsSuccess = false;
                    response.ErrorMasseges.Add("Tenant not found.");
                    return NotFound(response);
                }

                await _unitOfWork.Tenants.Update(tenantDto);
                response.Result = "Tenant updated successfully.";
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

        // DELETE: api/tenants/{id} (Delete tenant)
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<APIResponse>> DeleteTenant(int id)
        {
            var response = new APIResponse();
            try
            {
                var existingTenant = await _unitOfWork.Tenants.GetByIdAsync(id);
                if (existingTenant == null)
                {
                    response.statusCode = HttpStatusCode.NotFound;
                    response.IsSuccess = false;
                    response.ErrorMasseges.Add("Tenant not found.");
                    return NotFound(response);
                }

                await _unitOfWork.Tenants.DeleteAsync(id);
                response.Result = "Tenant deleted successfully.";
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
}
