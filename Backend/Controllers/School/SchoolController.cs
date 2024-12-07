using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.DTOS.School;
using Backend.Repository.School.Implements;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolController : ControllerBase
    {
        private readonly ISchoolRepository _schoolRepository;

        public SchoolController(ISchoolRepository schoolRepository)
        {
            _schoolRepository = schoolRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSchools()
        {
            var schools = await _schoolRepository.GetByIdAsync();
            return Ok(schools);
        }

        [HttpPost]
        public async Task<IActionResult> AddSchool([FromBody] SchoolDTO schoolDTO)
        {
            if (schoolDTO == null)
            {
                return BadRequest("Invalid school data.");
            }

            await _schoolRepository.AddAsync(schoolDTO);
            return CreatedAtAction(nameof(GetAllSchools), new { id = schoolDTO.SchoolID }, schoolDTO);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchool(int id, [FromBody] SchoolDTO schoolDTO)
        {
            if (schoolDTO == null || id != schoolDTO.SchoolID)
            {
                return BadRequest("School data is invalid or ID mismatch.");
            }

            try
            {
                await _schoolRepository.UpdateAsync(schoolDTO);
                return NoContent(); // HTTP 204: Successfully updated
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchool(int id)
        {
            try
            {
                await _schoolRepository.DeleteAsync(id);
                return NoContent(); // HTTP 204: Successfully deleted
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
