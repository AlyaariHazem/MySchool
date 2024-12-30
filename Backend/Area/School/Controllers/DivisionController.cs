using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Backend.Repository;
using Backend.Repository.School;
using Backend.DTOS;
using Backend.Models;
using Microsoft.AspNetCore.JsonPatch;
using Backend.Repository.IRepository;
using Backend.Services.IServices;
using Backend.DTOS.DivisionsDTO;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    public class DivisionsController : ControllerBase
    {
        protected readonly APIResponse _response;
        private readonly IDivisionServices _divisionServices;

        private readonly IClassesServices _classesServices;
        public DivisionsController(IClassesServices classesServices, IDivisionServices divisionServices)
        {
            _classesServices = classesServices;
            _divisionServices = divisionServices;
            this._response = new();
        }

        // POST api/divisions
        [HttpPost]
        public async Task<ActionResult<APIResponse>> AddDivision(AddDivisionDTO division)
        {
            if (ModelState.IsValid)
            {
                var Result = await _divisionServices.AddAsync(division);
                if (Result)
                {
                    _response.IsSuccess = true;
                    _response.statusCode = System.Net.HttpStatusCode.OK;
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


        // PUT api/divisions/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDivision(int id, UpdateDivisionDTO division)
        {
            if (ModelState.IsValid)
            {
                var Result = await _divisionServices.UpdateAsync(division);
                if (Result)
                {
                    _response.IsSuccess = true;
                    _response.statusCode = System.Net.HttpStatusCode.OK;
                    return Ok(_response);
                }
                _response.IsSuccess = false;
                _response.statusCode = System.Net.HttpStatusCode.NotFound;
                _response.ErrorMasseges.Add("This Division is not found.");
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


        // GET api/divisions
        [HttpGet]
        public async Task<IActionResult> GetDivisions()
        {
            var divisions = await _divisionServices.GetAllAsync();
            if (divisions == null)
            {
                _response.IsSuccess = false;
                _response.statusCode = System.Net.HttpStatusCode.NotFound;
                _response.ErrorMasseges.Add("No divisions found.");
                return NotFound(_response);
            }
            _response.Result = divisions;
            _response.IsSuccess = true;
            _response.statusCode = System.Net.HttpStatusCode.OK;
            return Ok(_response);
        }

        // DELETE api/divisions/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDivision(int id)
        {
            var Result = await _divisionServices.DeleteAsync(id);
            if (Result)
            {
                _response.IsSuccess = true;
                _response.statusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            _response.IsSuccess = false;
            _response.statusCode = System.Net.HttpStatusCode.NotFound;
            _response.ErrorMasseges.Add("This Division is not found.");
            return NotFound(_response);
        }
        //Patch api/divisions/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchDivision(int id, [FromBody] JsonPatchDocument<UpdateDivisionDTO> patchDoc)
        {
            var Result = await _divisionServices.UpdatePartialAsync(id, patchDoc);
            if (Result)
            {
                _response.IsSuccess = true;
                _response.statusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            _response.IsSuccess = false;
            _response.statusCode = System.Net.HttpStatusCode.NotFound;
            _response.ErrorMasseges.Add("This Division is not found.");
            return NotFound(_response);

        }
    }
}
