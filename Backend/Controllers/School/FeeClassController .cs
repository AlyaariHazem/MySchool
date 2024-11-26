using Backend.DTOS.School.Fees;
using Backend.Models;
using Backend.Repository.School.Implements;
using Backend.Repository.School.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

[Route("api/[controller]")]
[ApiController]
public class FeeClassController : ControllerBase
{
    private readonly IFeeClassRepository _feeClassRepository;

    public FeeClassController(IFeeClassRepository feeClassRepository)
    {
        _feeClassRepository = feeClassRepository;
    }

    // GET: api/FeeClass
    [HttpGet]
    public async Task<IActionResult> GetAllFeeClasses()
    {
        var feeClasses = await _feeClassRepository.GetAllAsync();
        return Ok(new { success = true, data = feeClasses });
    }

    // GET: api/FeeClass/{classId}/{feeId}
    [HttpGet("{classId:int}/{feeId:int}")]
    public async Task<IActionResult> GetFeeClassById(int classId, int feeId)
    {
        var feeClass = await _feeClassRepository.GetByIdAsync(classId, feeId);
        if (feeClass == null)
        {
            return NotFound(new {success = false, data = "FeeClass not found" });
        }

        return Ok(new { success = true, data = feeClass });
    }

    // POST: api/FeeClass
    [HttpPost]
    public async Task<IActionResult> AddFeeClass([FromBody] AddFeeClassDTO feeClass)
    {
        if (feeClass == null)
        {
            return BadRequest(new {success=false, data = "Invalid FeeClass data" });
        }

        await _feeClassRepository.AddAsync(feeClass);
        return Ok(new { success = true, data = "FeeClass created successfully." });
    }

    // PUT: api/FeeClass/{classId}/{feeId}
    [HttpPut("{classId:int}/{feeId:int}")]
    public async Task<IActionResult> UpdateFeeClass(int classId, int feeId, [FromBody] AddFeeClassDTO updatedFeeClass)
    {
       
        var existingFeeClass = await _feeClassRepository.GetByIdAsync(classId, feeId);
        if (existingFeeClass == null)
        {
            return NotFound(new {success=false, data = "FeeClass not found" });
        }

        await _feeClassRepository.UpdateAsync(updatedFeeClass);
        return Ok(new{success=true,data="updated successfully"});
    }

    // DELETE: api/FeeClass/{classId}/{feeId}
    [HttpDelete("{classId:int}/{feeId:int}")]
    public async Task<IActionResult> DeleteFeeClass(int classId, int feeId)
    {
        
     var check=  await _feeClassRepository.checkIfExist(classId,feeId);
     if(!check){
        return Ok(new{success=true,data="FeeClass not found"});    
     }else{
        await _feeClassRepository.DeleteAsync(classId, feeId);
        return Ok(new{success=true,data="deleted successfully"});
     }

    }
}
