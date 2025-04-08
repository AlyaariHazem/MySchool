using System;
using System.Net;
using System.Threading.Tasks;
using Backend.DTOS.School.Terms;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    public class TermController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public TermController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // POST: api/Term
        [HttpPost]
        public async Task<ActionResult<APIResponse>> Create([FromBody] TermDTO dto)
        {
            var response = new APIResponse();
            try
            {
                await _unitOfWork.Terms.AddTermAsync(dto);
                response.Result = dto;
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

        // GET: api/Term
        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetAll()
        {
            var response = new APIResponse();
            try
            {
                var terms = await _unitOfWork.Terms.GetAllTermsAsync();
                response.Result = terms;
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

        // GET: api/Term/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<APIResponse>> GetById(int id)
        {
            var response = new APIResponse();
            try
            {
                var term = await _unitOfWork.Terms.GetTermByIdAsync(id);
                response.Result = term;
                response.statusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (ArgumentNullException)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add($"Term with ID {id} not found.");
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

        // PUT: api/Term/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<APIResponse>> Update(int id, [FromBody] TermDTO dto)
        {
            var response = new APIResponse();
            try
            {
                if (id != dto.TermID)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("Term ID mismatch.");
                    return BadRequest(response);
                }

                await _unitOfWork.Terms.UpdateTermAsync(dto);
                response.Result = "Term updated successfully.";
                response.statusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (ArgumentNullException)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add($"Term with ID {id} not found.");
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

        // DELETE: api/Term/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var response = new APIResponse();
            try
            {
                await _unitOfWork.Terms.DeleteTermAsync(id);
                response.Result = "Term deleted successfully.";
                response.statusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (ArgumentNullException)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add($"Term with ID {id} not found.");
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
    }
}
