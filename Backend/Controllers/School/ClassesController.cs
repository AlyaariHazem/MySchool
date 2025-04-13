using AutoMapper;
using Backend.DTOS;
using Backend.DTOS.School.Stages;
using Backend.Interfaces;
using Backend.Models;
using Backend.Repository;
using Backend.Repository.School;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassesController : ControllerBase
    {
        // Instead of storing only a reference, we'll create a new APIResponse each time.
        // But you can also keep a single _response field if you prefer.
        private readonly IUnitOfWork _unitOfWork;

        public ClassesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // POST api/classes
        [HttpPost]
        public async Task<ActionResult<APIResponse>> AddClass([FromBody] AddClassDTO createDTO)
        {
            var response = new APIResponse();

            try
            {
                if (!ModelState.IsValid)
                {
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.IsSuccess = false;
                    response.ErrorMasseges.Add("Invalid class data.");
                    return BadRequest(response);
                }

                await _unitOfWork.Classes.Add(createDTO);

                response.Result = "Class added successfully";
                response.statusCode = HttpStatusCode.Created;  // or OK, depending on your preference
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

        // PUT api/classes/{id}
        [HttpPut("{id:int}")]
        public async Task<ActionResult<APIResponse>> UpdateClass(int id, [FromBody] AddClassDTO updateClass)
        {
            var response = new APIResponse();

            try
            {
                if (!ModelState.IsValid)
                {
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.IsSuccess = false;
                    response.ErrorMasseges.Add("Invalid class data.");
                    return BadRequest(response);
                }

                var existingClass = await _unitOfWork.Classes.GetByIdAsync(id);
                if (existingClass == null)
                {
                    response.statusCode = HttpStatusCode.NotFound;
                    response.IsSuccess = false;
                    response.ErrorMasseges.Add("Class not found.");
                    return NotFound(response);
                }

                updateClass.ClassID = id;
                await _unitOfWork.Classes.Update(updateClass);

                response.Result = "Class updated successfully.";
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

        // GET api/classes
        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetClasses()
        {
            var response = new APIResponse();

            try
            {
                var classes = await _unitOfWork.Classes.GetAllAsync();

                response.Result = classes;
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
        // GET api/classes
        [HttpGet("GetAllNameClasses")]
        public async Task<ActionResult<APIResponse>> GetAllNameClasses()
        {
            var response = new APIResponse();

            try
            {
                var classes = await _unitOfWork.Classes.GetAllNamesAsync();

                response.Result = classes;
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

        // GET api/classes/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<APIResponse>> GetClass(int id)
        {
            var response = new APIResponse();
            try
            {
                var classEntity = await _unitOfWork.Classes.GetByIdAsync(id);
                if (classEntity == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Class not found.");
                    return NotFound(response);
                }

                response.Result = classEntity;
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

        // DELETE api/classes/{id}
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<APIResponse>> DeleteClass(int id)
        {
            var response = new APIResponse();
            try
            {
                var classData = await _unitOfWork.Classes.GetByIdAsync(id);
                if (classData == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Class not found.");
                    return NotFound(response);
                }

                await _unitOfWork.Classes.DeleteAsync(id);
                var classes = await _unitOfWork.Classes.GetAllAsync();

                response.Result = "Class deleted successfully.";
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

        // PATCH api/Classes/{id}
        [HttpPatch("{id}")]
        public async Task<ActionResult<APIResponse>> UpdatePartial(int id, [FromBody] JsonPatchDocument<UpdateClassDTO> patchDoc)
        {
            var response = new APIResponse();
            try
            {
                if (patchDoc == null)
                {
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.IsSuccess = false;
                    response.ErrorMasseges.Add("Invalid patch document.");
                    return BadRequest(response);
                }

                var success = await _unitOfWork.Classes.UpdatePartial(id, patchDoc);
                if (!success)
                {
                    response.statusCode = HttpStatusCode.NotFound;
                    response.IsSuccess = false;
                    response.ErrorMasseges.Add("Class not found or update failed.");
                    return NotFound(response);
                }

                response.Result = "Class partially updated successfully.";
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
