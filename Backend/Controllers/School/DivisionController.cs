using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Backend.Repository;
using Backend.Repository.School;
using Backend.DTOS;
using Backend.DTOS.School.Stages;
using Backend.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    public class DivisionsController : ControllerBase
    {
        private readonly IDivisionRepository divisionRepo;
        private readonly IClassesRepository classRepo;

        public DivisionsController(IDivisionRepository divisionRepo, IClassesRepository classRepo)
        {
            this.divisionRepo = divisionRepo;
            this.classRepo = classRepo;
        }

        // POST api/divisions
        [HttpPost]
        public async Task<ActionResult<APIResponse>> AddDivision(AddDivisionDTO division)
        {
            if (division == null)
                return BadRequest("Invalid division data.");

            await divisionRepo.Add(division);

            return Ok(new { success = true, message = "Division added successfully" });
        }


        // PUT api/divisions/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDivision(int id, DivisionDTO division)
        {
            division.DivisionID = id;
            var divisions = await divisionRepo.GetByIdAsync(id);
            if (divisions == null)
                return BadRequest("Invalid division data.");

           await divisionRepo.Update(division);
            return Ok(new { success = true, message = "Division updated successfully" });
        }

        // GET api/divisions
        [HttpGet]
        public async Task<IActionResult> GetDivisions()
        {
            var divisions = await divisionRepo.GetAll();
            return Ok(new { DivisionInfo = divisions });
        }

        // DELETE api/divisions/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDivision(int id)
        {
            var division =await divisionRepo.GetByIdAsync(id);
            if (division == null)
                return NotFound(new { success = false, message = "Division not found." });

          await  divisionRepo.DeleteAsync(id);
            return Ok(new { success=true,message="Division deleted successflly"});
        }
        //Patch api/divisions/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchDivision(int id, [FromBody] JsonPatchDocument<UpdateDivisionDTO> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest("Invalid patch document.");

            var division = await divisionRepo.UpdatePartial(id, patchDoc);
            if (!division)
            return NotFound(new { success = false, message = "Division not found or update failed." });

            return Ok(new { success = true, message = "Division partially updated successfully." });
        }
    }
}
