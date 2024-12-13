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
        StudnetDOB=student.ApplicationUser.HireDate,
        StudentAddress=student.ApplicationUser.Address,
        PlaceBirth = student.PlaceBirth,
        StudentPhone = student.ApplicationUser.PhoneNumber,
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
            guardianFullName=student.Guardian.FullName,
            guardianType=student.Guardian.Type!,
            guardianEmail=student.ApplicationUser.Email,
            guardianPhone=student.ApplicationUser.PhoneNumber!,
            guardianDOB=student.ApplicationUser.HireDate,
            guardianAddress=student.ApplicationUser.Address!
        }
    }).ToList();

}
}
