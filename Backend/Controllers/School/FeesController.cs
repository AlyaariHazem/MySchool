using System.Net;
using Backend.DTOS.School.Fees;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FeesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public FeesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllFees()
    {
        var result = await _unitOfWork.Fees.GetAllAsync();

        if (!result.Ok)
            return StatusCode((int)HttpStatusCode.InternalServerError,
                              APIResponse.Fail(result.Error!));

        return Ok(APIResponse.Success(result.Value!));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetFeeById(int id)
    {
        var result = await _unitOfWork.Fees.GetByIdAsync(id);

        return result.Ok
            ? Ok(APIResponse.Success(result.Value!))
            : NotFound(APIResponse.Fail(result.Error!));
    }

    [HttpPost]
    public async Task<IActionResult> CreateFee([FromBody] FeeDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(APIResponse.Fail("Invalid fee data."));

        var result = await _unitOfWork.Fees.AddAsync(dto);

        return result.Ok
            ? StatusCode((int)HttpStatusCode.Created,
                         APIResponse.Success("Fee created successfully.", HttpStatusCode.Created))
            : StatusCode((int)HttpStatusCode.InternalServerError,
                         APIResponse.Fail(result.Error!));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateFee(int id, [FromBody] FeeDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(APIResponse.Fail("Invalid fee data."));

        if (id != dto.FeeID)
            return BadRequest(APIResponse.Fail("Fee ID mismatch."));

        var result = await _unitOfWork.Fees.UpdateAsync(dto);

        return result.Ok
            ? Ok(APIResponse.Success("Fee updated successfully."))
            : NotFound(APIResponse.Fail(result.Error!));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteFee(int id)
    {
        var result = await _unitOfWork.Fees.DeleteAsync(id);

        return result.Ok
            ? Ok(APIResponse.Success("Fee deleted successfully."))
            : NotFound(APIResponse.Fail(result.Error!));
    }
    
    [HttpPatch("{feeID:int}")]
    public async Task<IActionResult> ChangeStateFee(int feeID, [FromBody] JsonPatchDocument<ChangeStateFeeDTO> dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(APIResponse.Fail("Invalid Fee data."));

        var result = await _unitOfWork.Fees.UpdatePartial(feeID, dto);

        return result.Ok
            ? Ok(APIResponse.Success("Fee updated successfully."))
            : NotFound(APIResponse.Fail(result.Error!));
    }
}
