using Backend.Data;
using Backend.DTOS;
using Backend.DTOS.School.Attachments;
using Backend.DTOS.School.Students;
using Backend.Models;
using Backend.Repository.School.Implements;
using Backend.Repository.School.Interfaces;
using Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class StudentRepository : IStudentRepository
{
    private readonly TenantDbContext _context;
    private readonly IGuardianRepository _guardianRepo;
    private readonly mangeFilesService _mangeFilesService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<StudentRepository> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public StudentRepository(TenantDbContext context, IGuardianRepository guardianRepo, mangeFilesService mangeFilesService, IUserRepository userRepository, ILogger<StudentRepository> logger, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _guardianRepo = guardianRepo;
        _mangeFilesService = mangeFilesService;
        _userRepository = userRepository;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    // Helper method to get base URL dynamically
    private string GetBaseUrl()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request == null)
        {
            // Fallback if HttpContext is not available
            return "https://localhost:7258";
        }

        var scheme = request.Scheme;
        var host = request.Host.Value;
        return $"{scheme}://{host}";
    }

    // Create: Add a new student
    public async Task<Student> AddStudentAsync(Student student)
    {
        // Always auto-generate StudentID to avoid duplicate key errors
        // Since StudentID is configured with ValueGeneratedNever(), we must calculate it manually
        // Retry logic to handle race conditions and ensure unique ID
        const int maxRetries = 5;
        int retryCount = 0;
        
        while (retryCount < maxRetries)
        {
            int nextId = 0;
            int maxValue = 0;
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Always recalculate max value to ensure we get the latest
                maxValue = await _context.Students.MaxAsync(s => (int?)s.StudentID) ?? 0;
                
                // Calculate next ID, but also check for gaps in case of deletions
                nextId = maxValue + 1;
                
                // Double-check this ID doesn't exist (handles race conditions and gaps)
                var exists = await _context.Students.AnyAsync(s => s.StudentID == nextId);
                int attempts = 0;
                while (exists && attempts < 10) // Try up to 10 times to find a free ID
                {
                    nextId++;
                    exists = await _context.Students.AnyAsync(s => s.StudentID == nextId);
                    attempts++;
                }
                
                if (exists)
                {
                    // Still exists after checking - this shouldn't happen, but handle it
                    _logger.LogWarning("Could not find available StudentID after {Attempts} attempts. MaxValue: {MaxValue}, LastNextId: {NextId}", attempts, maxValue, nextId);
                    await transaction.RollbackAsync();
                    retryCount++;
                    continue;
                }
                
                // Explicitly set the StudentID to ensure it's updated
                student.StudentID = nextId;
                
                _logger.LogInformation("Attempting to add student with StudentID: {StudentID}, MaxValue was: {MaxValue}", student.StudentID, maxValue);
                Console.WriteLine($"[LOG] Attempting to add student with StudentID: {student.StudentID}, MaxValue was: {maxValue}");
                
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                _logger.LogInformation("Successfully added student with StudentID: {StudentID}", student.StudentID);
                Console.WriteLine($"[LOG] Successfully added student with StudentID: {student.StudentID}");
                break; // Success, exit retry loop
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException?.Message?.Contains("duplicate key") == true || ex.InnerException?.Message?.Contains("PRIMARY KEY constraint") == true)
            {
                // Duplicate key error - rollback and retry
                _logger.LogWarning(ex, 
                    "Duplicate key error when adding student. Attempt {RetryCount}/{MaxRetries}. StudentID: {StudentID}, MaxValue: {MaxValue}, NextId: {NextId}. Inner Exception: {InnerException}", 
                    retryCount + 1, maxRetries, student.StudentID, maxValue, nextId, ex.InnerException?.Message);
                Console.WriteLine($"[WARNING] Duplicate key error - Attempt {retryCount + 1}/{maxRetries}. StudentID: {student.StudentID}, MaxValue: {maxValue}, NextId: {nextId}. Error: {ex.InnerException?.Message}");
                
                await transaction.RollbackAsync();
                retryCount++;
                if (retryCount >= maxRetries)
                {
                    _logger.LogError(ex, "Max retries reached for duplicate key error. StudentID: {StudentID}, MaxValue: {MaxValue}, NextId: {NextId}", student.StudentID, maxValue, nextId);
                    throw; // Re-throw if max retries reached
                }
            }
            catch (Exception ex)
            {
                // Log detailed error information
                var innerException = ex.InnerException;
                var errorDetails = new System.Text.StringBuilder();
                errorDetails.AppendLine($"Error Type: {ex.GetType().FullName}");
                errorDetails.AppendLine($"Error Message: {ex.Message}");
                
                if (innerException != null)
                {
                    errorDetails.AppendLine($"Inner Exception Type: {innerException.GetType().FullName}");
                    errorDetails.AppendLine($"Inner Exception Message: {innerException.Message}");
                    if (!string.IsNullOrEmpty(innerException.StackTrace))
                        errorDetails.AppendLine($"Inner Exception Stack Trace: {innerException.StackTrace}");
                }
                
                if (!string.IsNullOrEmpty(ex.StackTrace))
                    errorDetails.AppendLine($"Stack Trace: {ex.StackTrace}");
                
                errorDetails.AppendLine($"StudentID Attempted: {student.StudentID}");
                errorDetails.AppendLine($"MaxValue: {maxValue}");
                errorDetails.AppendLine($"NextId Calculated: {nextId}");
                
                _logger.LogError(ex, "Error adding student: {ErrorDetails}", errorDetails.ToString());
                Console.WriteLine($"[ERROR] Error adding student: {errorDetails.ToString()}");
                
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Add student to manthly and termly grades
        student = await _context.Students
        .Include(s => s.Division)
            .ThenInclude(d => d.Class)
        .FirstOrDefaultAsync(s => s.StudentID == student.StudentID) ?? student;

        var plans = await _context.CoursePlans
            .Where(p => p.DivisionID == student.DivisionID &&
                        p.YearID == student.Division.Class.YearID &&
                        p.ClassID == student.Division.ClassID)
            .ToListAsync();

        // 3) بيانات مشتركة
        var months = await _context.YearTermMonths
            .Where(m => m.YearID == student.Division.Class.YearID)
            .Select(m => new { m.MonthID, m.TermID })
            .ToListAsync();

        var gradeTypes = await _context.GradeTypes
            .Where(g => g.IsActive)
            .Select(g => g.GradeTypeID)
            .ToListAsync();

        var monthly = new List<MonthlyGrade>();
        var termly = new List<TermlyGrade>();

        foreach (var plan in plans)
        {
            // Termly
            termly.Add(new TermlyGrade
            {
                StudentID = student.StudentID,
                YearID = plan.YearID,
                TermID = plan.TermID,
                ClassID = plan.ClassID,
                SubjectID = plan.SubjectID
            });

            // Monthly
            foreach (var m in months.Where(m => m.TermID == plan.TermID))
                foreach (var gt in gradeTypes)
                    monthly.Add(new MonthlyGrade
                    {
                        StudentID = student.StudentID,
                        YearID = plan.YearID,
                        TermID = plan.TermID,
                        ClassID = plan.ClassID,
                        SubjectID = plan.SubjectID,
                        MonthID = m.MonthID,
                        GradeTypeID = gt
                    });
        }

        if (monthly.Count > 0) _context.MonthlyGrades.AddRange(monthly);
        if (termly.Count > 0) _context.TermlyGrades.AddRange(termly);
        await _context.SaveChangesAsync();

        return student;
    }

    // Read: Get all students
    public async Task<StudentDetailsDTO?> GetStudentByIdAsync(int id)
    {
        var student = await _context.Students
            .Include(s => s.Division)
            .FirstOrDefaultAsync(s => s.StudentID == id);

        if (student == null)
            return null;

        // Fetch user data from admin database
        var user = await _userRepository.GetUserByIdAsync(student.UserID);

        string baseUrl = $"{GetBaseUrl()}/uploads/StudentPhotos";

        return new StudentDetailsDTO
        {
            StudentID = student.StudentID,
            FullName = new NameDTO
            {
                FirstName = student.FullName.FirstName,
                MiddleName = student.FullName.MiddleName!,
                LastName = student.FullName.LastName
            },
            PhotoUrl = student.ImageURL != null
                ? $"{baseUrl}/{student.ImageURL}"
                : $"{baseUrl}/default-placeholder.png",
            DivisionID = student.DivisionID,
            PlaceBirth = student.PlaceBirth,
            UserID = student.UserID,
            ApplicationUser = user != null ? new ApplicationUserDTO
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Gender = user.Gender
            } : null
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
            .Include(s => s.Division)        // Include Division details
                .ThenInclude(d => d.Class)   // Include Class details
                    .ThenInclude(c => c.Stage) // Include Stage details
            .Include(ASG => ASG.AccountStudentGuardians)
            .Include(s => s.Guardian)        // Include Guardian details
            .ToListAsync();

        if (students == null || !students.Any())
            return new List<StudentDetailsDTO>();

        // Fetch all user data in batch from admin database
        var userIds = students.Select(s => s.UserID).Distinct().ToList();
        var users = new Dictionary<string, ApplicationUser>();
        foreach (var userId in userIds)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user != null)
            {
                users[userId] = user;
            }
        }

        string baseUrl = $"{GetBaseUrl()}/uploads/StudentPhotos";

        return MapStudentsToDTOs(students, users, baseUrl);
    }

    private List<StudentDetailsDTO> MapStudentsToDTOs(List<Student> students, Dictionary<string, ApplicationUser> users, string baseUrl)
    {
        return students.Select(student =>
        {
            var user = users.ContainsKey(student.UserID) ? users[student.UserID] : null;
            return new StudentDetailsDTO
            {
                StudentID = student.StudentID,
                FullName = new NameDTO
                {
                    FirstName = student.FullName.FirstName,
                    MiddleName = student.FullName.MiddleName!,
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
                Gender = user?.Gender,
                HireDate = user?.HireDate ?? DateTime.MinValue,
                PlaceBirth = student.PlaceBirth,
                Fee = student.AccountStudentGuardians?.Sum(asg => asg.Amount) ?? 0, // Aggregate Fee Amount
                StudentPhone = user?.PhoneNumber,
                StudentAddress = user?.Address,
                UserID = student.UserID,
                ApplicationUser = user != null ? new ApplicationUserDTO
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Gender = user.Gender
                } : null,
                Guardians = user != null ? new GuardianDto
                {
                    guardianID = student.GuardianID,
                    guardianFullName = student.Guardian?.FullName ?? string.Empty,
                    guardianType = student.Guardian?.Type ?? string.Empty,
                    guardianEmail = user.Email ?? string.Empty,
                    guardianPhone = user.PhoneNumber ?? string.Empty,
                    guardianDOB = user.HireDate,
                    guardianAddress = user.Address ?? string.Empty
                } : null
            };
        }).ToList();
    }

    public async Task<(List<StudentDetailsDTO> Items, int TotalCount)> GetStudentsPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        // Get total count first (before pagination)
        var totalCount = await _context.Students.CountAsync(cancellationToken);

        if (totalCount == 0)
            return (new List<StudentDetailsDTO>(), 0);

        var students = await _context.Students
            .Include(s => s.Division)        // Include Division details
                .ThenInclude(d => d.Class)   // Include Class details
                    .ThenInclude(c => c.Stage) // Include Stage details
            .Include(ASG => ASG.AccountStudentGuardians)
            .Include(s => s.Guardian)        // Include Guardian details
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (students == null || !students.Any())
            return (new List<StudentDetailsDTO>(), totalCount);

        // Fetch all user data in batch from admin database
        var userIds = students.Select(s => s.UserID).Distinct().ToList();
        var users = new Dictionary<string, ApplicationUser>();
        foreach (var userId in userIds)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user != null)
            {
                users[userId] = user;
            }
        }

        string baseUrl = $"{GetBaseUrl()}/uploads/StudentPhotos";

        var items = MapStudentsToDTOs(students, users, baseUrl);
        return (items, totalCount);
    }

    public async Task<GetStudentForUpdateDTO?> GetUpdateStudentWithGuardianRequestData(int studentId)
    {
        var student = await _context.Students
            .Include(s => s.Guardian)
            .Include(s => s.Attachments)
            .Include(s => s.Division)
             .Include(s => s.StudentClassFees)
                .ThenInclude(f => f.FeeClass)
                  .ThenInclude(fc => fc.Class)
            .FirstOrDefaultAsync(s => s.StudentID == studentId);

        if (student == null)
            return null;

        // Fetch user data from admin database
        var user = await _userRepository.GetUserByIdAsync(student.UserID);

        string baseUrl = GetBaseUrl();
        string PhotoUrl = $"{baseUrl}/uploads/StudentPhotos";
        string AttachmentUrl = $"{baseUrl}/uploads/Attachments";
        
        // Await the async call properly instead of using .Result
        var guardianInfo = await _guardianRepo.GetGuardianByIdForUpdateAsync(student!.GuardianID);

        var guardian = student.Guardian;

        return new GetStudentForUpdateDTO
        {
            // Guardian Details
            ExistingGuardianId = guardian?.GuardianID,
            GuardianEmail = guardianInfo.GuardianEmail!,
            GuardianPassword = string.Empty,
            GuardianAddress = guardianInfo.GuardianAddress!,
            GuardianGender = guardianInfo.Gender!,
            GuardianFullName = guardianInfo.FullName,
            GuardianType = guardianInfo.Type!,
            GuardianPhone = guardianInfo.GuardianPhone!,
            GuardianDOB = guardianInfo.GuardianDOB,

            // Student Details
            StudentID = student.StudentID,
            StudentEmail = user?.Email ?? string.Empty,
            StudentPassword = string.Empty, // Passwords are not retrievable
            StudentAddress = user?.Address ?? string.Empty,
            StudentGender = user?.Gender ?? "Not Specified",
            StudentFirstName = student.FullName?.FirstName ?? string.Empty,
            StudentMiddleName = student.FullName?.MiddleName ?? string.Empty,
            StudentLastName = student.FullName?.LastName ?? string.Empty,
            StudentFirstNameEng = student.FullNameAlis?.FirstNameEng ?? string.Empty,
            StudentMiddleNameEng = student.FullNameAlis?.MiddleNameEng ?? string.Empty,
            StudentLastNameEng = student.FullNameAlis?.LastNameEng ?? string.Empty,
            StudentImageURL = student.ImageURL != null ? $"{PhotoUrl}/{student.ImageURL}" : string.Empty,
            ClassID = student.Division?.ClassID ?? 0,
            DivisionID = student.DivisionID,
            PlaceBirth = student.PlaceBirth ?? string.Empty,
            StudentPhone = user?.PhoneNumber ?? string.Empty,
            StudentDOB = student.StudentDOB ?? DateTime.MinValue,
            HireDate = user?.HireDate ?? DateTime.MinValue,

            // Attachments and Discounts
            Files = new List<IFormFile>(), // Default empty list
            Amount = student.AccountStudentGuardians?.Sum(asg => asg.Amount) ?? 0,
            Attachments = student.Attachments?.Select(a => new AttachmentDTO
            {
                AttachmentID = a.AttachmentID,
                AttachmentURL = a.AttachmentURL != null ? $"{AttachmentUrl}/{a.AttachmentURL}" : string.Empty,
                VoucherID = a.VoucherID
            }).ToList() ?? new List<AttachmentDTO>(),
            Discounts = student.StudentClassFees?.Select(f => new DisCountUpdate
            {
                StudentClassFeeID = f.StudentClassFeesID,
                FeeClassID = f.FeeClassID,
                AmountDiscount = f.AmountDiscount ?? 0,
                NoteDiscount = f.NoteDiscount ?? string.Empty,
                ClassName = f.FeeClass?.Class?.ClassName ?? string.Empty, // Check if FeeClass or Class is null
                FeeName = f.FeeClass?.Fee?.FeeName ?? string.Empty,
                Mandatory = f.Mandatory ?? false
            }).ToList() ?? new List<DisCountUpdate>()
        };
    }

    public async Task<bool> DeleteStudentAsync(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Load student along with ALL referencing entities
            var student = await _context.Students
                .Include(s => s.StudentClassFees)
                .Include(s => s.AccountStudentGuardians)
                .Include(s => s.TermlyGrades) // <-- Add this to fix your issue
                .Include(s => s.MonthlyGrades) // Highly recommended, similar potential FK
                .FirstOrDefaultAsync(s => s.StudentID == id);

            if (student == null)
                return false;

            // Explicitly delete referencing rows first
            if (student.StudentClassFees.Any())
                _context.StudentClassFees.RemoveRange(student.StudentClassFees);

            if (student.AccountStudentGuardians.Any())
                _context.AccountStudentGuardians.RemoveRange(student.AccountStudentGuardians);

            if (student.TermlyGrades.Any())
                _context.TermlyGrades.RemoveRange(student.TermlyGrades);

            if (student.MonthlyGrades.Any())
                _context.MonthlyGrades.RemoveRange(student.MonthlyGrades);

            // Finally delete the student itself
            _context.Students.Remove(student);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _mangeFilesService.RemoveFile("StudentPhotos", student.StudentID);
            await _mangeFilesService.RemoveAttachmentsAsync("Attachments", student.StudentID);

            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            // Helpful: Log inner exceptions clearly for troubleshooting
            var baseException = ex.GetBaseException();
            Console.WriteLine($"Deletion failed: {baseException.Message}");
            throw;
        }
    }

    public async Task<int> MaxValue()
    {
        var maxValue = await _context.Students.MaxAsync(s => (int?)s.StudentID) ?? 0;
        return maxValue;
    }
}
