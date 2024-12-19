using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Backend.Repository.School.Implements;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.Controllers.School;

[Route("api/[controller]")]
[ApiController]
public class GuardianController : Controller
{
    private readonly IGuardianRepository _guardianRepository;

    public GuardianController(IGuardianRepository guardianRepository)
    {
        _guardianRepository= guardianRepository;
    }
   [HttpGet]
    public async Task<IActionResult> GetAllGuardians()
    {
        try
        {
            var guardians = await _guardianRepository.GetAllGuardiansAsync();
            return Ok(new { success = true, data = guardians });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, new { success = false, message = "An error occurred." });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetGuardianByID(int id)
    {
        try
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "Invalid ID provided." });

            var guardian = await _guardianRepository.GetGuardianByIdAsync(id);

            if (guardian == null)
                return NotFound(new { success = false, message = "Guardian not found." });

            return Ok(new { success = true, data = guardian });
        }
        catch (Exception ex)
        {
            // Log the error
            Debug.WriteLine($"Error occurred: {ex.Message}");
            return StatusCode(500, new { success = false, message = "An error occurred while fetching data." });
        }
    }

}
