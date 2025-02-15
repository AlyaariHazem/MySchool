using Backend.DTOS.School.Fees;
using Backend.Models;
using Backend.Repository.School.Implements;
using Backend.Repository.School.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeeClassController : ControllerBase
    {
        private readonly IFeeClassRepository _feeClassRepository;

        public FeeClassController(IFeeClassRepository feeClassRepository)
        {
            _feeClassRepository = feeClassRepository;
        }

        // GET: api/FeeClass
        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetAllFeeClasses()
        {
            var response = new APIResponse();
            try
            {
                var feeClasses = await _feeClassRepository.GetAllAsync();
                response.Result = feeClasses;
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

        // GET: api/FeeClass/Fee/{feeClassID:int}
        [HttpGet("Fee/{feeClassID:int}")]
        public async Task<ActionResult<APIResponse>> GetFeeClassById(int feeClassID)
        {
            var response = new APIResponse();
            try
            {
                var feeClass = await _feeClassRepository.GetByIdAsync(feeClassID);
                if (feeClass == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("FeeClass not found");
                    return NotFound(response);
                }

                response.Result = feeClass;
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

        // GET: api/FeeClass/Class/{classId:int}
        [HttpGet("Class/{classId:int}")]
        public async Task<ActionResult<APIResponse>> GetAllFeeClassById(int classId)
        {
            var response = new APIResponse();
            try
            {
                var feeClass = await _feeClassRepository.GetAllByClassIdAsync(classId);
                if (feeClass == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("FeeClass not found");
                    return NotFound(response);
                }

                response.Result = feeClass;
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

        // POST: api/FeeClass
        [HttpPost]
        public async Task<ActionResult<APIResponse>> AddFeeClass([FromBody] AddFeeClassDTO feeClass)
        {
            var response = new APIResponse();
            try
            {
                if (feeClass == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("Invalid FeeClass data");
                    return BadRequest(response);
                }

                await _feeClassRepository.AddAsync(feeClass);

                response.Result = "FeeClass created successfully.";
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

        // PUT: api/FeeClass/{feeClassID}
        [HttpPut("{feeClassID:int}")]
        public async Task<ActionResult<APIResponse>> UpdateFeeClass(int feeClassID, [FromBody] AddFeeClassDTO updatedFeeClass)
        {
            var response = new APIResponse();
            try
            {
                var existingFeeClass = await _feeClassRepository.GetByIdAsync(feeClassID);
                if (existingFeeClass == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("FeeClass not found");
                    return NotFound(response);
                }

                await _feeClassRepository.UpdateAsync(feeClassID, updatedFeeClass);

                response.Result = "FeeClass updated successfully.";
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

        // DELETE: api/FeeClass/{FeeClassId}
        [HttpDelete("{FeeClassId:int}")]
        public async Task<ActionResult<APIResponse>> DeleteFeeClass(int FeeClassId)
        {
            var response = new APIResponse();
            try
            {
                var check = await _feeClassRepository.checkIfExist(FeeClassId);
                if (!check)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("FeeClass not found");
                    return NotFound(response);
                }

                await _feeClassRepository.DeleteAsync(FeeClassId);
                response.Result = "FeeClass deleted successfully.";
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
