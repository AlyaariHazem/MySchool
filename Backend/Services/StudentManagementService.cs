using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS;
using Backend.DTOS.School.Accounts;
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
    private readonly DatabaseContext _dbContext;
    private readonly mangeFilesService _mangeFilesService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentManagementService(
        DatabaseContext dbContext,
        IMapper mapper,
        mangeFilesService mangeFilesService,
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager) // Inject UserManager

    {
        _dbContext = dbContext;
        _mapper = mapper;
        _mangeFilesService = mangeFilesService;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<Student> AddStudentWithGuardianAsync(
        ApplicationUser guardianUser, string guardianPassword, Guardian guardian,
        ApplicationUser studentUser, string studentPassword, Student student,
    AccountsDTO account, AccountStudentGuardian accountStudentGuardian,  List<Attachments> attachments,List<StudentClassFeeDTO> studentClassFees)
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


         // Step 4: Create Account
            var createdAccount = await _unitOfWork.Accounts.AddAccountAsync(account);

            // Step 5: Create AccountStudentGuardian Mapping
            accountStudentGuardian.AccountID = createdAccount.AccountID ?? default(int);
            accountStudentGuardian.GuardianID = addedGuardian.GuardianID;
            accountStudentGuardian.StudentID = addedStudent.StudentID;
            await _unitOfWork.Accounts.AddAccountStudentGuardianAsync(accountStudentGuardian);
            
             if (attachments != null && attachments.Any())
                {
                    foreach (var attachment in attachments)
                    { // Associate with student
                         await  _dbContext.Attachments.AddAsync(attachment); 
                         await _dbContext.SaveChangesAsync();
                    }
                }

           try
            {
                foreach(var studentClassFee in studentClassFees)
                {
                    var studentClassFeeMapped = _mapper.Map<StudentClassFees>(studentClassFee);
                    await _dbContext.StudentClassFees.AddAsync(studentClassFeeMapped);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving StudentClassFees: {ex.Message}");
            }

            return addedStudent;
        }
  public async Task<Student> AddStudentToExistingGuardianAsync(
    GuardianDTO existingGuardian,
    ApplicationUser studentUser, string studentPassword, Student student,
    List<Attachments> attachments, List<StudentClassFeeDTO> studentClassFees,AccountStudentGuardian accountStudentGuardianPram)
    {
        // 1. Create student user
        var createdStudentUser = await _unitOfWork.Users.CreateUserAsync(studentUser, studentPassword, "Student");
        student.UserID = createdStudentUser.Id;
        student.GuardianID = existingGuardian.GuardianID;

        // 2. Add Student
        var addedStudent = await _unitOfWork.Students.AddStudentAsync(student);

        // 3. Retrieve the existing account ID associated with the guardian
        var existingAccountStudentGuardian = await _unitOfWork.Accounts.GetAccountStudentGuardianByGuardianIdAsync(existingGuardian.GuardianID);
        if (existingAccountStudentGuardian == null)
        {
            throw new Exception("No account found for the existing guardian.");
        }

        // 4. Create AccountStudentGuardian Mapping using the existing account ID
        var accountStudentGuardian = new AccountStudentGuardian
        {
            AccountID = existingAccountStudentGuardian.AccountID, // Use the existing account ID
            GuardianID = existingGuardian.GuardianID,
            StudentID = addedStudent.StudentID,
            Amount=accountStudentGuardianPram.Amount
        };

        await _unitOfWork.Accounts.AddAccountStudentGuardianAsync(accountStudentGuardian);

        // 5. Handle attachments
         if (attachments != null && attachments.Any())
            {
                foreach (var attachment in attachments)
                { // Associate with student
                        attachment.StudentID = addedStudent.StudentID;
                        await  _dbContext.Attachments.AddAsync(attachment); 
                        await _dbContext.SaveChangesAsync();
                }
            }

        // 6. Handle class fees
          foreach(var studentClassFee in studentClassFees)
            {
                var studentClassFeeMapped = _mapper.Map<StudentClassFees>(studentClassFee);
                await _dbContext.StudentClassFees.AddAsync(studentClassFeeMapped);
                await _dbContext.SaveChangesAsync();
            }

        return addedStudent;
    }
    
public async Task<StudentDetailsDTO> UpdateStudentWithGuardianAsync(int studentId, UpdateStudentWithGuardianRequestDTO request)
{
    using var transaction = await _dbContext.Database.BeginTransactionAsync();

    try
    {
        // **Update Student User**
        var studentUser = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Student != null && u.Student.StudentID == studentId);
        
        if (studentUser is null)
            throw new Exception("Student user not found.");
        

        studentUser.Email = request.StudentEmail ?? studentUser.Email;
        studentUser.PhoneNumber = request.StudentPhone ?? studentUser.PhoneNumber;
        studentUser.Address = request.StudentAddress ?? studentUser.Address;

        _dbContext.Entry(studentUser).State = EntityState.Modified;

        // **Update Student Entity**
        var student = await _unitOfWork.Students.GetStudentAsync(studentId);
        if (student == null)
            throw new Exception("Student not found.");
        

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
        student.ImageURL = request.StudentImageURL ?? student.ImageURL;
        student.StudentDOB = request.StudentDOB != default ? request.StudentDOB : student.StudentDOB;

        _dbContext.Entry(student).State = EntityState.Modified;

        // **Update Guardian User and Entity**
        if (request.ExistingGuardianId.HasValue)
        {
            var guardian = await _unitOfWork.Guardians.GetGuardianByIdAsync(request.ExistingGuardianId.Value);
            if (guardian != null)
            {
                guardian.FullName = request.GuardianFullName ?? guardian.FullName;
                guardian.GuardianDOB = request.GuardianDOB != default ? request.GuardianDOB : guardian.GuardianDOB;
                guardian.Type = request.GuardianType ?? guardian.Type;

                _dbContext.Entry(guardian).State = EntityState.Modified;

                var guardianUser = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Id == guardian.UserID);
                if (guardianUser != null)
                {
                    guardianUser.Email = request.GuardianEmail ?? guardianUser.Email;
                    guardianUser.PhoneNumber = request.GuardianPhone ?? guardianUser.PhoneNumber;
                    guardianUser.Address = request.GuardianAddress ?? guardianUser.Address;

                    _dbContext.Entry(guardianUser).State = EntityState.Modified;
                }
            }
        }

        // **Handle Attachments**
        if (request.Files != null && request.Files.Any())
        {
            var filePaths = await this._mangeFilesService.UploadAttachments(request.Files, student.StudentID);
            var attachments = filePaths.Select(filePath => new Attachments
            {
                StudentID = student.StudentID,
                AttachmentURL = filePath,
            }).ToList();

            await _dbContext.Attachments.AddRangeAsync(attachments);
        }

        // **Update Discounts**
        if (request.UpdateDiscounts != null && request.UpdateDiscounts.Any())
        {
            var existingDiscounts =request.UpdateDiscounts;
            foreach (var discount in existingDiscounts)
            {
                await _unitOfWork.StudentClassFees.UpdateAsync(discount);
                    
            }
        }

        // **Save Changes and Commit Transaction**
        await _dbContext.SaveChangesAsync();
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
