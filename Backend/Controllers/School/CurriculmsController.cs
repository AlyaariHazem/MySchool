using System.Net;
using System.Threading.Tasks;
using Backend.DTOS.School.Curriculms;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurriculmsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CurriculmsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // POST: api/curriculms
        [HttpPost]
        public async Task<ActionResult<APIResponse>> Add([FromBody] CurriculumDTO dto)
        {
            var response = new APIResponse();
            try
            {
                if (!ModelState.IsValid)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("Invalid curriculum data.");
                    return BadRequest(response);
                }

                var created = await _unitOfWork.Curriculums.AddAsync(dto);
                await _unitOfWork.CompleteAsync();

                if(created == false)
                response.Result = "Curriculum already exists.";
                else
                response.Result = "Curriculum added successfully.";

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

        // GET: api/curriculms
        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetAll()
        {
            var response = new APIResponse();
            try
            {
                var result = await _unitOfWork.Curriculums.GetAllAsync();
                response.Result = result;
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

        // GET: api/curriculms/{subjectId}/{classId}
        [HttpGet("{subjectId}/{classId}")]
        public async Task<ActionResult<APIResponse>> Get(int subjectId, int classId)
        {
            var response = new APIResponse();
            try
            {
                var result = await _unitOfWork.Curriculums.GetByIdAsync(subjectId, classId);
                if (result == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Curriculum not found.");
                    return NotFound(response);
                }

                response.Result = result;
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

        // PUT: api/curriculms/{subjectId}/{classId}
        [HttpPut("{subjectId}/{classId}")]
        public async Task<ActionResult<APIResponse>> Update(int subjectId, int classId, [FromBody] CurriculumDTO dto)
        {
            var response = new APIResponse();
            try
            {
                var existing = await _unitOfWork.Curriculums.GetByIdAsync(subjectId, classId);
                if (existing == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Curriculum not found.");
                    return NotFound(response);
                }

                dto.SubjectID = subjectId;
                dto.ClassID = classId;
                await _unitOfWork.Curriculums.UpdateAsync(dto);
                await _unitOfWork.CompleteAsync();

                response.Result = "Curriculum updated successfully.";
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

        // DELETE: api/curriculms/{subjectId}/{classId}
        [HttpDelete("{subjectId}/{classId}")]
        public async Task<ActionResult<APIResponse>> Delete(int subjectId, int classId)
        {
            var response = new APIResponse();
            try
            {
                var existing = await _unitOfWork.Curriculums.GetByIdAsync(subjectId, classId);
                if (existing == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Curriculum not found.");
                    return NotFound(response);
                }

                await _unitOfWork.Curriculums.DeleteAsync(subjectId, classId);
                await _unitOfWork.CompleteAsync();

                response.Result = "Curriculum deleted successfully.";
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
