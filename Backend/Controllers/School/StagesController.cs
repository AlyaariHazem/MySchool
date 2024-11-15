using System.Collections.Generic;
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
        private readonly IStagesRepository stageRepo;
        private readonly IClassesRepository classRepo;

        public StagesController(IStagesRepository stageRepo, IClassesRepository classRepo)
        {
            this.stageRepo = stageRepo;
            this.classRepo = classRepo;
        }

        // POST api/stages
        [HttpPost]
        public async Task<ActionResult> Add(StagesDTO model)
            {
                if (!ModelState.IsValid)
                    return BadRequest("Invalid stage data.");
            
                await stageRepo.AddStage(model); // Ensure `await` is used here
                return Ok(new { message = "Stage added successfully." });
            }


        // GET api/stages
        [HttpGet]
        public async Task<IActionResult> GetStages()
        {
            var stages = await stageRepo.GetAll();
            return Ok(new { StagesInfo = stages });
        }

        // PUT api/stages/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStage(int id, UpdateStageDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid stage data.");

            var existingStage = await stageRepo.GetByIdAsync(id);
            if (existingStage == null)
                return NotFound(new { success = false, message = "Stage not found." });

            model.ID=existingStage.StageID;
           await stageRepo.Update(model);

            return Ok(new { message = "Stage updated successfully." ,model});
        }

        // DELETE api/stages/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStage(int id)
        {
            var stage = await stageRepo.GetByIdAsync(id);
            if (stage == null)
                return NotFound(new { success = false, message = "Stage not found." });

         await stageRepo.DeleteAsync(id);

            return Ok(new { success = true, message = "Stage deleted successfully." });
        }
        // PATCH api/stages/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatePartial(int id, [FromBody] JsonPatchDocument<StagesDTO> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest("Invalid patch document.");

            var success = await stageRepo.UpdatePartial(id, patchDoc);
            if (!success)
                return NotFound(new { success = false, message = "Stage not found or update failed." });

            return Ok(new { success = true, message = "Stage partially updated successfully." });
        }
    }
}
