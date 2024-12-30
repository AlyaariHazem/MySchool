using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;
using Backend.Services.IServices;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.Controllers.School;

[Route("api/[controller]")]
[ApiController]
public class GuardianController : Controller
{
    private readonly IGuardianServices _guardianServices;
    protected readonly APIResponse _response;

    public GuardianController(IGuardianServices guardianServices)
    {
        _guardianServices = guardianServices;
        _response = new();
    }
    [HttpGet]
    public async Task<IActionResult> GetAllGuardians()
    {
        var Result = await _guardianServices.GetAllAsync();
        if (Result != null)
        {
            _response.IsSuccess = true;
            _response.statusCode = System.Net.HttpStatusCode.OK;
            _response.Result = Result;
            return Ok(_response);
        }
        _response.IsSuccess = false;
        _response.statusCode = System.Net.HttpStatusCode.BadRequest;
        _response.ErrorMasseges.Add("Invalid Guardian data.");
        return BadRequest(_response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetGuardian(int id)
    {
        var Result = await _guardianServices.GetAsync(g => g.GuardianID == id);
        if (Result != null)
        {
            _response.IsSuccess = true;
            _response.statusCode = System.Net.HttpStatusCode.OK;
            _response.Result = Result;
            return Ok(_response);
        }
        _response.IsSuccess = false;
        _response.statusCode = System.Net.HttpStatusCode.NotFound;
        _response.ErrorMasseges.Add("This Guardian is not found.");
        return NotFound(_response);
    }

}
