using System.Net;
using Microsoft.AspNetCore.Mvc;
using Backend.Repository.School;
using Backend.DTOS.School.Stages;
using Backend.Models;
using Microsoft.AspNetCore.JsonPatch;
using Backend.DTOS;
using Backend.Interfaces;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    public class DivisionsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public DivisionsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // POST api/divisions
        [HttpPost]
        public async Task<ActionResult<APIResponse>> AddDivision([FromBody] AddDivisionDTO division)
        {
            var response = new APIResponse();

            try
            {
                if (division == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("Invalid division data.");
                    return BadRequest(response);
                }

                await _unitOfWork.Divisions.Add(division);

                response.Result = "Division added successfully.";
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

        // PUT api/divisions/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<APIResponse>> UpdateDivision(int id, [FromBody] DivisionDTO division)
        {
            var response = new APIResponse();

            try
            {
                var existingDivision = await _unitOfWork.Divisions.GetByIdAsync(id);
                if (existingDivision == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Division not found.");
                    return NotFound(response);
                }

                // Assign the ID to the incoming DTO to ensure it's set correctly
                division.DivisionID = id;
                await _unitOfWork.Divisions.Update(division);

                response.Result = "Division updated successfully.";
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

        // GET api/divisions
        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetDivisions()
        {
            var response = new APIResponse();

            try
            {
                var divisions = await _unitOfWork.Divisions.GetAll();
                response.Result = divisions;
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

        // DELETE api/divisions/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<APIResponse>> DeleteDivision(int id)
        {
            var response = new APIResponse();

            try
            {
                var existingDivision = await _unitOfWork.Divisions.GetByIdAsync(id);
                if (existingDivision == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Division not found.");
                    return NotFound(response);
                }

                await _unitOfWork.Divisions.DeleteAsync(id);
                response.Result = "Division deleted successfully.";
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

        // PATCH api/divisions/{id}
        [HttpPatch("{id}")]
        public async Task<ActionResult<APIResponse>> PatchDivision(int id, [FromBody] JsonPatchDocument<UpdateDivisionDTO> patchDoc)
        {
            var response = new APIResponse();

            try
            {
                if (patchDoc == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("Invalid patch document.");
                    return BadRequest(response);
                }

                var success = await _unitOfWork.Divisions.UpdatePartial(id, patchDoc);
                if (!success)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Division not found or update failed.");
                    return NotFound(response);
                }

                response.Result = "Division partially updated successfully.";
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
