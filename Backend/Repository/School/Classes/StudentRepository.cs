using Backend.Data;
using Backend.DTOS.School.Students;
using Backend.Models;
using Backend.Repository.School.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class StudentRepository : IStudentRepository
{
    private readonly DatabaseContext _context;

    public StudentRepository(DatabaseContext context)
    {
        _context = context;
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



    // Update: Update an existing student
public async Task<bool> UpdateStudentAsync(UpdateStudentRequest updateRequest)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // Load the student and related entities
        var student = await _context.Students
            .Include(s => s.ApplicationUser)
            .Include(s => s.Guardian)
            .Include(s => s.StudentClassFees)
            .FirstOrDefaultAsync(s => s.StudentID == updateRequest.StudentID);

        if (student == null)
        {
            return false; // Student not found
        }

        // Update Guardian Details
        if (student.Guardian != null)
        {
            student.Guardian.FullName = updateRequest.GuardianFullName ?? student.Guardian.FullName;
            student.Guardian.Type = updateRequest.GuardianType ?? student.Guardian.Type;
            student.Guardian.GuardianDOB = updateRequest.GuardianDOB ?? student.Guardian.GuardianDOB;

            if (student.Guardian.ApplicationUser != null)
            {
                student.Guardian.ApplicationUser.Email = updateRequest.GuardianEmail ?? student.Guardian.ApplicationUser.Email;
                student.Guardian.ApplicationUser.Address = updateRequest.GuardianAddress ?? student.Guardian.ApplicationUser.Address;
                student.Guardian.ApplicationUser.Gender = updateRequest.GuardianGender ?? student.Guardian.ApplicationUser.Gender;
                student.Guardian.ApplicationUser.PhoneNumber = updateRequest.GuardianPhone ?? student.Guardian.ApplicationUser.PhoneNumber;
            }
        }

        // Update Student Details
        student.FullName.FirstName = updateRequest.StudentFirstName ?? student.FullName.FirstName;
        student.FullName.MiddleName = updateRequest.StudentMiddleName ?? student.FullName.MiddleName;
        student.FullName.LastName = updateRequest.StudentLastName ?? student.FullName.LastName;

        if (student.FullNameAlis != null)
        {
            student.FullNameAlis.FirstNameEng = updateRequest.StudentFirstNameEng ?? student.FullNameAlis.FirstNameEng;
            student.FullNameAlis.MiddleNameEng = updateRequest.StudentMiddleNameEng ?? student.FullNameAlis.MiddleNameEng;
            student.FullNameAlis.LastNameEng = updateRequest.StudentLastNameEng ?? student.FullNameAlis.LastNameEng;
        }

        student.PlaceBirth = updateRequest.PlaceBirth ?? student.PlaceBirth;
        student.StudentDOB = updateRequest.StudentDOB ?? student.StudentDOB;
        student.ImageURL = updateRequest.StudentImageURL ?? student.ImageURL;
        student.DivisionID = updateRequest.DivisionID ?? student.DivisionID;

        if (student.ApplicationUser != null)
        {
            student.ApplicationUser.Email = updateRequest.StudentEmail ?? student.ApplicationUser.Email;
            student.ApplicationUser.Address = updateRequest.StudentAddress ?? student.ApplicationUser.Address;
            student.ApplicationUser.Gender = updateRequest.StudentGender ?? student.ApplicationUser.Gender;
            student.ApplicationUser.HireDate = updateRequest.HireDate ?? student.ApplicationUser.HireDate;
            student.ApplicationUser.PhoneNumber = updateRequest.StudentPhone ?? student.ApplicationUser.PhoneNumber;
        }

        // Update Discounts
        if (updateRequest.Discounts != null && updateRequest.Discounts.Any())
        {
            foreach (var discount in updateRequest.Discounts)
            {
                var existingFee = student.StudentClassFees.FirstOrDefault(f => f.FeeClassID == discount.FeeClassID);
                if (existingFee != null)
                {
                    existingFee.AmountDiscount = discount.AmountDiscount ?? existingFee.AmountDiscount;
                    existingFee.NoteDiscount = discount.NoteDiscount ?? existingFee.NoteDiscount;
                }
                else
                {
                    var newFee = new StudentClassFees
                    {
                        StudentID = student.StudentID,
                        FeeClassID = discount.FeeClassID,
                        AmountDiscount = discount.AmountDiscount,
                        NoteDiscount = discount.NoteDiscount
                    };
                    _context.StudentClassFees.Add(newFee);
                }
            }
        }

        // Update Attachments
        if (updateRequest.Attachments != null && updateRequest.Attachments.Any())
        {
            _context.Attachments.RemoveRange(student.Attachments);
            foreach (var file in updateRequest.Attachments)
            {
                _context.Attachments.Add(new Attachments
                {
                    StudentID = student.StudentID,
                    AttachmentURL = file
                });
            }
        }

        // Save changes and commit transaction
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return true;
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        // Log exception as needed
        throw new Exception("An error occurred while updating the student.", ex);
    }
}

    // Delete: Delete a student by ID with Cascade Delete
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

            return true; // Student and related entities successfully deleted
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            // Log the exception as needed
            throw;
        }
}


    // Get MaxValue: Get the maximum value of StudentID
    public async Task<int> MaxValue()
    {
        var maxValue = await _context.Students.MaxAsync(s => (int?)s.StudentID) ?? 0;
        return maxValue;
    }
}
