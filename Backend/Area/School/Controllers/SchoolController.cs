using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.DTOS.SchoolsDTO;
using Backend.Models;
using Backend.Services.IServices;

using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolController : ControllerBase
    {
        private readonly ISchoolServices _schoolServices;
        protected readonly APIResponse _response;

        public SchoolController(ISchoolServices schoolServices)
        {
            _schoolServices = schoolServices;
            _response = new APIResponse();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSchools()
        {
            var Result = await _schoolServices.GetAllAsync();
            if (Result != null)
            {
                _response.IsSuccess = true;
                _response.statusCode = System.Net.HttpStatusCode.OK;
                _response.Result = Result;
                return Ok(_response);
            }
            _response.IsSuccess = false;
            _response.statusCode = System.Net.HttpStatusCode.BadRequest;
            _response.ErrorMasseges.Add("Invalid School data.");
            return BadRequest(_response);
        }

        [HttpPost]
        public async Task<IActionResult> AddSchool([FromBody] SchoolDTO schoolDTO)
        {
            if (ModelState.IsValid)
            {
                var Result = await _schoolServices.AddAsync(schoolDTO);
                if (Result)
                {
                    _response.IsSuccess = true;
                    _response.statusCode = System.Net.HttpStatusCode.OK;
                    return Ok(_response);
                }

                _response.IsSuccess = false;
                _response.statusCode = System.Net.HttpStatusCode.BadRequest;
                _response.ErrorMasseges.Add("Invalid school data.");
                return BadRequest(_response);
            }
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    _response.ErrorMasseges.Add(error.ErrorMessage);
                }
            }
            _response.IsSuccess = false;
            _response.statusCode = System.Net.HttpStatusCode.BadRequest;
            return BadRequest(_response);

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchool(int id, [FromBody] SchoolDTO schoolDTO)
        {
            if (ModelState.IsValid)
            {
                var Result = await _schoolServices.UpdateAsync(schoolDTO);
                if (Result)
                {
                    _response.IsSuccess = true;
                    _response.statusCode = System.Net.HttpStatusCode.OK;
                    return Ok(_response);
                }

                _response.IsSuccess = false;
                _response.statusCode = System.Net.HttpStatusCode.NotFound;
                _response.ErrorMasseges.Add("This School is not found.");
                return NotFound(_response);
            }
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    _response.ErrorMasseges.Add(error.ErrorMessage);
                }
            }
            _response.IsSuccess = false;
            _response.statusCode = System.Net.HttpStatusCode.BadRequest;
            return BadRequest(_response);

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchool(int id)
        {
            var Result = await _schoolServices.DeleteAsync(id);
            if (Result)
            {
                _response.IsSuccess = true;
                _response.statusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            _response.IsSuccess = false;
            _response.statusCode = System.Net.HttpStatusCode.NotFound;
            _response.ErrorMasseges.Add("This School is not found.");
            return NotFound(_response);
        }
    }
}
