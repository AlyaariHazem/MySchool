using System.Net;
using Backend.DTOS.School.FeeClass;
using Backend.DTOS.School.Fees;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

[Route("api/[controller]")]
[ApiController]
public class FeeClassController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public FeeClassController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /*----------------------------------------------------
     *                    GET  /api/FeeClass
     *---------------------------------------------------*/
    [HttpGet]
    public async Task<IActionResult> GetAllFeeClasses()
    {
        var result = await _unitOfWork.FeeClasses.GetAllAsync();

        return result.Ok
            ? Ok(APIResponse.Success(result.Value!))
            : StatusCode((int)HttpStatusCode.InternalServerError,
                         APIResponse.Fail(result.Error!));
    }

    /*----------------------------------------------------
     *       GET  /api/FeeClass/Fee/{feeClassID}
     *---------------------------------------------------*/
    [HttpGet("Fee/{feeClassID:int}")]
    public async Task<IActionResult> GetFeeClassById(int feeClassID)
    {
        var result = await _unitOfWork.FeeClasses.GetByIdAsync(feeClassID);

        return result.Ok
            ? Ok(APIResponse.Success(result.Value!))
            : NotFound(APIResponse.Fail(result.Error!));
    }

    /*----------------------------------------------------
     *     GET  /api/FeeClass/Class/{classId}
     *---------------------------------------------------*/
    [HttpGet("Class/{classId:int}")]
    public async Task<IActionResult> GetAllFeeClassByClassId(int classId)
    {
        var result = await _unitOfWork.FeeClasses.GetAllByClassIdAsync(classId);

        return result.Ok
            ? Ok(APIResponse.Success(result.Value!))
            : NotFound(APIResponse.Fail(result.Error!));
    }

    /*----------------------------------------------------
     *                POST  /api/FeeClass
     *---------------------------------------------------*/
    [HttpPost]
    public async Task<IActionResult> AddFeeClass([FromBody] AddFeeClassDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(APIResponse.Fail("Invalid FeeClass data."));

        var result = await _unitOfWork.FeeClasses.AddAsync(dto);

        return result.Ok
            ? StatusCode((int)HttpStatusCode.Created,
                         APIResponse.Success("FeeClass created successfully.",
                                             HttpStatusCode.Created))
            : StatusCode((int)HttpStatusCode.InternalServerError,
                         APIResponse.Fail(result.Error!));
    }

    /*----------------------------------------------------
     *        PUT  /api/FeeClass/{feeClassID}
     *---------------------------------------------------*/
    [HttpPut("{feeClassID:int}")]
    public async Task<IActionResult> UpdateFeeClass(int feeClassID,
                                                    [FromBody] AddFeeClassDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(APIResponse.Fail("Invalid FeeClass data."));

        var result = await _unitOfWork.FeeClasses.UpdateAsync(feeClassID, dto);

        return result.Ok
            ? Ok(APIResponse.Success("FeeClass updated successfully."))
            : NotFound(APIResponse.Fail(result.Error!));
    }

    /*----------------------------------------------------
     *       DELETE  /api/FeeClass/{feeClassID}
     *---------------------------------------------------*/
    [HttpDelete("{feeClassID:int}")]
    public async Task<IActionResult> DeleteFeeClass(int feeClassID)
    {
        var result = await _unitOfWork.FeeClasses.DeleteAsync(feeClassID);

        return result.Ok
            ? Ok(APIResponse.Success("FeeClass deleted successfully."))
            : NotFound(APIResponse.Fail(result.Error!));
    }

    [HttpPatch("{feeClassID:int}")]
    public async Task<IActionResult> ChangeStateFeeClass(int feeClassID, [FromBody] JsonPatchDocument<ChangeStateFeeClassDTO> dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(APIResponse.Fail("Invalid FeeClass data."));

        var result = await _unitOfWork.FeeClasses.UpdatePartial(feeClassID, dto);

        return result.Ok
            ? Ok(APIResponse.Success("FeeClass updated successfully."))
            : NotFound(APIResponse.Fail(result.Error!));
    }
}