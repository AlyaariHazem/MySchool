using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
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
    private readonly IMapper _mapper;

    public StudentManagementService(
        IUserRepository userRepository,
        IGuardianRepository guardianRepository,
        IStudentRepository studentRepository,
        DatabaseContext dbContext,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _guardianRepository = guardianRepository;
        _studentRepository = studentRepository;
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<Student> AddStudentWithGuardianAsync(
        ApplicationUser guardianUser, string guardianPassword, Guardian guardian,
        ApplicationUser studentUser, string studentPassword, Student student)
    {
        // Step 1: Add Guardian's User
        var createdGuardianUser = await _userRepository.CreateUserAsync(guardianUser, guardianPassword, "Guardian");
        guardian.UserID = createdGuardianUser.Id;

        // Step 2: Add Guardian
        var addedGuardian = await _guardianRepository.AddGuardianAsync(guardian);
        student.GuardianID = addedGuardian.GuardianID;

        // Step 3: Add Student's User
        var createdStudentUser = await _userRepository.CreateUserAsync(studentUser, studentPassword, "Student");
        student.UserID = createdStudentUser.Id;

        // Step 4: Add Student
        return await _studentRepository.AddStudentAsync(student);
    }
    public async Task<StudentDetailsDTO?> GetStudentByIdAsync(int id)
    {
        var student = await _dbContext.Students
            .Include(s => s.ApplicationUser) // Include ApplicationUser details
            .Include(s => s.Guardian)        // Include Guardian details
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
            GuardianID = student.GuardianID,
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
        .Include(s => s.Guardian)        // Include Guardian details
        .Include(s => s.Division)       // Include Division details
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
        GuardianID = student.GuardianID,
        UserID = student.UserID,
        ApplicationUser = new ApplicationUserDTO
        {
            Id = student.ApplicationUser.Id,
            UserName = student.ApplicationUser.UserName!,
            Email = student.ApplicationUser.Email!,
            Gender = student.ApplicationUser.Gender
        }
    }).ToList();
}

}
