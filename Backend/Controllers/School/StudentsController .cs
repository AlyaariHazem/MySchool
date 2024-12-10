using Backend.DTOS;
using Backend.Models;
using Backend.Repository.School.Classes;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly StudentManagementService _studentManagementService;
        private readonly StudentClassFeesRepository _studentClassFeesRepository;

        public StudentsController(StudentManagementService studentManagementService, StudentClassFeesRepository studentClassFeesRepository)
        {
            _studentManagementService = studentManagementService;
            _studentClassFeesRepository = studentClassFeesRepository;
        }

        [HttpPost]
        public async Task<IActionResult> AddStudentWithGuardian([FromBody] AddStudentWithGuardianRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userName ="Guardain_"+ Guid.NewGuid().ToString("N").Substring(0, 5);
            try
            {
                // Create Guardian User
                var guardianUser = new ApplicationUser
                {
                    UserName = userName,
                    Email = request.GuardianEmail,
                    Address = request.GuardianAddress,
                    Gender = request.GuardianGender,
                    HireDate = request.GuardianDOB,
                    PhoneNumber=request.GuardianPhone,
                    UserType = "Guardian"
                };

                // Create Guardian Entity
                var guardian = new Guardian
                {
                    FullName = request.GuardianFullName,
                    Type = request.GuardianType
                };
            var userNameStudent ="Student_"+ Guid.NewGuid().ToString("N").Substring(0, 5);
                // Create Student User
                var studentUser = new ApplicationUser
                {
                    UserName = userNameStudent,
                    Email = request.StudentEmail,
                    Address = request.StudentAddress,
                    Gender = request.StudentGender,
                    HireDate=request.StudentDOB,
                    PhoneNumber=request.StudentPhone,
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
                //Add Account
                 var account = new Accounts
                    {
                        Note = "",
                        State=true,
                        TypeAccountID = 1
                    };

                 var accountStudentGuardian = new AccountStudentGuardian
                    {
                        Amount = request.Amount
                    };

                    // Process attachments
                    var attachments = new List<Attachments>();
                    if (request.Attachments != null && request.Attachments.Any())
                    {
                        foreach (var fileUrl in request.Attachments)
                        {
                            attachments.Add(new Attachments
                            {
                                AttachmentURL = fileUrl,
                                Note = ""
                            });
                        }
                    }

                    // Map FeeClass data to StudentClassFees and assign to the student
                    var studentClassFees = new List<DisCount>();
                    if(request.Discounts != null && request.Discounts.Any())
                        foreach(var discount in request.Discounts)
                            studentClassFees.Add(new DisCount{
                                ClassID = discount.ClassID,
                                FeeID = discount.FeeID,
                                AmountDiscount = discount.AmountDiscount,
                                NoteDiscount = discount.NoteDiscount
                            });

                // Add Student and Guardian
                var createdStudent = await _studentManagementService.AddStudentWithGuardianAsync(
                    guardianUser, request.GuardianPassword, guardian,
                    studentUser, request.StudentPassword, student,
                    account, accountStudentGuardian, attachments, studentClassFees);
                // Fetch all fees for the selected class
                var feeClasses = await _studentClassFeesRepository.GetFeesForClassAsync(request.ClassID);

                return Ok(new { success = true, message = "Student deleted successfully." });
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
