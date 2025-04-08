using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Backend.DTOS.School.GradeTypes;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    public class GradeTypesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public GradeTypesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/GradeTypes
        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetAll()
        {
            var response = new APIResponse();
            try
            {
                var gradeTypes = await _unitOfWork.GradeTypes.GetAllAsync();
                response.Result = gradeTypes;
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

        // GET: api/GradeTypes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<APIResponse>> Get(int id)
        {
            var response = new APIResponse();
            var gradeType = await _unitOfWork.GradeTypes.GetByIdAsync(id);

            if (gradeType == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("GradeType not found.");
                return NotFound(response);
            }

            response.Result = gradeType;
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }

        // POST: api/GradeTypes
        [HttpPost]
        public async Task<ActionResult<APIResponse>> Create([FromBody] GradeTypesDTO gradeTypeDTO)
        {
            var response = new APIResponse();
            try
            {
                var createdGradeType = await _unitOfWork.GradeTypes.AddAsync(gradeTypeDTO);
                response.Result = createdGradeType;
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

        // PUT: api/GradeTypes/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<APIResponse>> Update(int id, [FromBody] GradeTypesDTO gradeTypeDTO)
        {
            var response = new APIResponse();
            try
            {
                gradeTypeDTO.GradeTypeID = id;
                await _unitOfWork.GradeTypes.UpdateAsync(gradeTypeDTO);
                response.Result = "GradeType updated successfully.";
                response.statusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("GradeType not found.");
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

        // DELETE: api/GradeTypes/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var response = new APIResponse();
            try
            {
                await _unitOfWork.GradeTypes.DeleteAsync(id);
                response.Result = "GradeType deleted successfully.";
                response.statusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("GradeType not found.");
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
