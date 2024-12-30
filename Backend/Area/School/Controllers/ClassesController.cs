using System.Collections.Generic;
using AutoMapper;
using Backend.DTOS;
using Backend.DTOS.ClassesDTO;

using Backend.Models;
using Backend.Repository;
using Backend.Repository.School;
using Backend.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClassesController : ControllerBase
    {
        protected readonly APIResponse _response;
        private readonly IClassesServices _classServices;
        private readonly IStagesServices stageRepo;

        public ClassesController(IClassesServices classRepo, IStagesServices stageRepo, IClassesServices classServices)
        {

            this.stageRepo = stageRepo;
            this._response = new();
            _classServices = classServices;
        }


        // POST api/classes
        [HttpPost]
        public async Task<ActionResult<APIResponse>> AddClass([FromBody] AddClassDTO createDTO)
        {
            if (ModelState.IsValid)
            {
                var Result = await _classServices.AddAsync(createDTO);
                if (Result)
                {
                    _response.IsSuccess = true;
                    _response.statusCode = System.Net.HttpStatusCode.OK;
                    return Ok(_response);
                }
                _response.IsSuccess = false;
                _response.statusCode = System.Net.HttpStatusCode.BadRequest;
                _response.ErrorMasseges.Add("Invalid class data.");
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

        // PUT api/classes/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateClass(int id, [FromBody] UpdateClassDTO updateClass)
        {
            if (ModelState.IsValid)
            {
                var Result = await _classServices.UpdateAsync(id, updateClass);
                if (Result)
                {
                    _response.IsSuccess = true;
                    _response.statusCode = System.Net.HttpStatusCode.OK;
                    return Ok(_response);
                }
                _response.IsSuccess = false;
                _response.statusCode = System.Net.HttpStatusCode.NotFound;
                _response.ErrorMasseges.Add("This Class is not found.");
                return NotFound(_response);

            }
            _response.IsSuccess = false;
            _response.statusCode = System.Net.HttpStatusCode.BadRequest;
            _response.ErrorMasseges.Add("Invalid class data.");
            return BadRequest(_response);

        }

        // GET api/classes
        // [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetClasses()
        {
            _response.Result = await _classServices.GetAllAsync();
            _response.IsSuccess = true;
            _response.statusCode = System.Net.HttpStatusCode.OK;
            return Ok(_response);
        }

        // GET api/classes/{id}
        [HttpGet("{id:int}")]
        public IActionResult GetClass(int id)
        {
            var Result = _classServices.GetAsync(c => c.ClassID == id);
            if (Result != null)
            {
                _response.Result = Result;
                _response.IsSuccess = true;
                _response.statusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            _response.IsSuccess = false;
            _response.statusCode = System.Net.HttpStatusCode.NotFound;
            _response.ErrorMasseges.Add("This Class is not found.");
            return NotFound(_response);
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteClass(int id)
        {
            var Result = await _classServices.DeleteAsync(id);
            if (Result)
            {
                _response.IsSuccess = true;
                _response.statusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            _response.IsSuccess = false;
            _response.statusCode = System.Net.HttpStatusCode.NotFound;
            _response.ErrorMasseges.Add("This Class is not found.");
            return NotFound(_response);
        }


        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatePartial(int id, [FromBody] JsonPatchDocument<UpdateClassDTO> patchDoc)
        {
            var Result = await _classServices.UpdatePartialAsync(id, patchDoc);
            if (Result)
            {
                _response.IsSuccess = true;
                _response.statusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            _response.IsSuccess = false;
            _response.statusCode = System.Net.HttpStatusCode.NotFound;
            _response.ErrorMasseges.Add("This Class is not found.");
            return NotFound(_response);
        }
    }
}
