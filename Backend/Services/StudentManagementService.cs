using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS;
using Backend.DTOS.School.StudentClassFee;
using Backend.DTOS.School.Students;
using Backend.Models;
using Backend.Repository.School.Implements;
using Backend.Repository.School.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class StudentManagementService
{
    private readonly IUserRepository _userRepository;
    private readonly IGuardianRepository _guardianRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly DatabaseContext _dbContext;
    private readonly IAccountRepository _accountRepository;
    private readonly IStudentClassFeeRepository _studentClassFeeRepository;
    private readonly IMapper _mapper;

    public StudentManagementService(
        IUserRepository userRepository,
        IGuardianRepository guardianRepository,
        IStudentRepository studentRepository,
        DatabaseContext dbContext,
        IAccountRepository accountRepository,
        IStudentClassFeeRepository studentClassFeeRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _guardianRepository = guardianRepository;
        _studentRepository = studentRepository;
        _dbContext = dbContext;
        _accountRepository = accountRepository;
        _studentClassFeeRepository = studentClassFeeRepository;
        _mapper = mapper;
    }

    public async Task<Student> AddStudentWithGuardianAsync(
        ApplicationUser guardianUser, string guardianPassword, Guardian guardian,
        ApplicationUser studentUser, string studentPassword, Student student,
    Accounts account, AccountStudentGuardian accountStudentGuardian,  List<Attachments> attachments,List<StudentClassFeeDTO> studentClassFees)
    {
        // Step 1: Add Guardian's User
        var createdGuardianUser = await _userRepository.CreateUserAsync(guardianUser, guardianPassword, "Guardian");
        guardian.UserID = createdGuardianUser.Id;

        // Step 2: Add Guardian
        var addedGuardian = await _guardianRepository.AddGuardianAsync(guardian);

        // Step 3: Add Student's User
        var createdStudentUser = await _userRepository.CreateUserAsync(studentUser, studentPassword, "Student");
        student.UserID = createdStudentUser.Id;
        student.GuardianID = addedGuardian.GuardianID;
        
         var addedStudent = await _studentRepository.AddStudentAsync(student);


         // Step 4: Create Account
            var createdAccount = await _accountRepository.AddAccountAsync(account);

            // Step 5: Create AccountStudentGuardian Mapping
            accountStudentGuardian.AccountID = createdAccount.AccountID;
            accountStudentGuardian.GuardianID = addedGuardian.GuardianID;
            accountStudentGuardian.StudentID = addedStudent.StudentID;
            await _accountRepository.AddAccountStudentGuardianAsync(accountStudentGuardian);
            
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
    Guardian existingGuardian,
    ApplicationUser studentUser, string studentPassword, Student student,
    List<Attachments> attachments, List<StudentClassFeeDTO> studentClassFees,AccountStudentGuardian accountStudentGuardianPram)
    {
        // 1. Create student user
        var createdStudentUser = await _userRepository.CreateUserAsync(studentUser, studentPassword, "Student");
        student.UserID = createdStudentUser.Id;
        student.GuardianID = existingGuardian.GuardianID;

        // 2. Add Student
        var addedStudent = await _studentRepository.AddStudentAsync(student);

        // 3. Retrieve the existing account ID associated with the guardian
        var existingAccountStudentGuardian = await _accountRepository.GetAccountStudentGuardianByGuardianIdAsync(existingGuardian.GuardianID);
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

        await _accountRepository.AddAccountStudentGuardianAsync(accountStudentGuardian);

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

    public async Task<List<string>> UploadAttachments(List<IFormFile> files, int studentId)
    {
        if (files == null || !files.Any())
            return null;

        var uploadsFolder = Path.Combine("wwwroot", "uploads", "Attachments");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var filePaths = new List<string>();
        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                var fileExtension = Path.GetExtension(file.FileName);
                if (!new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf" }.Contains(fileExtension.ToLower()))
                    throw new InvalidOperationException("Invalid file type.");

                var filePath = Path.Combine(uploadsFolder, $"{studentId}_{Path.GetFileName(file.FileName)}");
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    filePaths.Add(filePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error uploading file {file.FileName}: {ex.Message}");
                }
            }
        }
        return filePaths;
    }
    public async Task<List<string>> UploadStudentImage(IFormFile file, int studentId)
    {
        if (file == null)
            return null;

        var uploadsFolder = Path.Combine("wwwroot", "uploads", "StudentPhotos");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var filePaths = new List<string>();
        if (file.Length > 0)
        {
            var fileExtension = Path.GetExtension(file.FileName);
            if (!new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(fileExtension.ToLower()))
                throw new InvalidOperationException("Invalid file type.");

                var filePath = Path.Combine(uploadsFolder, $"{studentId}_{Path.GetFileName(file.FileName)}");
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    filePaths.Add(filePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error uploading file {file.FileName}: {ex.Message}");
                }
            }
        return filePaths;
    }

}
