using Backend.DTOS.School.Fees;
using Backend.Models;
using Backend.Repository.School.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FeesController : ControllerBase
{
    private readonly IFeesRepository _feesRepo;

    public FeesController(IFeesRepository feesRepo)
    {
        _feesRepo = feesRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllFees()
    {
        var fees = await _feesRepo.GetAllAsync();
        return Ok(new { success = true, data = fees });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetFeeById(int id)
    {
         var fee = await _feesRepo.GetByIdAsync(id);
        if (fee == null)
            return NotFound(new { success = false, message = "Fee not found." });

        return Ok(new { success = true, data = fee });
    }

    [HttpPost]
    public async Task<IActionResult> CreateFee([FromBody] FeeDTO fee)
    {
         if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Invalid fee data.", errors = ModelState });

        await _feesRepo.AddAsync(fee);
        return Ok(new { success = true, message = "Fee created successfully." });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFee(int id, [FromBody] FeeDTO fee)
    {
       if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Invalid fee data.", errors = ModelState });

        var existingFee = await _feesRepo.GetByIdAsync(id);
        if (existingFee == null)
            return NotFound(new { success = false, message = "Fee not found." });

        fee.FeeID = id;
        await _feesRepo.UpdateAsync(fee);

        return Ok(new { success = true, message = "Fee updated successfully.", data = fee });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFee(int id)
    {
        var existingFee = await _feesRepo.GetByIdAsync(id);
        if (existingFee == null)
            return NotFound(new { success = false, message = "Fee not found." });

        await _feesRepo.DeleteAsync(id);
        return Ok(new { success = true, message = "Fee deleted successfully." });
    }
}
