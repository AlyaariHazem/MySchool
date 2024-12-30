
using Backend.DTOS.FeeClassesDTO;
using Backend.Models;

using Backend.Repository.School.Interfaces;
using Backend.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

[Route("api/[controller]")]
[ApiController]
public class FeeClassController : ControllerBase
{
    private readonly IFeeClassServices _feeClassServices;
    protected readonly APIResponse _response;

    public FeeClassController(IFeeClassServices feeClassServices)
    {
        _feeClassServices = feeClassServices;
        _response = new();
    }

    // GET: api/FeeClass
    [HttpGet]
    public async Task<IActionResult> GetAllFeeClasses()
    {
        var Result = await _feeClassServices.GetAllAsync();
        _response.IsSuccess = true;
        _response.statusCode = System.Net.HttpStatusCode.OK;
        _response.Result = Result;
        return Ok(_response);
    }

    // GET: api/FeeClass/Fee/{feeClassID:int}
    [HttpGet("Fee/{feeClassID:int}")]
    public async Task<IActionResult> GetFeeClassById(int feeClassID)
    {
        var Result = await _feeClassServices.GetAsync(f => f.FeeClassID == feeClassID);
        if (Result != null)
        {
            _response.IsSuccess = true;
            _response.statusCode = System.Net.HttpStatusCode.OK;
            _response.Result = Result;
            return Ok(_response);
        }
        _response.IsSuccess = false;
        _response.statusCode = System.Net.HttpStatusCode.BadRequest;
        _response.ErrorMasseges.Add("Invalid FeeClass data.");
        return BadRequest(_response);
    }
    // GET: api/FeeClass/Class/{classId:int}
    [HttpGet("Class/{classId:int}")]
    public async Task<IActionResult> GetAllFeeClassById(int classId)
    {
        var Result = await _feeClassServices.GetAllAsync(f => f.ClassID == classId);
        if (Result != null)
        {
            _response.IsSuccess = true;
            _response.statusCode = System.Net.HttpStatusCode.OK;
            _response.Result = Result;
            return Ok(_response);
        }
        _response.IsSuccess = false;
        _response.statusCode = System.Net.HttpStatusCode.BadRequest;
        _response.ErrorMasseges.Add("Invalid FeeClass data.");
        return BadRequest(_response);
    }

    // POST: api/FeeClass
    [HttpPost]
    public async Task<IActionResult> AddFeeClass([FromBody] AddFeeClassDTO feeClass)
    {
        if (ModelState.IsValid)
        {
            var Result = await _feeClassServices.AddAsync(feeClass);
            if (Result)
            {
                _response.IsSuccess = true;
                _response.statusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            _response.IsSuccess = false;
            _response.statusCode = System.Net.HttpStatusCode.BadRequest;
            _response.ErrorMasseges.Add("Invalid FeeClass data.");
            return BadRequest(_response);

        }
        _response.IsSuccess = false;
        _response.statusCode = System.Net.HttpStatusCode.BadRequest;
        foreach (var modelState in ModelState.Values)
        {
            foreach (var modelError in modelState.Errors)
            {
                _response.ErrorMasseges.Add(modelError.ErrorMessage);
            }
        }
        return BadRequest(_response);
    }

    // PUT: api/FeeClass/{feeClassID}
    [HttpPut("{feeClassID:int}")]
    public async Task<IActionResult> UpdateFeeClass(int feeClassID, [FromBody] UpdateFeeClassDTO updatedFeeClass)
    {

        if (ModelState.IsValid)
        {
            var Result = await _feeClassServices.UpdateAsync(feeClassID, updatedFeeClass);
            if (Result)
            {
                _response.IsSuccess = true;
                _response.statusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            _response.IsSuccess = false;
            _response.statusCode = System.Net.HttpStatusCode.NotFound;
            _response.ErrorMasseges.Add("This FeeClass is not found.");
            return NotFound(_response);

        }
        _response.IsSuccess = false;
        _response.statusCode = System.Net.HttpStatusCode.BadRequest;
        foreach (var modelState in ModelState.Values)
        {
            foreach (var modelError in modelState.Errors)
            {
                _response.ErrorMasseges.Add(modelError.ErrorMessage);
            }
        }
        return BadRequest(_response);
    }

    // DELETE: api/FeeClass/{feeClassID}}
    [HttpDelete("{FeeClassId:int}")]
    public async Task<IActionResult> DeleteFeeClass(int FeeClassId)
    {

        var Result = await _feeClassServices.DeleteAsync(FeeClassId);
        if (Result)
        {
            _response.IsSuccess = true;
            _response.statusCode = System.Net.HttpStatusCode.OK;
            return Ok(_response);
        }
        _response.IsSuccess = false;
        _response.statusCode = System.Net.HttpStatusCode.NotFound;
        _response.ErrorMasseges.Add("This FeeClass is not found.");
        return NotFound(_response);

    }
}
