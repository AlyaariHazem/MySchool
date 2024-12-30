using System.Collections.Generic;
using Backend.DTOS;
using Backend.DTOS.StagesDTO;
using Backend.Models;
using Backend.Repository;
using Backend.Repository.School;
using Backend.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School
{
    // [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StagesController : ControllerBase
    {
        private readonly IStagesServices stagesServices;
        protected readonly APIResponse _response;

        public StagesController(IStagesServices stagesServices)
        {
            this.stagesServices = stagesServices;
            _response = new APIResponse();
        }


        // POST api/stages
        [HttpPost]
        public async Task<ActionResult> Add(AddStageDTO model)
        {
            if (ModelState.IsValid)
            {
                var result = await stagesServices.AddAsync(model);
                if (result)
                {
                    _response.IsSuccess = true;
                    _response.statusCode = System.Net.HttpStatusCode.OK;
                    return Ok(_response);
                }
                _response.IsSuccess = false;
                _response.statusCode = System.Net.HttpStatusCode.BadRequest;
                _response.ErrorMasseges.Add("Invalid stage data.");
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


        // GET api/stages
        [HttpGet]
        public async Task<IActionResult> GetStages()
        {
            var Result = await stagesServices.GetAllAsync();
            _response.IsSuccess = true;
            _response.statusCode = System.Net.HttpStatusCode.OK;
            _response.Result = Result;
            return Ok(_response);
        }

        // PUT api/stages/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStage(int id, UpdateStageDTO model)
        {
            if (ModelState.IsValid)
            {
                var result = await stagesServices.UpdateAsync(model);
                if (result)
                {
                    _response.IsSuccess = true;
                    _response.statusCode = System.Net.HttpStatusCode.OK;
                    return Ok(_response);
                }
                _response.IsSuccess = false;
                _response.statusCode = System.Net.HttpStatusCode.NotFound;
                _response.ErrorMasseges.Add("This Stage is not found.");
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

        // DELETE api/stages/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStage(int id)
        {
            var result = await stagesServices.DeleteAsync(id);
            if (result)
            {
                _response.IsSuccess = true;
                _response.statusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            _response.IsSuccess = false;
            _response.statusCode = System.Net.HttpStatusCode.NotFound;
            _response.ErrorMasseges.Add("This stage is not found.");
            return NotFound(_response);
        }
        // PATCH api/stages/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatePartial(int id, [FromBody] JsonPatchDocument<UpdateStageDTO> patchDoc)
        {
            if (patchDoc != null)
            {
                var result = await stagesServices.UpdatePartialAsync(id, patchDoc);
                if (result)
                {
                    _response.IsSuccess = true;
                    _response.statusCode = System.Net.HttpStatusCode.OK;
                    return Ok(_response);
                }
                _response.IsSuccess = false;
                _response.statusCode = System.Net.HttpStatusCode.NotFound;
                _response.ErrorMasseges.Add("This Stage is not found.");
                return NotFound(_response);
            }
            _response.IsSuccess = false;
            _response.statusCode = System.Net.HttpStatusCode.BadRequest;
            _response.ErrorMasseges.Add("Invalid stage data.");
            return BadRequest(_response);
        }
    }
}
