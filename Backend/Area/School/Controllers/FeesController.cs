using Backend.DTOS.FeesDTO;
using Backend.Models;
using Backend.Repository.School.Interfaces;
using Backend.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FeesController : ControllerBase
{
    private readonly IFeesServices _feesServices;
    protected readonly APIResponse _response;

    public FeesController(IFeesServices feesServices)
    {
        _feesServices = feesServices;
        _response = new APIResponse();
    }


    [HttpGet]
    public async Task<IActionResult> GetAllFees()
    {
        var Result = await _feesServices.GetAllAsync();
        if (Result != null)
        {
            _response.IsSuccess = true;
            _response.statusCode = System.Net.HttpStatusCode.OK;
            _response.Result = Result;
            return Ok(_response);
        }
        _response.IsSuccess = false;
        _response.statusCode = System.Net.HttpStatusCode.BadRequest;
        _response.ErrorMasseges.Add("Invalid Fee data.");
        return BadRequest(_response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetFee(int id)
    {
        var Result = await _feesServices.GetAsync(f => f.FeeID == id);
        if (Result != null)
        {
            _response.IsSuccess = true;
            _response.statusCode = System.Net.HttpStatusCode.OK;
            _response.Result = Result;
            return Ok(_response);
        }
        _response.IsSuccess = false;
        _response.statusCode = System.Net.HttpStatusCode.BadRequest;
        _response.ErrorMasseges.Add("Invalid Fee data.");
        return BadRequest(_response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateFee([FromBody] AddFeeDTO fee)
    {
        if (ModelState.IsValid)
        {
            var Result = await _feesServices.AddAsync(fee);
            if (Result)
            {
                _response.IsSuccess = true;
                _response.statusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            _response.IsSuccess = false;
            _response.statusCode = System.Net.HttpStatusCode.BadRequest;
            _response.ErrorMasseges.Add("Invalid fee data.");
            return BadRequest(_response);
        }
        foreach (var modelState in ModelState.Values)
        {
            foreach (var modelError in modelState.Errors)
            {
                _response.ErrorMasseges.Add(modelError.ErrorMessage);
            }
        }
        _response.IsSuccess = false;
        _response.statusCode = System.Net.HttpStatusCode.BadRequest;
        return BadRequest(_response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFee(int id, [FromBody] UpdateFeeDTO fee)
    {
        if (ModelState.IsValid)
        {
            var Result = await _feesServices.UpdateAsync(fee);
            if (Result)
            {
                _response.IsSuccess = true;
                _response.statusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            _response.IsSuccess = false;
            _response.statusCode = System.Net.HttpStatusCode.BadRequest;
            _response.ErrorMasseges.Add("Invalid fee data.");
            return BadRequest(_response);
        }
        foreach (var modelState in ModelState.Values)
        {
            foreach (var modelError in modelState.Errors)
            {
                _response.ErrorMasseges.Add(modelError.ErrorMessage);
            }
        }
        _response.IsSuccess = false;
        _response.statusCode = System.Net.HttpStatusCode.BadRequest;
        return BadRequest(_response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFee(int id)
    {
        var Result = await _feesServices.DeleteAsync(id);
        if (Result)
        {
            _response.IsSuccess = true;
            _response.statusCode = System.Net.HttpStatusCode.OK;
            return Ok(_response);
        }
        _response.IsSuccess = false;
        _response.statusCode = System.Net.HttpStatusCode.BadRequest;
        _response.ErrorMasseges.Add("Invalid fee data.");
        return BadRequest(_response);
    }
}
