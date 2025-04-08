using System;
using System.Net;
using System.Threading.Tasks;
using Backend.DTOS.School.MonthlyGrade;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    public class MonthlyGradesController : ControllerBase
    {
        private readonly IMonthlyGradeRepository _monthlyGradeRepository;

        public MonthlyGradesController(IMonthlyGradeRepository monthlyGradeRepository)
        {
            _monthlyGradeRepository = monthlyGradeRepository;
        }

        // POST: api/MonthlyGrades
        [HttpPost]
        public async Task<ActionResult<APIResponse>> Create([FromBody] MonthlyGradeDTO dto)
        {
            var response = new APIResponse();
            try
            {
                var createdGrade = await _monthlyGradeRepository.AddAsync(dto);
                response.Result = createdGrade;
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

        // GET: api/MonthlyGrades/{term}/{monthId}/{classId}/{gradeTypeId}
        [HttpGet("{term}/{monthId}/{classId}/{gradeTypeId}")]
        public async Task<ActionResult<APIResponse>> GetAll(int term, int monthId, int classId)
        {
            var response = new APIResponse();
            try
            {
                var grades = await _monthlyGradeRepository.GetAllAsync(term, monthId, classId);
                response.Result = grades;
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

        // PUT: api/MonthlyGrades/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<APIResponse>> Update(int id, [FromBody] MonthlyGradeDTO dto)
        {
            var response = new APIResponse();
            try
            {
                dto.MonthlyGradeID = id;
                var isUpdated = await _monthlyGradeRepository.UpdateAsync(dto);
                if (isUpdated)
                {
                    response.Result = "Monthly grade updated successfully.";
                    response.statusCode = HttpStatusCode.OK;
                    return Ok(response);
                }
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("Monthly grade not found.");
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

        // DELETE: api/MonthlyGrades/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var response = new APIResponse();
            try
            {
                var isDeleted = await _monthlyGradeRepository.DeleteAsync(id);
                if (isDeleted)
                {
                    response.Result = "Monthly grade deleted successfully.";
                    response.statusCode = HttpStatusCode.OK;
                    return Ok(response);
                }
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add("Monthly grade not found.");
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
