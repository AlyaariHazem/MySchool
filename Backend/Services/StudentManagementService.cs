using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS;
using Backend.DTOS.School.Accounts;
using Backend.DTOS.School.Attachments;
using Backend.DTOS.School.Guardians;
using Backend.DTOS.School.StudentClassFee;
using Backend.DTOS.School.Students;
using Backend.Interfaces;
using Backend.Models;
using Backend.Repository.School.Implements;
using Backend.Repository.School.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class StudentManagementService
{
    private readonly TenantDbContext _tenantContext; // For tenant-specific operations
    private readonly DatabaseContext _dbContext; // For admin operations (if needed)
    private readonly mangeFilesService _mangeFilesService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentManagementService(
        TenantDbContext tenantContext,
        DatabaseContext dbContext,
        IMapper mapper,
        mangeFilesService mangeFilesService,
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager) // Inject UserManager

    {
        _tenantContext = tenantContext;
        _dbContext = dbContext;
        _mapper = mapper;
        _mangeFilesService = mangeFilesService;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<Student> AddStudentWithGuardianAsync(
        ApplicationUser guardianUser, string guardianPassword, Guardian guardian,
        ApplicationUser studentUser, string studentPassword, Student student,
    AccountsDTO account, AccountStudentGuardian accountStudentGuardian, List<Attachments> attachments, List<StudentClassFeeDTO> studentClassFees)
    {
        using var transaction = await _tenantContext.Database.BeginTransactionAsync();
        
        try
        {
            // Step 1: Add Guardian's User
            var createdGuardianUser = await _unitOfWork.Users.CreateUserAsync(guardianUser, guardianPassword, "Guardian");
            guardian.UserID = createdGuardianUser.Id;

            // Step 2: Add Guardian
            var addedGuardian = await _unitOfWork.Guardians.AddGuardianAsync(guardian);

            // Step 3: Add Student's User
            var createdStudentUser = await _unitOfWork.Users.CreateUserAsync(studentUser, studentPassword, "Student");
            student.UserID = createdStudentUser.Id;
            student.GuardianID = addedGuardian.GuardianID;

            var addedStudent = await _unitOfWork.Students.AddStudentAsync(student);

            // Step 4: Create Account (only when creating a new guardian)
            var createdAccount = await _unitOfWork.Accounts.AddAccountAsync(account);
            if (createdAccount?.AccountID == null)
            {
                throw new Exception("Failed to create account. AccountID is null.");
            }

            // Step 5: Create AccountStudentGuardian Mapping
            accountStudentGuardian.AccountID = createdAccount.AccountID.Value;
            accountStudentGuardian.GuardianID = addedGuardian.GuardianID;
            accountStudentGuardian.StudentID = addedStudent.StudentID;
            // Ensure amount is set (should already be set from the request)
            await _unitOfWork.AccountStudentGuardians.AddAccountStudentGuardianAsync(accountStudentGuardian);

            // Step 6: Handle attachments
            if (attachments != null && attachments.Any())
            {
                foreach (var attachment in attachments)
                {
                    attachment.StudentID = addedStudent.StudentID; // Ensure StudentID is set
                    var attachmentDTO = new AttachmentDTO
                    {
                        StudentID = attachment.StudentID,
                        AttachmentURL = attachment.AttachmentURL,
                        VoucherID = attachment.VoucherID
                    };
                    await _unitOfWork.Attachments.AddAsync(attachmentDTO);
                }
            }

            // Step 7: Handle class fees
            if (studentClassFees != null && studentClassFees.Any())
            {
                foreach (var studentClassFee in studentClassFees)
                {
                    studentClassFee.StudentID = addedStudent.StudentID; // Ensure StudentID is set
                    await _unitOfWork.StudentClassFees.AddAsync(studentClassFee);
                }
            }

            // Commit the transaction
            await transaction.CommitAsync();

            return addedStudent;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"Error adding student with guardian: {ex.Message}", ex);
        }
    }
    public async Task<Student> AddStudentToExistingGuardianAsync(
      GuardianDTO existingGuardian,
      ApplicationUser studentUser, string studentPassword, Student student,
      List<Attachments> attachments, List<StudentClassFeeDTO> studentClassFees, AccountStudentGuardian accountStudentGuardianPram)
    {
        // IMPORTANT: This method is called when an EXISTING guardian is selected.
        // DO NOT create:
        // - New Guardian User (AspNetUsers record) - the guardian user already exists
        // - New Guardian (Guardians table record) - the guardian already exists
        // - New Account (Accounts table record) - use the existing guardian's account
        
        // Validate that existingGuardian is provided and has a valid ID
        if (existingGuardian == null)
            throw new ArgumentNullException(nameof(existingGuardian), "Existing guardian must be provided.");
        
        if (existingGuardian.GuardianID <= 0)
            throw new ArgumentException("Existing guardian must have a valid GuardianID.", nameof(existingGuardian));
        
        try
        {
            // Verify the guardian exists in the database (defensive check)
            var guardianExists = await _tenantContext.Guardians
                .AnyAsync(g => g.GuardianID == existingGuardian.GuardianID);
            
            if (!guardianExists)
            {
                throw new Exception($"Guardian with ID {existingGuardian.GuardianID} does not exist in the database.");
            }

            // 1. Create student user (only create student-related records)
            // DO NOT create any guardian user or guardian entity here
            var createdStudentUser = await _unitOfWork.Users.CreateUserAsync(studentUser, studentPassword, "Student");
            student.UserID = createdStudentUser.Id;
            student.GuardianID = existingGuardian.GuardianID; // Link to existing guardian

            // 2. Add Student (this method has its own transaction, so we don't wrap it)
            var addedStudent = await _unitOfWork.Students.AddStudentAsync(student);

            // 3. Retrieve the existing account ID associated with the guardian
            // Query directly from the context to ensure we get the account ID correctly
            var existingAccountStudentGuardian = await _tenantContext.AccountStudentGuardians
                .Where(asg => asg.GuardianID == existingGuardian.GuardianID)
                .FirstOrDefaultAsync();

            if (existingAccountStudentGuardian == null)
            {
                throw new Exception($"No account found for the existing guardian with ID {existingGuardian.GuardianID}. The guardian must have at least one student with an associated account.");
            }

            // Get the account ID from the existing record
            int existingAccountID = existingAccountStudentGuardian.AccountID;

            // 4. Create AccountStudentGuardian Mapping using the existing account ID
            // IMPORTANT: Use the amount from the request parameter, not from the existing record
            // Note: AddAccountStudentGuardianAsync calls SaveChangesAsync internally, so we don't need a transaction here
            var accountStudentGuardian = new AccountStudentGuardian
            {
                AccountID = existingAccountID, // Use the existing account ID (DO NOT create a new account)
                GuardianID = existingGuardian.GuardianID,
                StudentID = addedStudent.StudentID,
                Amount = accountStudentGuardianPram.Amount // Use the amount from the request
            };

            await _unitOfWork.AccountStudentGuardians.AddAccountStudentGuardianAsync(accountStudentGuardian);

            // 5. Handle attachments - use UnitOfWork to save to tenant database
            if (attachments != null && attachments.Any())
            {
                foreach (var attachment in attachments)
                {
                    attachment.StudentID = addedStudent.StudentID;
                    var attachmentDTO = new AttachmentDTO
                    {
                        StudentID = attachment.StudentID,
                        AttachmentURL = attachment.AttachmentURL,
                        VoucherID = attachment.VoucherID
                    };
                    await _unitOfWork.Attachments.AddAsync(attachmentDTO);
                }
            }

            // 6. Handle class fees - use UnitOfWork to save to tenant database
            if (studentClassFees != null && studentClassFees.Any())
            {
                foreach (var studentClassFee in studentClassFees)
                {
                    studentClassFee.StudentID = addedStudent.StudentID; // Ensure StudentID is set
                    await _unitOfWork.StudentClassFees.AddAsync(studentClassFee);
                }
            }

            return addedStudent;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error adding student to existing guardian: {ex.Message}", ex);
        }
    }

    public async Task<StudentDetailsDTO> UpdateStudentWithGuardianAsync(int studentId, UpdateStudentWithGuardianRequestDTO request)
    {
        using var transaction = await _tenantContext.Database.BeginTransactionAsync();

        try
        {
            // **Update Student Entity** - Get student first to get UserID
            var student = await _unitOfWork.Students.GetStudentAsync(studentId);
            if (student == null)
                throw new Exception("Student not found.");

            // **Update Student User** - Use UserID from student to find user in admin database
            var studentUser = await _userManager.FindByIdAsync(student.UserID);
            if (studentUser is null)
                throw new Exception("Student user not found.");

            studentUser.Email = request.StudentEmail ?? studentUser.Email;
            studentUser.PhoneNumber = request.StudentPhone ?? studentUser.PhoneNumber;
            studentUser.Address = request.StudentAddress ?? studentUser.Address;
            studentUser.Gender = request.StudentGender ?? studentUser.Gender;
            if (request.HireDate.HasValue && request.HireDate.Value != default)
                studentUser.HireDate = request.HireDate.Value;

            await _userManager.UpdateAsync(studentUser); // Update via Identity manager


            student.FullName.FirstName = request.StudentFirstName ?? student.FullName.FirstName;
            student.FullName.MiddleName = request.StudentMiddleName ?? student.FullName.MiddleName;
            student.FullName.LastName = request.StudentLastName ?? student.FullName.LastName;

            if (student.FullNameAlis != null)
            {
                student.FullNameAlis.FirstNameEng = request.StudentFirstNameEng ?? student.FullNameAlis.FirstNameEng;
                student.FullNameAlis.MiddleNameEng = request.StudentMiddleNameEng ?? student.FullNameAlis.MiddleNameEng;
                student.FullNameAlis.LastNameEng = request.StudentLastNameEng ?? student.FullNameAlis.LastNameEng;
            }

            student.DivisionID = request.DivisionID != 0 ? request.DivisionID : student.DivisionID;
            student.PlaceBirth = request.PlaceBirth ?? student.PlaceBirth;
            
            // Handle image URL - if it's already in the correct format, use it; otherwise construct it
            if (!string.IsNullOrWhiteSpace(request.StudentImageURL))
            {
                // If it already starts with "StudentPhotos_", use it as-is (already uploaded)
                if (request.StudentImageURL.StartsWith("StudentPhotos_"))
                {
                    student.ImageURL = request.StudentImageURL;
                }
                else
                {
                    // Extract just the filename if a full path is provided
                    var fileName = Path.GetFileName(request.StudentImageURL);
                    student.ImageURL = $"StudentPhotos_{studentId}_{fileName}";
                }
            }

            student.StudentDOB = request.StudentDOB != default ? request.StudentDOB : student.StudentDOB;

            // No need to manually set EntityState.Modified - EF Core tracks changes automatically

            // **Update Guardian User and Entity**
            // IMPORTANT: When ExistingGuardianId is provided, we should NEVER create a new guardian
            // We only update the existing guardian or change the reference to another existing guardian
            if (request.ExistingGuardianId.HasValue && request.ExistingGuardianId.Value > 0)
            {
                // Verify the existing guardian exists
                var existingGuardian = await _unitOfWork.Guardians.GetGuardianByIdAsync(request.ExistingGuardianId.Value);
                if (existingGuardian == null)
                    throw new Exception($"Existing Guardian with ID {request.ExistingGuardianId.Value} not found.");

                if (request.ExistingGuardianId.Value == student.GuardianID)
                {
                    // Update the current guardian's information
                    var guardian = await _unitOfWork.Guardians.GetGuardianByGuardianIdAsync(request.ExistingGuardianId.Value);
                    if (guardian != null)
                    {
                        guardian.FullName = request.GuardianFullName ?? guardian.FullName;
                        guardian.GuardianDOB = request.GuardianDOB != default ? request.GuardianDOB : guardian.GuardianDOB;
                        guardian.Type = request.GuardianType ?? guardian.Type;

                        var guardianUser = await _userManager.Users
                            .FirstOrDefaultAsync(u => u.Id == guardian.UserID);
                        if (guardianUser != null)
                        {
                            guardianUser.Email = request.GuardianEmail ?? guardianUser.Email;
                            guardianUser.PhoneNumber = request.GuardianPhone ?? guardianUser.PhoneNumber;
                            guardianUser.Address = request.GuardianAddress ?? guardianUser.Address;

                            await _userManager.UpdateAsync(guardianUser); // update via Identity manager
                        }

                        await _tenantContext.SaveChangesAsync(); // save guardian changes
                    }
                }
                else
                {
                    // Change the student's guardian to a different existing guardian
                    // Step 1: Get the wrapper result
                    var accountStudentGuardianResult = await _unitOfWork.AccountStudentGuardians.GetAcountStudentGuardianByIdAsync(studentId);

                    // Step 2: Get the actual DTO from the result
                    var dto = accountStudentGuardianResult.Value;
                    if (dto == null)
                        throw new Exception("AccountStudentGuardian not found.");

                    // Step 3: Get the entity from the DB to update
                    var entity = await _tenantContext.AccountStudentGuardians.FindAsync(dto.AccountStudentGuardianID);
                    if (entity == null)
                        throw new Exception("AccountStudentGuardian entity not found.");

                    // Step 4: Update the guardian reference to the existing guardian
                    entity.GuardianID = request.ExistingGuardianId.Value;

                    // Step 5: Update student as well
                    var studentEntity = await _unitOfWork.Students.GetStudentAsync(studentId);
                    if (studentEntity == null)
                        throw new Exception("Student not found.");

                    studentEntity.GuardianID = request.ExistingGuardianId.Value;

                    // No need to manually set EntityState.Modified - EF Core tracks changes automatically
                }
            }
            else
            {
                // If ExistingGuardianId is not provided, update the current guardian (don't create a new one)
                var currentGuardian = await _unitOfWork.Guardians.GetGuardianByGuardianIdAsync(student.GuardianID);
                if (currentGuardian != null)
                {
                    currentGuardian.FullName = request.GuardianFullName ?? currentGuardian.FullName;
                    currentGuardian.GuardianDOB = request.GuardianDOB != default ? request.GuardianDOB : currentGuardian.GuardianDOB;
                    currentGuardian.Type = request.GuardianType ?? currentGuardian.Type;

                    var guardianUser = await _userManager.Users
                        .FirstOrDefaultAsync(u => u.Id == currentGuardian.UserID);
                    if (guardianUser != null)
                    {
                        guardianUser.Email = request.GuardianEmail ?? guardianUser.Email;
                        guardianUser.PhoneNumber = request.GuardianPhone ?? guardianUser.PhoneNumber;
                        guardianUser.Address = request.GuardianAddress ?? guardianUser.Address;

                        await _userManager.UpdateAsync(guardianUser);
                    }

                    await _tenantContext.SaveChangesAsync();
                }
            }

            // Remove old image only if a new image is being uploaded
            if (!string.IsNullOrWhiteSpace(request.StudentImageURL) && student.ImageURL != request.StudentImageURL)
            {
                // Only remove if there was a previous image
                if (!string.IsNullOrWhiteSpace(student.ImageURL))
                {
                    _mangeFilesService.RemoveFile("StudentPhotos", studentId);
                }
            }


            // **Update Discounts**
            if (request.UpdateDiscounts != null && request.UpdateDiscounts.Any())
            {
                var existingDiscounts = request.UpdateDiscounts;
                var studentsClassFees = await _tenantContext.StudentClassFees
                    .Where(s => s.StudentID == request.StudentID)
                    .ToListAsync();
                _tenantContext.StudentClassFees.RemoveRange(studentsClassFees);

                foreach (var discount in existingDiscounts)
                {
                    var studentClassFees = (new StudentClassFeeDTO
                    {
                        StudentID = request.StudentID,
                        FeeClassID = discount.FeeClassID,
                        AmountDiscount = discount.AmountDiscount,
                        NoteDiscount = discount.NoteDiscount,
                        Mandatory = discount.Mandatory
                    });
                    await _unitOfWork.StudentClassFees.AddAsync(studentClassFees);
                }
            }

            // **Update Attachments**
            if (request.Attachments != null && request.Attachments.Any())
            {
                foreach (var attachment in request.Attachments)
                {
                    if (string.IsNullOrWhiteSpace(attachment))
                        continue;

                    // Extract the actual filename from the attachment string
                    // The frontend sends full URLs like: "https://localhost:7258/uploads/Attachments/Attachments_6_CV.pdf"
                    // But the database stores just: "Attachments_6_CV.pdf"
                    string attachmentURL;
                    
                    // Check if it's a full URL (contains http:// or https://)
                    if (attachment.StartsWith("http://") || attachment.StartsWith("https://"))
                    {
                        // Extract just the filename from the full URL
                        // e.g., "https://localhost:7258/uploads/Attachments/Attachments_6_CV.pdf" -> "Attachments_6_CV.pdf"
                        attachmentURL = Path.GetFileName(attachment);
                    }
                    // Check if it already has the correct prefix format (just the filename)
                    else if (attachment.StartsWith($"Attachments_{request.StudentID}_"))
                    {
                        attachmentURL = attachment; // Already in correct format: "Attachments_6_CV.pdf"
                    }
                    else if (attachment.StartsWith("Attachments_"))
                    {
                        // Has prefix but might be for different student ID, extract the actual filename
                        var parts = attachment.Split('_');
                        if (parts.Length >= 3)
                        {
                            // Extract everything after "Attachments_{id}_"
                            var actualFileName = string.Join("_", parts.Skip(2));
                            attachmentURL = $"Attachments_{request.StudentID}_{actualFileName}";
                        }
                        else
                        {
                            attachmentURL = $"Attachments_{request.StudentID}_{attachment}";
                        }
                    }
                    else
                    {
                        // Just a filename without prefix
                        attachmentURL = $"Attachments_{request.StudentID}_{attachment}";
                    }

                    // تأكد من عدم التكرار - Check if this attachment already exists in database
                    bool alreadyExists = await _tenantContext.Attachments
                        .AnyAsync(a =>
                            a.AttachmentURL == attachmentURL
                            && a.StudentID == request.StudentID
                        );

                    if (!alreadyExists)
                    {
                        var newAttachment = new Attachments
                        {
                            StudentID = request.StudentID,
                            AttachmentURL = attachmentURL
                        };
                        await _tenantContext.Attachments.AddAsync(newAttachment);
                    }
                }
            }



            // **Save Changes and Commit Transaction**
            await _tenantContext.SaveChangesAsync();
            await transaction.CommitAsync();

            // Return updated student details
            var updatedStudent = await _unitOfWork.Students.GetStudentByIdAsync(studentId);
            if (updatedStudent == null)
                throw new Exception("Student data not found after update.");
            return updatedStudent;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"Error updating student: {ex.Message}");
        }
    }

}
