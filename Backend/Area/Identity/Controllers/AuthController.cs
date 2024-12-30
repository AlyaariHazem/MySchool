using Backend.DTOS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Models;
using Backend.DTOS.IdentityDTO;
using Backend.Services.IServices;

namespace WebAPIDotNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServices _accountServices;
        protected readonly APIResponse _response;

        public AuthController(IAuthServices accountServices)
        {
            _accountServices = accountServices;
            _response = new();
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTO register)
        {
            if (ModelState.IsValid)
            {
                var Result = await _accountServices.Register(register);
                if (Result != null)
                {
                    _response.IsSuccess = true;
                    _response.statusCode = System.Net.HttpStatusCode.OK;
                    _response.Result = Result;
                    return Ok(_response);
                }
                _response.IsSuccess = false;
                _response.statusCode = System.Net.HttpStatusCode.BadRequest;
                _response.ErrorMasseges.Add("User is already.");
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

        [HttpPost("Login")]//Post api/Account/login
        public async Task<IActionResult> Login(LoginDTO login)
        {
            if (ModelState.IsValid)
            {
                var Result = await _accountServices.Login(login);
                if (Result != null)
                {
                    _response.IsSuccess = true;
                    _response.statusCode = System.Net.HttpStatusCode.OK;
                    _response.Result = Result;
                    return Ok(_response);
                }
                _response.IsSuccess = false;
                _response.statusCode = System.Net.HttpStatusCode.BadRequest;
                _response.ErrorMasseges.Add("Invalid division data.");
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
    }
}
