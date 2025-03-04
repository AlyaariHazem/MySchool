using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountRepository _accountRepo;
    public AccountsController(IAccountRepository accountRepo)
    {
        _accountRepo = accountRepo;
    }

    [HttpGet]
    public async Task<ActionResult<List<Accounts>>> GetAllAccounts()
    {
        var response = new APIResponse();
        try
        {
            var accounts = await _accountRepo.GetAllAccounts();
            response.Result = accounts;
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (System.Exception ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.InternalServerError;
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }
}
