using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Backend.DTOS.School.Guardians;
using Backend.Interfaces;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    public class GuardianController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public GuardianController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/Guardian
        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetAllGuardians()
        {
            var response = new APIResponse();
            try
            {
                var guardians = await _unitOfWork.Guardians.GetAllGuardiansAsync();

                response.Result = guardians;
                response.statusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.InternalServerError;
                response.ErrorMasseges.Add("An error occurred while fetching data.");
                // Optionally, add the actual exception message: response.ErrorMasseges.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }
        
        [HttpGet("GuardianInfo")]
        public async Task<ActionResult<APIResponse>> GetAllGuardiansInfo()
        {
            var response = new APIResponse();
            try
            {
                var guardiansInfo = await _unitOfWork.Guardians.GetAllGuardiansInfoAsync();

                response.Result = guardiansInfo;
                response.statusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.InternalServerError;
                response.ErrorMasseges.Add("An error occurred while fetching data.");
                // Optionally, add the actual exception message: response.ErrorMasseges.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }

        // GET: api/Guardian/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<APIResponse>> GetGuardianByID(int id)
        {
            var response = new APIResponse();
            try
            {
                if (id <= 0)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("Invalid ID provided.");
                    return BadRequest(response);
                }

                var guardian = await _unitOfWork.Guardians.GetGuardianByIdAsync(id);
                if (guardian == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Guardian not found.");
                    return NotFound(response);
                }

                response.Result = guardian;
                response.statusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occurred: {ex.Message}");
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.InternalServerError;
                response.ErrorMasseges.Add("An error occurred while fetching data.");
                // Optionally, add the actual exception message: response.ErrorMasseges.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<APIResponse>> UpdateGuardian(int id, [FromBody] GuardianDTO guardianDto)
        {
            var response = new APIResponse();
            try
            {
                // Optional: Ensure ID in URL matches DTO
                if (id != guardianDto.GuardianID)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("Mismatched Guardian ID.");
                    return BadRequest(response);
                }

                await _unitOfWork.Guardians.UpdateGuardianAsync(guardianDto);

                response.Result = "Guardian updated successfully.";
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
                // Log exception if needed
                Debug.WriteLine($"Error occurred: {ex.Message}");
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.InternalServerError;
                response.ErrorMasseges.Add("An error occurred while updating data.");
                // Optionally include the actual exception message
                // response.ErrorMasseges.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }

    }
}
