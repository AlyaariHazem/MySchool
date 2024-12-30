using Backend.DTOS;
using Backend.DTOS.RegisterStudentsDTO;

using Backend.DTOS.StudentClassFeesDTO;
using Backend.DTOS.StudentsDTO;
using Backend.Models;

using Backend.Repository.School.Interfaces;
using Backend.Services;
using Backend.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {



        private readonly IStudentServices _studentServices;
        protected readonly APIResponse _response;
        public StudentsController(IStudentServices studentServices)
        {
            _studentServices = studentServices;


            _response = new();
        }

        [HttpPost("AddStudentWithGuardian")]
        public async Task<IActionResult> AddStudentWithGuardian([FromBody] RegisterStudentWithGuardianDTO studentDTO)
        {
            if (ModelState.IsValid)
            {
                var Result = await _studentServices.RegisterWithGuardianAsync(studentDTO);
                if (Result.IsSuccess)
                {
                    _response.IsSuccess = true;
                    _response.statusCode = System.Net.HttpStatusCode.OK;
                    _response.Result = Result.StudentId;
                    return Ok(_response);
                }
                _response.IsSuccess = false;
                _response.statusCode = System.Net.HttpStatusCode.BadRequest;
                _response.ErrorMasseges.Add(Result.Error);
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
        [HttpPost("AddStudent")]
        public async Task<IActionResult> AddStudent([FromBody] RegisterStudentDTO studentDTO)
        {

            if (ModelState.IsValid)
            {
                var Result = await _studentServices.RegisterAsync(studentDTO);
                if (Result.IsSuccess)
                {
                    _response.IsSuccess = true;
                    _response.statusCode = System.Net.HttpStatusCode.OK;
                    _response.Result = Result.StudentId;
                    return Ok(_response);
                }
                _response.IsSuccess = false;
                _response.statusCode = System.Net.HttpStatusCode.BadRequest;
                _response.ErrorMasseges.Add(Result.Error);
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


        [HttpGet]
        public async Task<IActionResult> GetAllStudents()
        {
            var Result = await _studentServices.GetAllWithDetailsAsync();
            _response.IsSuccess = true;
            _response.statusCode = System.Net.HttpStatusCode.OK;
            _response.Result = Result;
            return Ok(_response);
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteStudent([FromRoute] int id)
        {
            var Result = await _studentServices.DeleteAsync(id);
            if (Result)
            {
                _response.IsSuccess = true;
                _response.statusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            _response.IsSuccess = false;
            _response.statusCode = System.Net.HttpStatusCode.NotFound;
            _response.ErrorMasseges.Add("This Student is not found.");
            return NotFound(_response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentDataAsRequest(int id)
        {
            var Result = await _studentServices.GetWithDetailsAsync(s => s.StudentID == id);
            if (Result == null)
            {
                _response.IsSuccess = false;
                _response.statusCode = System.Net.HttpStatusCode.NotFound;
                _response.ErrorMasseges.Add("This Student is not found.");
                return NotFound(_response);
            }
            _response.IsSuccess = true;
            _response.statusCode = System.Net.HttpStatusCode.OK;
            return Ok(_response);
        }


    }
}