using Backend.Data;
using Backend.DTOS;
using Backend.DTOS.School.Attachments;
using Backend.DTOS.School.Students;
using Backend.Models;
using Backend.Repository.School.Implements;
using Backend.Repository.School.Interfaces;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class StudentRepository : IStudentRepository
{
    private readonly DatabaseContext _context;
     private readonly IGuardianRepository _guardianRepo;
     private readonly mangeFilesService _mangeFilesService;

    public StudentRepository(DatabaseContext context,IGuardianRepository guardianRepo,mangeFilesService mangeFilesService)
    {
        _context = context;
        _guardianRepo = guardianRepo;
        _mangeFilesService = mangeFilesService;
    }

    // Create: Add a new student
    public async Task<Student> AddStudentAsync(Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        return student;
    }

    // Read: Get all students
public async Task<StudentDetailsDTO?> GetStudentByIdAsync(int id)
{
    var student = await _context.Students
        .Include(s => s.ApplicationUser)
        .Include(s => s.Division)
        .FirstOrDefaultAsync(s => s.StudentID == id);

    if (student == null)
    {
        return null;
    }

    string baseUrl = "https://localhost:7258/uploads/StudentPhotos";

    return new StudentDetailsDTO
    {
        StudentID = student.StudentID,
        FullName = new NameDTO
        {
            FirstName = student.FullName.FirstName,
            MiddleName = student.FullName.MiddleName,
            LastName = student.FullName.LastName
        },
        PhotoUrl = student.ImageURL != null
            ? $"{baseUrl}/{student.ImageURL}"
            : $"{baseUrl}/default-placeholder.png",
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
public async Task<Student?> GetStudentAsync(int id)
{
    var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentID == id);

    if (student == null)
    {
        return null;
    }
        return student;

}

  public async Task<List<StudentDetailsDTO>> GetAllStudentsAsync()
{
    var students = await _context.Students
        .Include(s => s.ApplicationUser)  // Include ApplicationUser details
        .Include(s => s.Division)        // Include Division details
            .ThenInclude(d => d.Class)   // Include Class details
                .ThenInclude(c => c.Stage) // Include Stage details
        .Include(ASG => ASG.AccountStudentGuardians)
        .Include(s => s.Guardian)        // Include Guardian details
        .ToListAsync();

    if (students == null || !students.Any())
        return new List<StudentDetailsDTO>();

    string baseUrl = "https://localhost:7258/uploads/StudentPhotos";

    return students.Select(student => new StudentDetailsDTO
    {
        StudentID = student.StudentID,
        FullName = new NameDTO
        {
            FirstName = student.FullName.FirstName,
            MiddleName = student.FullName.MiddleName,
            LastName = student.FullName.LastName
        },
        PhotoUrl = student.ImageURL != null
            ? $"{baseUrl}/{student.ImageURL}" 
            : $"{baseUrl}/default-placeholder.png",
        DivisionID = student.DivisionID,
        DivisionName = student.Division?.DivisionName,
        ClassName = student.Division?.Class?.ClassName,
        StageName = student.Division?.Class?.Stage?.StageName,
        Age = student.StudentDOB.HasValue
            ? DateTime.Now.Year - student.StudentDOB.Value.Year
            : (int?)null,
        Gender = student.ApplicationUser.Gender,
        HireDate = student.ApplicationUser.HireDate,
        PlaceBirth = student.PlaceBirth,
        Fee = student.AccountStudentGuardians?.Sum(asg => asg.Amount) ?? 0, // Aggregate Fee Amount
        StudentPhone = student.ApplicationUser.PhoneNumber,
        StudentAddress = student.ApplicationUser.Address,
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
            guardianFullName = student.Guardian.FullName,
            guardianType = student.Guardian.Type!,
            guardianEmail = student.ApplicationUser.Email,
            guardianPhone = student.ApplicationUser.PhoneNumber!,
            guardianDOB = student.ApplicationUser.HireDate,
            guardianAddress = student.ApplicationUser.Address!
        }
    }).ToList();
}

    public async Task<GetStudentForUpdateDTO?> GetUpdateStudentWithGuardianRequestData(int studentId)
    {
        var student = await _context.Students
            .Include(s => s.ApplicationUser)
            .Include(s => s.Guardian)
            .Include(s => s.Attachments)
            .Include(s => s.Division)
             .Include(s => s.StudentClassFees)
                .ThenInclude(f => f.FeeClass)
                  .ThenInclude(fc => fc.Class)
            .FirstOrDefaultAsync(s => s.StudentID == studentId);

        if (student == null)
        {
            return null;
        }
        string PhotoUrl = "https://localhost:7258/uploads/StudentPhotos";
        string AttachmentUrl = "https://localhost:7258/uploads/Attachments";
         var guardianInfo =_guardianRepo.GetGuardianByIdForUpdateAsync(student!.GuardianID);

        var guardian = student.Guardian;

        return new GetStudentForUpdateDTO
        {
            // Guardian Details
            ExistingGuardianId = guardian?.GuardianID,
            GuardianEmail = guardianInfo.Result.GuardianEmail,
            GuardianPassword = string.Empty,
            GuardianAddress =guardianInfo.Result.GuardianAddress,
            GuardianGender = guardianInfo.Result.Gender!,
            GuardianFullName = guardianInfo.Result.GuardianFullName,
            GuardianType = guardianInfo.Result.Type!,
            GuardianPhone = guardianInfo.Result.GuardianPhone!,
            GuardianDOB = guardianInfo.Result.GuardianDOB,

            // Student Details
            StudentID = student.StudentID,
            StudentEmail = student.ApplicationUser?.Email ?? string.Empty,
            StudentPassword = string.Empty, // Passwords are not retrievable
            StudentAddress = student.ApplicationUser?.Address ?? string.Empty,
            StudentGender = student.ApplicationUser?.Gender ?? "Not Specified",
            StudentFirstName = student.FullName?.FirstName ?? string.Empty,
            StudentMiddleName = student.FullName?.MiddleName ?? string.Empty,
            StudentLastName = student.FullName?.LastName ?? string.Empty,
            StudentFirstNameEng = student.FullNameAlis?.FirstNameEng ?? string.Empty,
            StudentMiddleNameEng = student.FullNameAlis?.MiddleNameEng ?? string.Empty,
            StudentLastNameEng = student.FullNameAlis?.LastNameEng ?? string.Empty,
            StudentImageURL = student.ImageURL!=null?$"{PhotoUrl}/{student.ImageURL}": string.Empty,
            ClassID = student.Division?.ClassID ?? 0,
            DivisionID = student.DivisionID,
            PlaceBirth = student.PlaceBirth ?? string.Empty,
            StudentPhone = student.ApplicationUser?.PhoneNumber ?? string.Empty,
            StudentDOB = student.StudentDOB ?? DateTime.MinValue,
            HireDate = student.ApplicationUser?.HireDate ?? DateTime.MinValue,

            // Attachments and Discounts
            Files = new List<IFormFile>(), // Default empty list
            Amount = student.AccountStudentGuardians?.Sum(asg => asg.Amount) ?? 0,
            Attachments=student.Attachments?.Select(a=> new AttachmentDTO{
            AttachmentID=a.AttachmentID,
            AttachmentURL=a.AttachmentURL!=null?$"{AttachmentUrl}/{a.AttachmentURL}":string.Empty,
            Note=a.Note!,
            VoucherID=a.VoucherID
            }).ToList()?? new List<AttachmentDTO>(),
            Discounts = student.StudentClassFees?.Select(f => new DisCountUpdate
            {
                FeeClassID = f.FeeClassID,
                AmountDiscount = f.AmountDiscount ?? 0,
                NoteDiscount = f.NoteDiscount ?? string.Empty,
                ClassName = f.FeeClass?.Class?.ClassName ?? string.Empty, // Check if FeeClass or Class is null
                FeeName=f.FeeClass?.Fee?.FeeName ?? string.Empty,
                Mandatory=f.FeeClass?.Mandatory ?? false
            }).ToList() ?? new List<DisCountUpdate>()
        };
    }

    public async Task<bool> DeleteStudentAsync(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Load the student along with all related entities
            var student = await _context.Students
                .Include(s => s.TeacherStudents)    // Many-to-many via TeacherStudents
                .Include(s => s.SubjectStudents)    // Many-to-many via SubjectStudents
                .Include(s => s.StudentClassFees)   // Possibly referencing Student
                .Include(s => s.AccountStudentGuardians) // Possibly referencing Student
                .FirstOrDefaultAsync(s => s.StudentID == id);

            if (student == null)
            {
                return false; // No student found with this ID
            }

            // Remove related TeacherStudents if any
            if (student.TeacherStudents != null && student.TeacherStudents.Any())
            {
                _context.TeacherStudents.RemoveRange(student.TeacherStudents);
            }

            // Remove related SubjectStudents if any
            if (student.SubjectStudents != null && student.SubjectStudents.Any())
            {
                _context.SubjectStudents.RemoveRange(student.SubjectStudents);
            }

            // Remove related StudentClassFees if any
            if (student.StudentClassFees != null && student.StudentClassFees.Any())
            {
                _context.StudentClassFees.RemoveRange(student.StudentClassFees);
            }

            // Remove related AccountStudentGuardians if any
            if (student.AccountStudentGuardians != null && student.AccountStudentGuardians.Any())
            {
                _context.AccountStudentGuardians.RemoveRange(student.AccountStudentGuardians);
            }

            // Now that all dependent entities have been removed,
            // we can safely remove the student.
            _context.Students.Remove(student);

            // Save changes
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
             _mangeFilesService.RemoveStudentImage(student.StudentID);
            await _mangeFilesService.RemoveAttachmentsAsync(student.StudentID);

            return true; // Student and related entities successfully deleted
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            // Log the exception as needed
            throw;
        }
}

    public async Task<int> MaxValue()
    {
        var maxValue = await _context.Students.MaxAsync(s => (int?)s.StudentID) ?? 0;
        return maxValue;
    }
}
