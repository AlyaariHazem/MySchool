using System.Net;
using System.Threading.Tasks;
using Backend.DTOS;
using Backend.DTOS.School.Stages;
using Backend.Models;
using Backend.Repository;
using Backend.Repository.School;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    public class StagesController : ControllerBase
    {
        private readonly IStagesRepository _stageRepo;
        private readonly IClassesRepository _classRepo;

        public StagesController(IStagesRepository stageRepo, IClassesRepository classRepo)
        {
            _stageRepo = stageRepo;
            _classRepo = classRepo;
        }

        // POST api/stages
        [HttpPost]
        public async Task<ActionResult<APIResponse>> Add([FromBody] StagesDTO model)
        {
            var response = new APIResponse();
            try
            {
                if (!ModelState.IsValid)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("Invalid stage data.");
                    return BadRequest(response);
                }

                await _stageRepo.AddStage(model);
                response.Result = "Stage added successfully.";
                response.statusCode = HttpStatusCode.Created;
                return Ok(response); // or StatusCode((int)HttpStatusCode.Created, response);
            }
            catch (System.Exception ex)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.InternalServerError;
                response.ErrorMasseges.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }

        // GET api/stages
        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetStages()
        {
            var response = new APIResponse();
            try
            {
                var stages = await _stageRepo.GetAll();
                response.Result = stages;
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

        // PUT api/stages/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<APIResponse>> UpdateStage(int id, [FromBody] UpdateStageDTO model)
        {
            var response = new APIResponse();
            try
            {
                if (!ModelState.IsValid)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("Invalid stage data.");
                    return BadRequest(response);
                }

                var existingStage = await _stageRepo.GetByIdAsync(id);
                if (existingStage == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Stage not found.");
                    return NotFound(response);
                }

                // Ensure the incoming model has the correct ID:
                model.ID = existingStage.StageID;

                await _stageRepo.Update(model);

                response.Result = "Stage updated successfully.";
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

        // DELETE api/stages/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<APIResponse>> DeleteStage(int id)
        {
            var response = new APIResponse();
            try
            {
                var stage = await _stageRepo.GetByIdAsync(id);
                if (stage == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Stage not found.");
                    return NotFound(response);
                }

                await _stageRepo.DeleteAsync(id);

                response.Result = "Stage deleted successfully.";
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

        // PATCH api/stages/{id}
        [HttpPatch("{id}")]
        public async Task<ActionResult<APIResponse>> UpdatePartial(int id, [FromBody] JsonPatchDocument<StagesDTO> patchDoc)
        {
            var response = new APIResponse();
            try
            {
                if (patchDoc == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("Invalid patch document.");
                    return BadRequest(response);
                }

                var success = await _stageRepo.UpdatePartial(id, patchDoc);
                if (!success)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Stage not found or update failed.");
                    return NotFound(response);
                }

                response.Result = "Stage partially updated successfully.";
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
