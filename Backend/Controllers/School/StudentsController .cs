using Backend.DTOS;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly StudentManagementService _studentManagementService;

        public StudentsController(StudentManagementService studentManagementService)
        {
            _studentManagementService = studentManagementService;
        }

        [HttpPost("AddStudentWithGuardian")]
        public async Task<IActionResult> AddStudentWithGuardian([FromBody] AddStudentWithGuardianRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Create Guardian User
                var guardianUser = new ApplicationUser
                {
                    UserName = request.GuardianUserName,
                    Email = request.GuardianEmail,
                    Address = request.GuardianAddress,
                    Gender = request.GuardianGender,
                    UserType = "Guardian"
                };

                // Create Guardian Entity
                var guardian = new Guardian
                {
                    FullName = request.GuardianFullName,
                    Type = request.GuardianType
                };

                // Create Student User
                var studentUser = new ApplicationUser
                {
                    UserName = request.StudentUserName,
                    Email = request.StudentEmail,
                    Address = request.StudentAddress,
                    Gender = request.StudentGender,
                    UserType = "Student"
                };

                // Create Student Entity
                var student = new Student
                {
                    FullName = new Name
                    {
                        FirstName = request.StudentFirstName,
                        MiddleName = request.StudentMiddleName,
                        LastName = request.StudentLastName
                    },
                    FullNameAlis = string.IsNullOrWhiteSpace(request.StudentFirstNameEng)
                        ? null
                        : new NameAlis
                        {
                            FirstNameEng = request.StudentFirstNameEng,
                            MiddleNameEng = request.StudentMiddleNameEng,
                            LastNameEng = request.StudentLastNameEng
                        },
                    DivisionID = request.DivisionID,
                    PlaceBirth = request.PlaceBirth
                };

                // Add Student and Guardian
                var createdStudent = await _studentManagementService.AddStudentWithGuardianAsync(
                    guardianUser, request.GuardianPassword, guardian,
                    studentUser, request.StudentPassword, student);

                return CreatedAtAction(nameof(GetStudentById), new { id = createdStudent.StudentID }, createdStudent);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            var student = await _studentManagementService.GetStudentByIdAsync(id);
            if (student == null)
                return NotFound(new { message = "Student not found." });

            return Ok(student);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _studentManagementService.GetAllStudentsAsync();
            if (students == null)
                return NotFound(new { message = "Students not found." });

            return Ok(students);
        }
    }
}
