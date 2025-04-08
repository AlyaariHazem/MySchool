using System;
using System.Net;
using System.Threading.Tasks;
using Backend.DTOS.School.Months;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    public class MonthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public MonthController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // POST: api/Month
        [HttpPost]
        public async Task<ActionResult<APIResponse>> Create([FromBody] MonthDTO dto)
        {
            var response = new APIResponse();
            try
            {
                await _unitOfWork.Months.AddMonthAsync(dto);
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

        // GET: api/Month
        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetAll()
        {
            var response = new APIResponse();
            try
            {
                var months = await _unitOfWork.Months.GetAllMonthsAsync();
                response.Result = months;
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

        // GET: api/Month/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<APIResponse>> GetById(int id)
        {
            var response = new APIResponse();
            try
            {
                var month = await _unitOfWork.Months.GetMonthByIdAsync(id);
                response.Result = month;
                response.statusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (ArgumentNullException)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add($"Month with ID {id} not found.");
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

        // PUT: api/Month/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<APIResponse>> Update(int id, [FromBody] MonthDTO dto)
        {
            var response = new APIResponse();
            try
            {
                if (id != dto.MonthID)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("Month ID mismatch.");
                    return BadRequest(response);
                }

                await _unitOfWork.Months.UpdateMonthAsync(dto);
                response.Result = "Month updated successfully.";
                response.statusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (ArgumentNullException)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add($"Month with ID {id} not found.");
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

        // DELETE: api/Month/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var response = new APIResponse();
            try
            {
                await _unitOfWork.Months.DeleteMonthAsync(id);
                response.Result = "Month deleted successfully.";
                response.statusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (ArgumentNullException)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add($"Month with ID {id} not found.");
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
