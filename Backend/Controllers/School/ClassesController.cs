using System.Collections.Generic;
using AutoMapper;
using Backend.DTOS;
using Backend.DTOS.School.Stages;
using Backend.Models;
using Backend.Repository;
using Backend.Repository.School;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassesController : ControllerBase
    {
        protected readonly APIResponse _response;
        private readonly IClassesRepository classRepo;
        private readonly IStagesRepository stageRepo;

        public ClassesController(IClassesRepository classRepo, IStagesRepository stageRepo)
        {
            this.classRepo = classRepo;
            this.stageRepo = stageRepo;
            this._response = new();
        }

        // POST api/classes
        [HttpPost]
        public async Task<ActionResult<APIResponse>> AddClass([FromBody] AddClassDTO createDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid class data.");
          await  classRepo.Add(createDTO);

            return Ok(new { success = true });
        }

        // PUT api/classes/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateClass(int id, [FromBody] AddClassDTO updateClass)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid class data.");

            var existingClass =await classRepo.GetByIdAsync(id);
            if (existingClass == null)
                return NotFound(new { success = false, message = "Class not found." });
            updateClass.ClassID = id;

          await  classRepo.Update(updateClass);

            return Ok(new { message = "Class updated successfully." ,updateClass});
        }

        // GET api/classes
        // [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetClasses()
        {
            var classes = await classRepo.GetAllAsync();
            return Ok(new { ClassInfo = classes });
        }

        // GET api/classes/{id}
        [HttpGet("{id:int}")]
        public IActionResult GetClass(int id)
        {
            var classes =  classRepo.GetByIdAsync(id);
            return Ok(new { ClassInfo = classes });
        }

        // DELETE api/classes/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteClass(int id)
        {
            var classData = await classRepo.GetByIdAsync(id);
            if (classData == null)
                return NotFound(new { success = false, message = "Class not found." });

          await  classRepo.DeleteAsync(id);

            var classes = await classRepo.GetAllAsync();
            return Ok(new { success = true, classes});
        }

         // PATCH api/Classes/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatePartial(int id, [FromBody] JsonPatchDocument<UpdateClassDTO> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest("Invalid patch document.");

            var success = await classRepo.UpdatePartial(id, patchDoc);
            if (!success)
                return NotFound(new { success = false, message = "Class not found or update failed." });

            return Ok(new { success = true, message = "Class partially updated successfully." });
        }
    }
}
