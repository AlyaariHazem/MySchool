using System.Net;
using System.Threading.Tasks;
using Backend.DTOS.School.Years;
using Backend.Models;
using Backend.Repository.School.Classes;
using Backend.Repository.School.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    public class YearController : ControllerBase
    {
        private readonly IYearRepository _yearRepo;

        public YearController(IYearRepository yearRepo)
        {
            _yearRepo = yearRepo;
        }

        // POST api/year
        [HttpPost]
        public async Task<ActionResult<APIResponse>> Add([FromBody] YearDTO model)
        {
            var response = new APIResponse();
            try
            {
                if (!ModelState.IsValid)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("Invalid year data.");
                    return BadRequest(response);
                }
                
                await _yearRepo.Add(model);
                response.Result = "Year added successfully.";
                response.statusCode = HttpStatusCode.Created;
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

        // GET api/year
        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetYears()
        {
            var response = new APIResponse();
            try
            {
                var years = await _yearRepo.GetAll();
                response.Result = years;
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

        // GET api/year/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<APIResponse>> Get(int id)
        {
            var response = new APIResponse();
            try
            {
                var year = await _yearRepo.GetByIdAsync(id);
                if (year == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Year not found.");
                    return NotFound(response);
                }
                response.Result = year;
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

        // PUT api/year/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<APIResponse>> Update(int id, [FromBody] YearDTO model)
        {
            var response = new APIResponse();
            try
            {
                if (!ModelState.IsValid)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("Invalid year data.");
                    return BadRequest(response);
                }

                var existingYear = await _yearRepo.GetByIdAsync(id);
                if (existingYear == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Year not found.");
                    return NotFound(response);
                }

                await _yearRepo.Update(model);
                response.Result = "Year updated successfully.";
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

        // DELETE api/year/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var response = new APIResponse();
            try
            {
                var year = await _yearRepo.GetByIdAsync(id);
                if (year == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Year not found.");
                    return NotFound(response);
                }

                await _yearRepo.DeleteAsync(id);
                response.Result = "Year deleted successfully.";
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
    }
}
