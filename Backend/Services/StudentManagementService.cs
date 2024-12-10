using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS;
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
    private readonly IMapper _mapper;

    public StudentManagementService(
        IUserRepository userRepository,
        IGuardianRepository guardianRepository,
        IStudentRepository studentRepository,
        DatabaseContext dbContext,
        IAccountRepository accountRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _guardianRepository = guardianRepository;
        _studentRepository = studentRepository;
        _dbContext = dbContext;
        _accountRepository = accountRepository;
        _mapper = mapper;
    }

    public async Task<Student> AddStudentWithGuardianAsync(
        ApplicationUser guardianUser, string guardianPassword, Guardian guardian,
        ApplicationUser studentUser, string studentPassword, Student student,
    Accounts account, AccountStudentGuardian accountStudentGuardian,  List<Attachments> attachments,List<DisCount> studentClassFees)
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
                    {
                        attachment.StudentID = student.StudentID; // Associate with student
                        _dbContext.Attachments.Add(attachment);  // Add to database
                    }
                }
                //this is for DisCount 
             if (studentClassFees != null && studentClassFees.Any())
                {
                    foreach (var studentClassFee in studentClassFees)
                    {
                        var  studentClassFees1 = new StudentClassFees(); 
                        studentClassFees1.AmountDiscount = studentClassFee.AmountDiscount;
                        studentClassFees1.ClassID = studentClassFee.ClassID;
                        studentClassFees1.StudentID = studentClassFees1.StudentID;
                        studentClassFees1.FeeID = studentClassFee.FeeID;
                        studentClassFees1.NoteDiscount = studentClassFee.NoteDiscount;
                        _dbContext.StudentClassFees.Add(studentClassFees1);  // Add to database
                    }
                }


        return addedStudent;
        
    }
    public async Task<StudentDetailsDTO?> GetStudentByIdAsync(int id)
    {
        var student = await _dbContext.Students
            .Include(s => s.ApplicationUser) // Include ApplicationUser details
            .Include(s => s.Division)       // Include Division details if needed
            .FirstOrDefaultAsync(s => s.StudentID == id);

        if (student == null)
        {
            return null;
        }

        return new StudentDetailsDTO
        {
            StudentID = student.StudentID,
            FullName = new NameDTO
            {
                FirstName = student.FullName.FirstName,
                MiddleName = student.FullName.MiddleName,
                LastName = student.FullName.LastName
            },
            FullNameAlis = student.FullNameAlis == null ? null : new NameAlisDTO
            {
                FirstNameEng = student.FullNameAlis.FirstNameEng,
                MiddleNameEng = student.FullNameAlis.MiddleNameEng,
                LastNameEng = student.FullNameAlis.LastNameEng
            },
            DivisionID = student.DivisionID,
            PlaceBirth = student.PlaceBirth,
            UserID = student.UserID,
            ApplicationUser = new ApplicationUserDTO
            {
                Id = student.ApplicationUser.Id,
                UserName = student.ApplicationUser.UserName!,
                Email = student.ApplicationUser.Email!,
                Gender = student.ApplicationUser.Gender
            }
        };
    }

public async Task<List<StudentDetailsDTO>> GetAllStudentsAsync()
{
    var students = await _dbContext.Students
        .Include(s => s.ApplicationUser) // Include ApplicationUser details
        .Include(s => s.Division)       // Include Division details
        .Include(s => s.AccountStudentGuardians)
        .Include(s=>s.Guardian)       // Include Division details
        .ToListAsync();

    if (students == null || !students.Any())
        return new List<StudentDetailsDTO>();

    return students.Select(student => new StudentDetailsDTO
    {
        StudentID = student.StudentID,
        FullName = new NameDTO
        {
            FirstName = student.FullName.FirstName,
            MiddleName = student.FullName.MiddleName,
            LastName = student.FullName.LastName
        },
        FullNameAlis = student.FullNameAlis == null ? null : new NameAlisDTO
        {
            FirstNameEng = student.FullNameAlis.FirstNameEng,
            MiddleNameEng = student.FullNameAlis.MiddleNameEng,
            LastNameEng = student.FullNameAlis.LastNameEng
        },
        DivisionID = student.DivisionID,
        PlaceBirth = student.PlaceBirth,
        UserID = student.UserID,
        ApplicationUser = new ApplicationUserDTO
        {
            Id = student.ApplicationUser.Id,
            UserName = student.ApplicationUser.UserName!,
            Email = student.ApplicationUser.Email!,
            Gender = student.ApplicationUser.Gender
        },
        Guardians = new GuardianDto
        {
            guardianEmail=student.ApplicationUser.Email,
            guardianPhone=student.ApplicationUser.PhoneNumber!,
            guardianType=student.ApplicationUser.UserType,
            guardianDOB=student.ApplicationUser.HireDate,
            guardianFullName=student.Guardian.FullName,
            guardianAddress=student.ApplicationUser.Address!
        }
    }).ToList();
}

}
