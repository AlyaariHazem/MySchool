using System;
using System.Net;
using System.Threading.Tasks;
using Backend.DTOS.School.Subjects;
using Backend.Repository.School.Implements;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Backend.Interfaces;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public SubjectController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/subject/paginated?pageNumber=1&pageSize=10
        [HttpGet("paginated")]
        public async Task<ActionResult<APIResponse>> GetSubjectsPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var response = new APIResponse();
            try
            {
                if (pageNumber <= 0 || pageSize <= 0)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("Page number and page size must be greater than 0.");
                    return BadRequest(response);
                }

                var subjects = await _unitOfWork.Subjects.GetSubjectsPaginatedAsync(pageNumber, pageSize);
                var totalCount = await _unitOfWork.Subjects.GetTotalSubjectsCountAsync();

                var paginatedResult = new
                {
                    data = subjects,
                    pageNumber,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };

                response.Result = paginatedResult;
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


        // GET: api/subject/{id} (Get subject by ID)
        [HttpGet("{id:int}")]
        public async Task<ActionResult<APIResponse>> GetSubjectById(int id)
        {
            var response = new APIResponse();
            try
            {
                var subjectDto = await _unitOfWork.Subjects.GetSubjectByIdAsync(id);
                if (subjectDto == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add($"Subject with ID {id} not found.");
                    return NotFound(response);
                }

                response.Result = subjectDto;
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

        // POST: api/subject (Create a new subject)
        [HttpPost]
        public async Task<ActionResult<APIResponse>> CreateSubject([FromBody] SubjectsDTO subjectDto)
        {
            var response = new APIResponse();
            try
            {
                if (subjectDto == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("Invalid subject data.");
                    return BadRequest(response);
                }

                await _unitOfWork.Subjects.AddSubjectAsync(subjectDto);
                response.Result = subjectDto;
                response.statusCode = HttpStatusCode.Created;
                return Ok(response);
                // or: return CreatedAtAction(nameof(GetSubjectById), new { id = subjectDto.SubjectId }, response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.InternalServerError;
                response.ErrorMasseges.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }

        // PUT: api/subject/{id} (Update subject)
        [HttpPut("{id:int}")]
        public async Task<ActionResult<APIResponse>> UpdateSubject(int id, [FromBody] SubjectsDTO subjectDto)
        {
            var response = new APIResponse();
            try
            {
                if (subjectDto == null || id != subjectDto.SubjectID)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("Invalid subject data or ID mismatch.");
                    return BadRequest(response);
                }

                var existingSubject = await _unitOfWork.Subjects.GetSubjectByIdAsync(id);
                if (existingSubject == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Subject not found.");
                    return NotFound(response);
                }

                await _unitOfWork.Subjects.UpdateSubjectAsync(subjectDto);
                response.Result = "Subject updated successfully.";
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

        // DELETE: api/subject/{id} (Delete subject)
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<APIResponse>> DeleteSubject(int id)
        {
            var response = new APIResponse();
            try
            {
                var existingSubject = await _unitOfWork.Subjects.GetSubjectByIdAsync(id);
                if (existingSubject == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Subject not found.");
                    return NotFound(response);
                }

                await _unitOfWork.Subjects.DeleteSubjectAsync(id);
                response.Result = "Subject deleted successfully.";
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
