using AutoMapper;
using Backend.DTOS.School.WeeklySchedule;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Linq;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WeeklyScheduleController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public WeeklyScheduleController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET api/WeeklySchedule
        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetAllSchedules()
        {
            var response = new APIResponse();

            try
            {
                var schedules = await _unitOfWork.WeeklySchedules.GetAllAsync();
                response.Result = schedules;
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

        // GET api/WeeklySchedule/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<APIResponse>> GetSchedule(int id)
        {
            var response = new APIResponse();

            try
            {
                var schedule = await _unitOfWork.WeeklySchedules.GetByIdAsync(id);
                if (schedule == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Schedule not found.");
                    return NotFound(response);
                }

                response.Result = schedule;
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

        // GET api/WeeklySchedule/class/{classId}/term/{termId}
        [HttpGet("class/{classId:int}/term/{termId:int}")]
        public async Task<ActionResult<APIResponse>> GetScheduleByClassAndTerm(int classId, int termId, [FromQuery] int? divisionId = null)
        {
            var response = new APIResponse();

            try
            {
                var schedules = await _unitOfWork.WeeklySchedules.GetByClassAndTermAsync(classId, termId, divisionId);
                response.Result = schedules;
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

        // GET api/WeeklySchedule/grid/class/{classId}/term/{termId}
        [HttpGet("grid/class/{classId:int}/term/{termId:int}")]
        public async Task<ActionResult<APIResponse>> GetScheduleGrid(int classId, int termId, [FromQuery] int? divisionId = null)
        {
            var response = new APIResponse();

            try
            {
                var grid = await _unitOfWork.WeeklySchedules.GetScheduleGridAsync(classId, termId, divisionId);
                response.Result = grid;
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

        // POST api/WeeklySchedule
        [HttpPost]
        public async Task<ActionResult<APIResponse>> AddSchedule([FromBody] AddWeeklyScheduleDTO createDTO)
        {
            var response = new APIResponse();

            try
            {
                if (!ModelState.IsValid)
                {
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.IsSuccess = false;
                    response.ErrorMasseges.Add("Invalid schedule data.");
                    return BadRequest(response);
                }

                await _unitOfWork.WeeklySchedules.Add(createDTO);
                await _unitOfWork.CompleteAsync();

                response.Result = "Schedule added successfully";
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

        // PUT api/WeeklySchedule/{id}
        [HttpPut("{id:int}")]
        public async Task<ActionResult<APIResponse>> UpdateSchedule(int id, [FromBody] UpdateWeeklyScheduleDTO updateDTO)
        {
            var response = new APIResponse();

            try
            {
                if (!ModelState.IsValid)
                {
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.IsSuccess = false;
                    response.ErrorMasseges.Add("Invalid schedule data.");
                    return BadRequest(response);
                }

                var existingSchedule = await _unitOfWork.WeeklySchedules.GetByIdAsync(id);
                if (existingSchedule == null)
                {
                    response.statusCode = HttpStatusCode.NotFound;
                    response.IsSuccess = false;
                    response.ErrorMasseges.Add("Schedule not found.");
                    return NotFound(response);
                }

                updateDTO.WeeklyScheduleID = id;
                await _unitOfWork.WeeklySchedules.Update(updateDTO);
                await _unitOfWork.CompleteAsync();

                response.Result = "Schedule updated successfully.";
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

        // POST api/WeeklySchedule/bulk
        [HttpPost("bulk")]
        public async Task<ActionResult<APIResponse>> BulkUpdateSchedules([FromBody] List<AddWeeklyScheduleDTO> schedules)
        {
            var response = new APIResponse();

            try
            {
                if (!ModelState.IsValid || schedules == null || !schedules.Any())
                {
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.IsSuccess = false;
                    response.ErrorMasseges.Add("Invalid schedule data.");
                    return BadRequest(response);
                }

                await _unitOfWork.WeeklySchedules.BulkUpdateAsync(schedules);
                await _unitOfWork.CompleteAsync();

                response.Result = "Schedules updated successfully";
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

        // DELETE api/WeeklySchedule/{id}
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<APIResponse>> DeleteSchedule(int id)
        {
            var response = new APIResponse();

            try
            {
                var schedule = await _unitOfWork.WeeklySchedules.GetByIdAsync(id);
                if (schedule == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Schedule not found.");
                    return NotFound(response);
                }

                await _unitOfWork.WeeklySchedules.DeleteAsync(id);
                await _unitOfWork.CompleteAsync();

                response.Result = "Schedule deleted successfully.";
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
