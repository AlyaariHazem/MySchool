using System.Net;
using Backend.DTOS.School.Months;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

[Route("api/[controller]")]
[ApiController]
public class MonthController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public MonthController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MonthDTO dto)
    {
        var result = await _unitOfWork.Months.AddMonthAsync(dto);

        if (!result.Ok)
            return BadRequest(APIResponse.Fail(result.Error!));

        return CreatedAtAction(nameof(GetById),
                               new { id = dto.MonthID },
                               APIResponse.Success(dto, HttpStatusCode.Created));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _unitOfWork.Months.GetAllMonthsAsync();

        if (!result.Ok)
            return NotFound(APIResponse.Fail(result.Error!));

        return Ok(APIResponse.Success(result.Value!));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _unitOfWork.Months.GetMonthByIdAsync(id);

        return result.Ok
            ? Ok(APIResponse.Success(result.Value!))
            : NotFound(APIResponse.Fail(result.Error!));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] MonthDTO dto)
    {
        if (id != dto.MonthID)
            return BadRequest(APIResponse.Fail("Month ID mismatch."));

        var result = await _unitOfWork.Months.UpdateMonthAsync(dto);

        return result.Ok
            ? Ok(APIResponse.Success("Month updated successfully."))
            : NotFound(APIResponse.Fail(result.Error!));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _unitOfWork.Months.DeleteMonthAsync(id);

        return result.Ok
            ? Ok(APIResponse.Success("Month deleted successfully."))
            : NotFound(APIResponse.Fail(result.Error!));
    }
}
