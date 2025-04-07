using Backend.DTOS.School.Fees;
using Backend.Interfaces;
using Backend.Models;
using Backend.Repository.School.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public FeesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/Fees
        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetAllFees()
        {
            var response = new APIResponse();
            try
            {
                var fees = await _unitOfWork.Fees.GetAllAsync();
                
                response.Result = fees;
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

        // GET: api/Fees/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<APIResponse>> GetFeeById(int id)
        {
            var response = new APIResponse();
            try
            {
                var fee = await _unitOfWork.Fees.GetByIdAsync(id);
                if (fee == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Fee not found.");
                    return NotFound(response);
                }

                response.Result = fee;
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

        // POST: api/Fees
        [HttpPost]
        public async Task<ActionResult<APIResponse>> CreateFee([FromBody] FeeDTO fee)
        {
            var response = new APIResponse();
            try
            {
                if (!ModelState.IsValid)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("Invalid fee data.");
                    return BadRequest(response);
                }

                await _unitOfWork.Fees.AddAsync(fee);

                response.Result = "Fee created successfully.";
                response.statusCode = HttpStatusCode.Created;

                // You can either return Ok(...) or CreatedAtAction(...) 
                // but if you want correct REST semantics, use StatusCode/Created like:
                // return StatusCode((int)HttpStatusCode.Created, response);
                // For consistency with your other controllers, we can do:
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

        // PUT: api/Fees/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<APIResponse>> UpdateFee(int id, [FromBody] FeeDTO fee)
        {
            var response = new APIResponse();
            try
            {
                if (!ModelState.IsValid)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("Invalid fee data.");
                    return BadRequest(response);
                }

                var existingFee = await _unitOfWork.Fees.GetByIdAsync(id);
                if (existingFee == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Fee not found.");
                    return NotFound(response);
                }

                fee.FeeID = id;
                await _unitOfWork.Fees.UpdateAsync(fee);

                response.Result = "Fee updated successfully.";
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

        // DELETE: api/Fees/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<APIResponse>> DeleteFee(int id)
        {
            var response = new APIResponse();
            try
            {
                var existingFee = await _unitOfWork.Fees.GetByIdAsync(id);
                if (existingFee == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Fee not found.");
                    return NotFound(response);
                }

                await _unitOfWork.Fees.DeleteAsync(id);

                response.Result = "Fee deleted successfully.";
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
