using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Backend.DTOS.School;
using Backend.Interfaces;
using Backend.Models;
using Backend.Repository.School.Implements;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly TenantProvisioningService _tenantProvisioningService;

        public SchoolController(IUnitOfWork unitOfWork, TenantProvisioningService tenantProvisioningService)
        {
            _unitOfWork= unitOfWork;
            _tenantProvisioningService = tenantProvisioningService;
        }

        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetAllSchools()
        {
            var response = new APIResponse();
            try
            {
                var schools = await _unitOfWork.Schools.GetAllAsync();
                response.Result = schools;
                response.statusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.InternalServerError;
                response.ErrorMasseges.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<APIResponse>> GetSchoolById(int id)
        {
            var response = new APIResponse();
            try
            {
                var school = await _unitOfWork.Schools.GetByIdAsync(id);
                response.Result = school;
                response.statusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add(ex.Message);
                return NotFound(response);
            }
        }

        [HttpPost]
        public async Task<ActionResult<APIResponse>> AddSchool([FromBody] SchoolDTO schoolDTO)
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

                // 1) Create a record in your *master* db for the "school" meta data
                // (if you want to store basic info in master, you can do it here)

                // 2) Use the provisioning service to create & migrate the new db
                // This creates the School in both admin DB and tenant DB with matching SchoolID
                var tenant = await _tenantProvisioningService.CreateSchoolDatabaseAsync(schoolDTO.SchoolName, schoolDTO);

                // 3) Return the newly created resource, incl. tenant ID or connection
                // The school has already been created in both databases by the provisioning service
                response.Result = new
                {
                    TenantId = tenant.TenantId,
                    SchoolName = tenant.SchoolName,
                    ConnectionString = tenant.ConnectionString
                };
                response.statusCode = HttpStatusCode.Created;
                return StatusCode((int)HttpStatusCode.Created, response);
            }
            catch (System.Exception ex)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.InternalServerError;
                response.ErrorMasseges.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<APIResponse>> UpdateSchool(int id, [FromBody] SchoolDTO schoolDTO)
        {
            var response = new APIResponse();
            try
            {
                if (schoolDTO == null || id != schoolDTO.SchoolID)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("School data is invalid or ID mismatch.");
                    return BadRequest(response);
                }

                await _unitOfWork.Schools.UpdateAsync(schoolDTO);

                // HTTP 204 is often used for a successful update with no content.
                // But if you prefer returning data, you can return 200 + updated object.
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
            catch (System.Exception ex)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.InternalServerError;
                response.ErrorMasseges.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<APIResponse>> DeleteSchool(int id)
        {
            var response = new APIResponse();
            try
            {
                await _unitOfWork.Schools.DeleteAsync(id);
                response.Result = "School deleted successfully.";
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
            catch (System.Exception ex)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.InternalServerError;
                response.ErrorMasseges.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }
    }
}
