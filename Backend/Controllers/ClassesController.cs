using System.Collections.Generic;
using Backend.DTOS;
using Backend.Models;
using Backend.Repository;
using Backend.Repository.School;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassesController : ControllerBase
    {
        private readonly IClassesRepository classRepo;
        private readonly IStagesRepository stageRepo;

        public ClassesController(IClassesRepository _classRepo, IStagesRepository _stageRepo)
        {
            classRepo = _classRepo;
            stageRepo = _stageRepo;
        }

        [HttpPost("add")]
        public IActionResult AddClass([FromBody] AddClassDTO model)
        {
            classRepo.Add(model);

            // Retrieve updated list of classes
            var classes = classRepo.DisplayClasses();

            return Ok(new { success = true, data = classes });
        }

        [HttpPut("update")]
        public IActionResult UpdateClass([FromBody] AddClassDTO model)
        {
            classRepo.Update(model);

            // Retrieve updated list of stages
            var stages = stageRepo.DisplayStages();

            return Ok(new { success = true, data = stages });
        }
        [Authorize]
        [HttpGet("display")]
        public IActionResult DisplayClassesInfo()
        {
            var classes = classRepo.DisplayClasses();
            // var stages = stageRepo.DisplayStages();

            return Ok(new { classes });
        }

        [HttpDelete("delete/{id}")]
        public IActionResult DeleteClassByID(int id)
        {
            var classData = classRepo.GetById(id);

            if (classData == null)
            {
                return NotFound(new { success = false, message = "الصـف غير موجود" });
            }

            classRepo.Delete(id);

            // Retrieve updated lists of classes and stages after deletion
            var classes = classRepo.DisplayClasses();
            var stages = stageRepo.DisplayStages();

            return Ok(new { success = true, classes, stages });
        }
    }
}
