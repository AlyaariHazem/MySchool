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

    // GET: api/FeeClass/Fee/{feeClassID:int}
    [HttpGet("Fee/{feeClassID:int}")]
    public async Task<IActionResult> GetFeeClassById(int feeClassID)
    {
        var feeClass = await _feeClassRepository.GetByIdAsync(feeClassID);
        if (feeClass == null)
        {
            return NotFound(new {success = false, data = "FeeClass not found" });
        }

        return Ok(new { success = true, data = feeClass });
    }
    // GET: api/FeeClass/Class/{classId:int}
    [HttpGet("Class/{classId:int}")]
    public async Task<IActionResult> GetAllFeeClassById(int classId)
    {
        var feeClass = await _feeClassRepository.GetAllByClassIdAsync(classId);
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

    // PUT: api/FeeClass/{feeClassID}
    [HttpPut("{feeClassID:int}")]
    public async Task<IActionResult> UpdateFeeClass(int feeClassID, [FromBody] AddFeeClassDTO updatedFeeClass)
    {
       
        var existingFeeClass = await _feeClassRepository.GetByIdAsync(feeClassID);
        if (existingFeeClass == null)
        {
            return NotFound(new {success=false, data = "FeeClass not found" });
        }

        await _feeClassRepository.UpdateAsync(feeClassID,updatedFeeClass);
        return Ok(new{success=true,data="updated successfully"});
    }

    // DELETE: api/FeeClass/{feeClassID}}
    [HttpDelete("{FeeClassId:int}")]
    public async Task<IActionResult> DeleteFeeClass(int FeeClassId)
    {
        
     var check=  await _feeClassRepository.checkIfExist(FeeClassId);
     if(!check){
        return Ok(new{success=true,data="FeeClass not found"});    
     }else{
        await _feeClassRepository.DeleteAsync(FeeClassId);
        return Ok(new{success=true,data="deleted successfully"});
     }

    }
}
