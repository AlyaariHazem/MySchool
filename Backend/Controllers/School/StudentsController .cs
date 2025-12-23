using Backend.Common;
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace Backend.Controllers
{
    [Authorize(Roles = "ADMIN,MANAGER")]
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
                bool isUsingExistingGuardian = false;

                // Step 1: Check if we are adding a student to an existing guardian.
                // IMPORTANT: If ExistingGuardianId is provided and valid, we MUST NOT create a new guardian
                if (request.ExistingGuardianId.HasValue && request.ExistingGuardianId.Value > 0)
                {
                    existingGuardian = await _unitOfWork.Guardians.GetGuardianByIdAsync(request.ExistingGuardianId.Value);
                    if (existingGuardian == null)
                        return NotFound(new { message = $"Existing Guardian with ID {request.ExistingGuardianId.Value} not found." });
                    
                    isUsingExistingGuardian = true; // Mark that we're using an existing guardian
                }

                // If no existing guardian is provided (null or 0), create a new guardian and guardian user
                ApplicationUser guardianUser = null!;
                Guardian guardian = null!;
                AccountsDTO account = null!;

                // Only create a new guardian if no existing guardian was found/selected
                if (!isUsingExistingGuardian && existingGuardian == null)
                {
                    // Validate guardian information is provided when creating a new guardian
                    if (string.IsNullOrWhiteSpace(request.GuardianEmail) || 
                        string.IsNullOrWhiteSpace(request.GuardianFullName))
                    {
                        return BadRequest(new { message = "Guardian email and full name are required when creating a new guardian." });
                    }

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
                    // Add Account - only create account when creating a new guardian
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
                    // IMPORTANT: When using an existing guardian:
                    // - DO NOT create a new guardian user
                    // - DO NOT create a new guardian entity
                    // - DO NOT create a new account
                    // The existing guardian already has an account that will be reused
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


                // Create AccountStudentGuardian object with the amount from the request
                // This will be used to link the student to the guardian's account
                var accountStudentGuardian = new AccountStudentGuardian
                {
                    Amount = request.Amount // Ensure amount is set from the request
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
                
                // IMPORTANT: Use isUsingExistingGuardian flag to ensure correct method is called
                if (isUsingExistingGuardian && existingGuardian != null)
                {
                    // IMPORTANT: When existing guardian is selected, DO NOT create:
                    // - New Guardian User (AspNetUsers record)
                    // - New Guardian (Guardians table record)
                    // - New Account (Accounts table record)
                    // Only create the student and link to existing guardian's account
                    
                    // Validate that amount is provided
                    if (request.Amount < 0)
                    {
                        return BadRequest(new { message = "Amount must be a valid non-negative number." });
                    }

                    // Ensure guardian-related objects are null when using existing guardian
                    if (guardianUser != null || guardian != null || account != null)
                    {
                        return BadRequest(new { message = "Guardian-related data should not be provided when using an existing guardian." });
                    }

                    createdStudent = await _studentManagementService.AddStudentToExistingGuardianAsync(
                        existingGuardian,
                        studentUser, request.StudentPassword, student,
                        attachments, studentClassFees, accountStudentGuardian);
                }
                else
                {
                    // Only create new guardian, guardian user, and account when no existing guardian is selected
                    if (guardianUser == null || guardian == null || account == null)
                        return BadRequest(new { message = "Guardian information is required when creating a new guardian." });

                    createdStudent = await _studentManagementService.AddStudentWithGuardianAsync(
                        guardianUser, request.GuardianPassword, guardian,
                        studentUser, request.StudentPassword, student,
                        account, accountStudentGuardian, attachments, studentClassFees);
                }

                return Ok(new { success = true, message = "Student added successfully.", student = createdStudent });
            }
            catch (Exception ex)
            {
                // Log the full exception details
                var errorDetails = new
                {
                    message = ex.Message,
                    innerException = ex.InnerException?.Message,
                    innerExceptionType = ex.InnerException?.GetType()?.FullName,
                    stackTrace = ex.StackTrace,
                    exceptionType = ex.GetType().FullName
                };
                
                // Log to console/logger for debugging
                Console.WriteLine($"Error adding student: {System.Text.Json.JsonSerializer.Serialize(errorDetails)}");
                
                // Return detailed error information
                return StatusCode(500, new 
                { 
                    error = "An error occurred while saving the entity changes. See the inner exception for details.",
                    details = errorDetails
                });
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
        public async Task<ActionResult<PagedResult<StudentDetailsDTO>>> GetAllStudents(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 8,
            CancellationToken cancellationToken = default)
        {
            // Clamp values to avoid abuse (e.g., pageSize=100000)
            const int maxPageSize = 100;
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 8;
            if (pageSize > maxPageSize) pageSize = maxPageSize;

            var (items, totalCount) = await _unitOfWork.Students
                .GetStudentsPageAsync(pageNumber, pageSize, cancellationToken);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return Ok(new PagedResult<StudentDetailsDTO>(
                items,
                pageNumber,
                pageSize,
                totalCount,
                totalPages
            ));
        }

        [HttpPost("page")]
        public async Task<ActionResult<PagedResult<StudentDetailsDTO>>> GetStudentsWithFilters(
            [FromBody] FilterRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Clamp values to avoid abuse (e.g., pageSize=100000)
            const int maxPageSize = 100;
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 8;
            if (request.PageSize > maxPageSize) request.PageSize = maxPageSize;

            var (items, totalCount) = await _unitOfWork.Students
                .GetStudentsPageWithFiltersAsync(request.PageNumber, request.PageSize, request.Filters, cancellationToken);

            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return Ok(new PagedResult<StudentDetailsDTO>(
                items,
                request.PageNumber,
                request.PageSize,
                totalCount,
                totalPages
            ));
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