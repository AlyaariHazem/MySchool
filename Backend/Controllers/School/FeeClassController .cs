using Backend.DTOS.School.Fees;
using Backend.Interfaces;
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
        private readonly IUnitOfWork _unitOfWork;

        public FeeClassController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/FeeClass
        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetAllFeeClasses()
        {
            var response = new APIResponse();
            try
            {
                var feeClasses = await _unitOfWork.FeeClasses.GetAllAsync();
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
                var feeClass = await _unitOfWork.FeeClasses.GetByIdAsync(feeClassID);
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
                var feeClass = await _unitOfWork.FeeClasses.GetAllByClassIdAsync(classId);
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

                await _unitOfWork.FeeClasses.AddAsync(feeClass);

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
                var existingFeeClass = await _unitOfWork.FeeClasses.GetByIdAsync(feeClassID);
                if (existingFeeClass == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("FeeClass not found");
                    return NotFound(response);
                }

                await _unitOfWork.FeeClasses.UpdateAsync(feeClassID, updatedFeeClass);

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
                var check = await _unitOfWork.FeeClasses.checkIfExist(FeeClassId);
                if (!check)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("FeeClass not found");
                    return NotFound(response);
                }

                await _unitOfWork.FeeClasses.DeleteAsync(FeeClassId);
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
