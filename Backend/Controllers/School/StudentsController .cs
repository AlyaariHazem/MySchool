using Backend.DTOS;
using Backend.DTOS.School.Accounts;
using Backend.DTOS.School.Guardians;
using Backend.DTOS.School.StudentClassFee;
using Backend.DTOS.School.Students;
using Backend.Interfaces;
using Backend.Models;
using Backend.Repository.School.Classes;
using Backend.Repository.School.Implements;
using Backend.Repository.School.Interfaces;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly StudentManagementService _studentManagementService;
        private readonly mangeFilesService _mangeFilesService;
        private readonly IUnitOfWork _unitOfWork;

        public StudentsController(StudentManagementService studentManagementService,
        IUnitOfWork unitOfWork, mangeFilesService mangeFilesService)
        {
            _studentManagementService = studentManagementService;
            _unitOfWork = unitOfWork;
            _mangeFilesService = mangeFilesService;
        }

        [HttpPost]
        public async Task<IActionResult> AddStudentWithGuardian([FromBody] AddStudentWithGuardianRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                GuardianDTO existingGuardian = null!;

                // Step 1: Check if we are adding a student to an existing guardian.
                if (request.ExistingGuardianId.HasValue && !request.ExistingGuardianId.Value.Equals(0))
                {
                    existingGuardian = await _unitOfWork.Guardians.GetGuardianByIdAsync(request.ExistingGuardianId.Value);
                    if (existingGuardian == null)
                        return NotFound(new { message = "Existing Guardian not found." });
                }

                // If no existing guardian is provided, create a new guardian and guardian user
                ApplicationUser guardianUser = null!;
                Guardian guardian = null!;
                AccountsDTO account = null!;

                if (existingGuardian == null)
                {
                    var userName = "Guardain_" + Guid.NewGuid().ToString("N").Substring(0, 5);
                    guardianUser = new ApplicationUser
                    {
                        UserName = userName,
                        Email = request.GuardianEmail,
                        Address = request.GuardianAddress,
                        Gender = request.GuardianGender,
                        PhoneNumber = request.GuardianPhone,
                        UserType = "Guardian"
                    };

                    guardian = new Guardian
                    {
                        FullName = request.GuardianFullName,
                        Type = request.GuardianType,
                        GuardianDOB = request.GuardianDOB
                    };
                    // Add Account
                    account = new AccountsDTO
                    {
                        AccountName = request.GuardianFullName,
                        Note = "",
                        State = true,
                        TypeAccountID = 1
                    };
                }
                else
                {
                    // We already have a guardian, so no new guardian user or guardian entity needed
                    // If the guardian was created previously, its user should also already exist.
                }

                var userNameStudent = "St_" + Guid.NewGuid().ToString("N").Substring(0, 5);

                // Create Student User
                var studentUser = new ApplicationUser
                {
                    UserName = userNameStudent,
                    Email = request.StudentEmail,
                    Address = request.StudentAddress,
                    Gender = request.StudentGender,
                    HireDate = request.HireDate,
                    PhoneNumber = request.StudentPhone,
                    UserType = "Student"
                };

                // Create Student Entity
                var student = new Student
                {
                    StudentID = request.StudentID,
                    FullName = new Name
                    {
                        FirstName = request.StudentFirstName,
                        MiddleName = request.StudentMiddleName!,
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
                    PlaceBirth = request.PlaceBirth,
                    StudentDOB = request.StudentDOB,
                    ImageURL = $"StudentPhotos_{request.StudentID}_{request.StudentImageURL}",
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
                            StudentID = request.StudentID,
                            AttachmentURL = $"Attachments_{request.StudentID}_{fileUrl}"
                        });
                    }
                }

                // Map FeeClass data to StudentClassFees
                var studentClassFees = new List<StudentClassFeeDTO>();
                if (request.Discounts != null && request.Discounts.Any())
                {
                    foreach (var discount in request.Discounts)
                        studentClassFees.Add(new StudentClassFeeDTO
                        {
                            StudentID = request.StudentID,
                            FeeClassID = discount.FeeClassID,
                            AmountDiscount = discount.AmountDiscount,
                            NoteDiscount = discount.NoteDiscount,
                            Mandatory = discount.Mandatory
                        });
                }

                Student createdStudent;
                if (existingGuardian == null)
                {
                    createdStudent = await _studentManagementService.AddStudentWithGuardianAsync(
                        guardianUser!, request.GuardianPassword, guardian!,
                        studentUser, request.StudentPassword, student,
                        account!, accountStudentGuardian, attachments, studentClassFees);
                }
                else
                {
                    //if guardian exist
                    createdStudent = await _studentManagementService.AddStudentToExistingGuardianAsync(
                        existingGuardian,
                        studentUser, request.StudentPassword, student,
                        attachments, studentClassFees, accountStudentGuardian);
                }

                return Ok(new { success = true, message = "Student added successfully.", student = createdStudent });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        [HttpPut("updateStudentWithGuardian/{id}")]
        public async Task<IActionResult> UpdateStudentWithGuardian(int id, [FromBody] UpdateStudentWithGuardianRequestDTO request)
        {
            if (id != request.StudentID)
            {
                return BadRequest("Student ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {

                var updatedStudent = await _studentManagementService.UpdateStudentWithGuardianAsync(id, request);
                if (updatedStudent == null)
                {
                    return NotFound(new { message = "Student or Guardian not found." });
                }

                return Ok(new { success = true, message = "Student updated successfully.", student = updatedStudent });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _unitOfWork.Students.GetAllStudentsAsync();
            if (students == null)
                return NotFound(new { message = "Students not found." });

            return Ok(students);
        }

        [HttpGet("MaxValue")]
        public async Task<IActionResult> GetMaxValue()
        {
            var students = await _unitOfWork.Students.MaxValue();

            return Ok(students);
        }

        // DELETE: api/Students/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteStudent([FromRoute] int id)
        {
            var isDeleted = await _unitOfWork.Students.DeleteStudentAsync(id);

            if (isDeleted)
            {
                return NoContent(); // 204 No Content
            }
            else
            {
                return NotFound(new { message = $"Student with ID {id} not found." });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentDataAsRequest(int id)
        {
            var requestData = await _unitOfWork.Students.GetUpdateStudentWithGuardianRequestData(id);

            if (requestData == null)
            {
                return NotFound(new { message = "Student not found." });
            }

            return Ok(requestData);
        }

        [HttpPost("uploadFiles")]
        public async Task<IActionResult> UploadAttachments([FromForm] List<IFormFile> files, [FromForm] int studentId)
        {
            if (files == null || !files.Any())
                return BadRequest("No files uploaded.");

            try
            {
                var filePaths = await _mangeFilesService.UploadAttachments(files,"Attachments", studentId);
                return Ok(new { success = true, filePaths });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        [HttpPost("uploadImage")]
        public async Task<IActionResult> UploadStudentImage([FromForm] IFormFile file, [FromForm] int studentId)
        {
            if (file == null)
                return BadRequest("No files uploaded.");

            try
            {
                var filePaths = await _mangeFilesService.UploadImage(file,"StudentPhotos", studentId);
                return Ok(new { success = true, filePaths });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}