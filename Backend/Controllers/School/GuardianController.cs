using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    public class GuardianController : Controller
    {
        private readonly IGuardianRepository _guardianRepository;

        public GuardianController(IGuardianRepository guardianRepository)
        {
            _guardianRepository = guardianRepository;
        }

        // GET: api/Guardian
        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetAllGuardians()
        {
            var response = new APIResponse();
            try
            {
                var guardians = await _guardianRepository.GetAllGuardiansAsync();

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

                var guardian = await _guardianRepository.GetGuardianByIdAsync(id);
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
    }
}
