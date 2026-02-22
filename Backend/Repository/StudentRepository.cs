using Backend.Common;
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

        // Add student to monthly and termly grades
        student = await _context.Students
            .Include(s => s.Division)
                .ThenInclude(d => d.Class)
                    .ThenInclude(c => c.Year)
            .Include(s => s.Division)
                .ThenInclude(d => d.Class)
                    .ThenInclude(c => c.Stage)
                        .ThenInclude(s => s.Year)
            .FirstOrDefaultAsync(s => s.StudentID == student.StudentID) ?? student;

        if (student.Division == null || student.Division.Class == null)
        {
            _logger.LogWarning("Student {StudentID} has no division or class. Cannot create grades.", student.StudentID);
            return student;
        }

        // Get the year ID (either from Class.YearID or Class.Stage.YearID)
        var yearID = student.Division.Class.YearID ?? 
                    (student.Division.Class.Stage?.YearID);

        if (!yearID.HasValue)
        {
            _logger.LogWarning("Student {StudentID} has no associated year. Cannot create grades.", student.StudentID);
            return student;
        }

        var classID = student.Division.ClassID;

        _logger.LogInformation("Creating grades for new student {StudentID} in DivisionID: {DivisionID}, ClassID: {ClassID}, YearID: {YearID}",
            student.StudentID, student.DivisionID, classID, yearID.Value);

        var plans = await _context.CoursePlans
            .Where(p => p.DivisionID == student.DivisionID &&
                        p.YearID == yearID.Value &&
                        p.ClassID == classID)
            .ToListAsync();

        _logger.LogInformation("Found {Count} course plans for new student {StudentID}", plans.Count, student.StudentID);

        if (plans.Count == 0)
        {
            _logger.LogWarning("No course plans found for new student {StudentID} in DivisionID: {DivisionID}, ClassID: {ClassID}, YearID: {YearID}. Grades will not be created.",
                student.StudentID, student.DivisionID, classID, yearID.Value);
            return student;
        }

        // Get months for the year
        var months = await _context.YearTermMonths
            .Where(m => m.YearID == yearID.Value)
            .Select(m => new { m.MonthID, m.TermID })
            .ToListAsync();

        // If no months found, try to create default months
        if (months.Count == 0)
        {
            _logger.LogWarning("No YearTermMonths found for YearID: {YearID}. Creating default months.", yearID.Value);
            
            var defaultMonths = new List<YearTermMonth>
            {
                new YearTermMonth { YearID = yearID.Value, TermID = 1, MonthID = 5 },
                new YearTermMonth { YearID = yearID.Value, TermID = 1, MonthID = 6 },
                new YearTermMonth { YearID = yearID.Value, TermID = 1, MonthID = 7 },
                new YearTermMonth { YearID = yearID.Value, TermID = 1, MonthID = 8 },
                new YearTermMonth { YearID = yearID.Value, TermID = 2, MonthID = 9 },
                new YearTermMonth { YearID = yearID.Value, TermID = 2, MonthID = 10 },
                new YearTermMonth { YearID = yearID.Value, TermID = 2, MonthID = 11 },
                new YearTermMonth { YearID = yearID.Value, TermID = 2, MonthID = 12 }
            };

            await _context.YearTermMonths.AddRangeAsync(defaultMonths);
            await _context.SaveChangesAsync();

            months = await _context.YearTermMonths
                .Where(m => m.YearID == yearID.Value)
                .Select(m => new { m.MonthID, m.TermID })
                .ToListAsync();
        }

        var gradeTypes = await _context.GradeTypes
            .Where(g => g.IsActive)
            .Select(g => g.GradeTypeID)
            .ToListAsync();

        _logger.LogInformation("Found {MonthCount} months and {GradeTypeCount} grade types for YearID: {YearID}",
            months.Count, gradeTypes.Count, yearID.Value);

        if (months.Count == 0 || gradeTypes.Count == 0)
        {
            _logger.LogWarning("Cannot create grades for student {StudentID}. Months: {MonthCount}, GradeTypes: {GradeTypeCount}",
                student.StudentID, months.Count, gradeTypes.Count);
            return student;
        }

        // Bulk fetch existing grades to avoid duplicates
        var existingTermlyGrades = await _context.TermlyGrades
            .Where(tg => tg.StudentID == student.StudentID &&
                        tg.YearID == yearID.Value &&
                        tg.ClassID == classID)
            .Select(tg => new { tg.StudentID, tg.YearID, tg.TermID, tg.ClassID, tg.SubjectID })
            .ToListAsync();

        var existingMonthlyGrades = await _context.MonthlyGrades
            .Where(mg => mg.StudentID == student.StudentID &&
                        mg.YearID == yearID.Value &&
                        mg.ClassID == classID)
            .Select(mg => new { mg.StudentID, mg.YearID, mg.TermID, mg.ClassID, mg.SubjectID, mg.MonthID, mg.GradeTypeID })
            .ToListAsync();

        var monthly = new List<MonthlyGrade>();
        var termly = new List<TermlyGrade>();

        foreach (var plan in plans)
        {
            // Check if TermlyGrade already exists
            var termlyExists = existingTermlyGrades.Any(tg => 
                tg.StudentID == student.StudentID &&
                tg.YearID == yearID.Value &&
                tg.TermID == plan.TermID &&
                tg.ClassID == classID &&
                tg.SubjectID == plan.SubjectID);

            if (!termlyExists)
            {
                termly.Add(new TermlyGrade
                {
                    StudentID = student.StudentID,
                    YearID = yearID.Value,
                    TermID = plan.TermID,
                    ClassID = classID,
                    SubjectID = plan.SubjectID,
                    Grade = null,
                    Note = null
                });
            }

            // Create MonthlyGrade for each month and grade type
            foreach (var m in months.Where(m => m.TermID == plan.TermID))
            {
                foreach (var gt in gradeTypes)
                {
                    var monthlyExists = existingMonthlyGrades.Any(mg => 
                        mg.StudentID == student.StudentID &&
                        mg.YearID == yearID.Value &&
                        mg.TermID == plan.TermID &&
                        mg.ClassID == classID &&
                        mg.SubjectID == plan.SubjectID &&
                        mg.MonthID == m.MonthID &&
                        mg.GradeTypeID == gt);

                    if (!monthlyExists)
                    {
                        monthly.Add(new MonthlyGrade
                        {
                            StudentID = student.StudentID,
                            YearID = yearID.Value,
                            TermID = plan.TermID,
                            ClassID = classID,
                            SubjectID = plan.SubjectID,
                            MonthID = m.MonthID,
                            GradeTypeID = gt,
                            Grade = null
                        });
                    }
                }
            }
        }

        _logger.LogInformation("Prepared {TermlyCount} TermlyGrades and {MonthlyCount} MonthlyGrades for new student {StudentID}",
            termly.Count, monthly.Count, student.StudentID);

        if (monthly.Count > 0)
        {
            await _context.MonthlyGrades.AddRangeAsync(monthly);
            _logger.LogInformation("Added {Count} MonthlyGrades to context for student {StudentID}", monthly.Count, student.StudentID);
        }

        if (termly.Count > 0)
        {
            await _context.TermlyGrades.AddRangeAsync(termly);
            _logger.LogInformation("Added {Count} TermlyGrades to context for student {StudentID}", termly.Count, student.StudentID);
        }

        if (monthly.Count > 0 || termly.Count > 0)
        {
            var savedCount = await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Successfully saved {SavedCount} grade records for new student {StudentID}. TermlyGrades: {TermlyCount}, MonthlyGrades: {MonthlyCount}",
                savedCount, student.StudentID, termly.Count, monthly.Count);

            // Verify the grades were actually saved
            var savedTermlyCount = await _context.TermlyGrades
                .CountAsync(tg => tg.StudentID == student.StudentID && 
                                 tg.YearID == yearID.Value && 
                                 tg.ClassID == classID);
            
            var savedMonthlyCount = await _context.MonthlyGrades
                .CountAsync(mg => mg.StudentID == student.StudentID && 
                                mg.YearID == yearID.Value && 
                                mg.ClassID == classID);
            
            _logger.LogInformation("Verified saved grades for new student {StudentID}: {TermlyCount} TermlyGrades, {MonthlyCount} MonthlyGrades in database",
                student.StudentID, savedTermlyCount, savedMonthlyCount);
        }
        else
        {
            _logger.LogWarning("⚠️ No grades to save for new student {StudentID}. All grades may already exist.",
                student.StudentID);
        }

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
                    .ThenInclude(c => c.Year) // Include Class direct Year
            .Include(s => s.Division)
                .ThenInclude(d => d.Class)
                    .ThenInclude(c => c.Stage) // Include Stage details
                        .ThenInclude(s => s.Year) // Include Stage Year
            .Include(ASG => ASG.AccountStudentGuardians)
            .Include(s => s.Guardian)        // Include Guardian details
            .Where(s => s.Division != null && 
                      s.Division.Class != null && 
                      ((s.Division.Class.Year != null && s.Division.Class.Year.Active == true) || 
                       (s.Division.Class.Stage != null && s.Division.Class.Stage.Year != null && s.Division.Class.Stage.Year.Active == true)))
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
        // Base query with filtering for active year
        var baseQuery = _context.Students
            .Include(s => s.Division)
                .ThenInclude(d => d.Class)
                    .ThenInclude(c => c.Year)
            .Include(s => s.Division)
                .ThenInclude(d => d.Class)
                    .ThenInclude(c => c.Stage)
                        .ThenInclude(s => s.Year)
            .Where(s => s.Division != null && 
                       s.Division.Class != null && 
                       ((s.Division.Class.Year != null && s.Division.Class.Year.Active == true) || 
                        (s.Division.Class.Stage != null && s.Division.Class.Stage.Year != null && s.Division.Class.Stage.Year.Active == true)));

        // Get total count with filters applied
        var totalCount = await baseQuery.CountAsync(cancellationToken);

        if (totalCount == 0)
            return (new List<StudentDetailsDTO>(), 0);

        var students = await baseQuery
            .Include(ASG => ASG.AccountStudentGuardians)
            .Include(s => s.Guardian)
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

    public async Task<(List<StudentDetailsDTO> Items, int TotalCount)> GetStudentsPageWithFiltersAsync(int pageNumber, int pageSize, Dictionary<string, Backend.Common.FilterValue> filters, CancellationToken cancellationToken = default)
    {
        // Start with base query - include all necessary relationships first
        var query = _context.Students
            .Include(s => s.Division)
                .ThenInclude(d => d.Class)
                    .ThenInclude(c => c.Year)
            .Include(s => s.Division)
                .ThenInclude(d => d.Class)
                    .ThenInclude(c => c.Stage)
                        .ThenInclude(s => s.Year)
            .Include(ASG => ASG.AccountStudentGuardians)
            .Include(s => s.Guardian)
            .Where(s => s.Division != null && 
                       s.Division.Class != null && 
                       ((s.Division.Class.Year != null && s.Division.Class.Year.Active == true) || 
                        (s.Division.Class.Stage != null && s.Division.Class.Stage.Year != null && s.Division.Class.Stage.Year.Active == true)))
            .AsQueryable();

        // Apply filters dynamically
        foreach (var filter in filters)
        {
            var columnName = filter.Key;
            var filterValue = filter.Value;

            query = columnName.ToLower() switch
            {
                "userid" or "userId" => !string.IsNullOrEmpty(filterValue.Value)
                    ? query.Where(s => s.UserID == filterValue.Value)
                    : query,
                "studentid" or "studentId" => filterValue.IntValue.HasValue 
                    ? query.Where(s => s.StudentID == filterValue.IntValue.Value)
                    : query,
                "divisionid" or "divisionId" => filterValue.IntValue.HasValue
                    ? query.Where(s => s.DivisionID == filterValue.IntValue.Value)
                    : query,
                "guardianid" or "guardianId" => filterValue.IntValue.HasValue
                    ? query.Where(s => s.GuardianID == filterValue.IntValue.Value)
                    : query,
                "placebirth" or "placeBirth" => !string.IsNullOrEmpty(filterValue.Value)
                    ? query.Where(s => s.PlaceBirth != null && s.PlaceBirth.Contains(filterValue.Value))
                    : query,
                "studentdob" or "studentDob" => filterValue.DateValue.HasValue
                    ? query.Where(s => s.StudentDOB.HasValue && s.StudentDOB.Value.Date == filterValue.DateValue.Value.Date)
                    : query,
                "firstname" or "firstName" => !string.IsNullOrEmpty(filterValue.Value)
                    ? query.Where(s => s.FullName.FirstName.Contains(filterValue.Value))
                    : query,
                "lastname" or "lastName" => !string.IsNullOrEmpty(filterValue.Value)
                    ? query.Where(s => s.FullName.LastName.Contains(filterValue.Value))
                    : query,
                "middlename" or "middleName" => !string.IsNullOrEmpty(filterValue.Value)
                    ? query.Where(s => s.FullName.MiddleName != null && s.FullName.MiddleName.Contains(filterValue.Value))
                    : query,
                _ => query // Unknown filter, ignore it
            };
        }

        // Get total count with filters applied
        var totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
            return (new List<StudentDetailsDTO>(), 0);

        // Apply pagination
        var students = await query
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

    public async Task<(List<UnregisteredStudentDTO> Items, int TotalCount)> GetUnregisteredStudentsAsync(
        int? targetYearID, 
        int pageNumber, 
        int pageSize, 
        string? studentName, 
        int? stageID, 
        CancellationToken cancellationToken = default)
    {
        // Get current active year
        var currentYear = await _context.Years
            .Where(y => y.Active == true)
            .OrderByDescending(y => y.YearID)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentYear == null)
            return (new List<UnregisteredStudentDTO>(), 0);

        // If targetYearID is not provided, get the next year (or create logic to determine target year)
        int? targetYear = targetYearID;
        if (!targetYear.HasValue)
        {
            // Get next year or most recent inactive year
            targetYear = await _context.Years
                .Where(y => !y.Active && y.YearID > currentYear.YearID)
                .OrderBy(y => y.YearID)
                .Select(y => (int?)y.YearID)
                .FirstOrDefaultAsync(cancellationToken);
        }

        // Base query: Get students in current active year
        var baseQuery = _context.Students
            .Include(s => s.Division)
                .ThenInclude(d => d.Class)
                    .ThenInclude(c => c.Year)
            .Include(s => s.Division)
                .ThenInclude(d => d.Class)
                    .ThenInclude(c => c.Stage)
                        .ThenInclude(s => s.Year)
            .Where(s => s.Division != null && 
                       s.Division.Class != null && 
                       ((s.Division.Class.Year != null && s.Division.Class.Year.YearID == currentYear.YearID) ||
                        (s.Division.Class.Stage != null && s.Division.Class.Stage.Year != null && s.Division.Class.Stage.Year.YearID == currentYear.YearID)));

        // Apply filters
        if (!string.IsNullOrWhiteSpace(studentName))
        {
            baseQuery = baseQuery.Where(s => 
                (s.FullName.FirstName + " " + s.FullName.MiddleName + " " + s.FullName.LastName)
                .Contains(studentName));
        }

        if (stageID.HasValue)
        {
            baseQuery = baseQuery.Where(s => 
                s.Division.Class.StageID == stageID.Value);
        }

        // Get total count
        var totalCount = await baseQuery.CountAsync(cancellationToken);

        // Get paginated results
        var students = await baseQuery
            .OrderBy(s => s.FullName.FirstName)
            .ThenBy(s => s.FullName.MiddleName)
            .ThenBy(s => s.FullName.LastName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var items = students.Select(s => new UnregisteredStudentDTO
        {
            StudentID = s.StudentID,
            StudentName = (s.FullName?.FirstName ?? "") + " " + 
                        (s.FullName?.MiddleName ?? "") + " " + 
                        (s.FullName?.LastName ?? ""),
            CurrentClassName = s.Division?.Class?.ClassName,
            CurrentStageName = s.Division?.Class?.Stage?.StageName,
            CurrentDivisionName = s.Division?.DivisionName,
            CurrentDivisionID = s.DivisionID,
            CurrentYearID = s.Division?.Class?.YearID ?? 
                          (s.Division?.Class?.Stage?.YearID)
        }).ToList();

        return (items, totalCount);
    }

    public async Task<PromoteStudentsResponseDTO> PromoteStudentsAsync(List<PromoteStudentRequestDTO> students, int? targetYearID = null)
    {
        var response = new PromoteStudentsResponseDTO
        {
            TotalCount = students.Count,
            Results = new List<PromoteStudentResultDTO>()
        };

        // Get target year (use provided year, or next year after current active year)
        Year? targetYear = null;
        if (targetYearID.HasValue)
        {
            targetYear = await _context.Years.FirstOrDefaultAsync(y => y.YearID == targetYearID.Value);
        }
        else
        {
            // Get current active year first
            var currentYear = await _context.Years.FirstOrDefaultAsync(y => y.Active);
            
            if (currentYear != null)
            {
                // Get the next year (year after current year)
                // First try to find an inactive year with YearID greater than current year
                targetYear = await _context.Years
                    .Where(y => !y.Active && y.YearID > currentYear.YearID)
                    .OrderBy(y => y.YearID)
                    .FirstOrDefaultAsync();
                
                // If no inactive year found, try to find any year with YearID greater than current year
                if (targetYear == null)
                {
                    targetYear = await _context.Years
                        .Where(y => y.YearID > currentYear.YearID)
                        .OrderBy(y => y.YearID)
                        .FirstOrDefaultAsync();
                }
                
                _logger.LogInformation("Current active year: {CurrentYearID}, Target year (next year): {TargetYearID}", 
                    currentYear.YearID, targetYear?.YearID);
            }
            else
            {
                // If no active year, use active year as fallback
                targetYear = await _context.Years.FirstOrDefaultAsync(y => y.Active);
            }
        }

        if (targetYear == null)
        {
            // If no target year found, return error for all students
            foreach (var studentRequest in students)
            {
                var result = new PromoteStudentResultDTO
                {
                    StudentID = studentRequest.StudentID,
                    NewDivisionID = studentRequest.NewDivisionID,
                    Success = false,
                    ErrorMessage = "لم يتم العثور على سنة دراسية للترقية. يرجى التأكد من وجود سنة دراسية بعد السنة الحالية"
                };
                response.Results.Add(result);
                response.FailedCount++;
            }
            return response;
        }

        // Process each student individually to allow partial success
        foreach (var studentRequest in students)
        {
            var result = new PromoteStudentResultDTO
            {
                StudentID = studentRequest.StudentID,
                NewDivisionID = studentRequest.NewDivisionID
            };

            try
            {
                // Get student with related data
                var student = await _context.Students
                    .Include(s => s.FullName)
                    .Include(s => s.Division)
                        .ThenInclude(d => d.Class)
                            .ThenInclude(c => c.Year)
                    .Include(s => s.Division)
                        .ThenInclude(d => d.Class)
                            .ThenInclude(c => c.Stage)
                                .ThenInclude(st => st.Year)
                    .FirstOrDefaultAsync(s => s.StudentID == studentRequest.StudentID);

                if (student == null)
                {
                    result.Success = false;
                    result.ErrorMessage = "الطالب غير موجود";
                    result.StudentName = $"طالب #{studentRequest.StudentID}";
                    response.Results.Add(result);
                    response.FailedCount++;
                    continue;
                }

                result.StudentName = (student.FullName?.FirstName ?? "") + " " +
                                   (student.FullName?.MiddleName ?? "") + " " +
                                   (student.FullName?.LastName ?? "");

                // Verify the new division exists and belongs to target year
                var newDivision = await _context.Divisions
                    .Include(d => d.Class)
                        .ThenInclude(c => c.Year)
                    .Include(d => d.Class)
                        .ThenInclude(c => c.Stage)
                            .ThenInclude(st => st.Year)
                    .FirstOrDefaultAsync(d => d.DivisionID == studentRequest.NewDivisionID);

                if (newDivision == null)
                {
                    result.Success = false;
                    result.ErrorMessage = "القسم المحدد غير موجود";
                    response.Results.Add(result);
                    response.FailedCount++;
                    continue;
                }

                // Verify new division belongs to target year
                bool belongsToTargetYear = (newDivision.Class.Year != null && newDivision.Class.Year.YearID == targetYear.YearID) ||
                                         (newDivision.Class.Stage != null && newDivision.Class.Stage.Year != null && 
                                          newDivision.Class.Stage.Year.YearID == targetYear.YearID);

                if (!belongsToTargetYear)
                {
                    result.Success = false;
                    result.ErrorMessage = "القسم المحدد لا ينتمي للسنة الدراسية المستهدفة";
                    response.Results.Add(result);
                    response.FailedCount++;
                    continue;
                }

                // Check if student passed (optional - check TermlyGrade for current year)
                var currentYearID = student.Division?.Class?.YearID ?? 
                                   (student.Division?.Class?.Stage?.YearID);
                
                if (currentYearID.HasValue)
                {
                    // Get student's termly grades for current year to check if passed
                    var termlyGrades = await _context.TermlyGrades
                        .Where(tg => tg.StudentID == student.StudentID && tg.YearID == currentYearID.Value)
                        .ToListAsync();

                    // Optional: Add logic to check if student passed based on grades
                    // For now, we'll allow promotion regardless (you can add minimum grade requirements here)
                }

                // Store original division ID for reference (not for rollback - we're creating new data)
                var originalDivisionID = student.DivisionID;
                var originalClassID = student.Division?.Class?.ClassID;

                // Update student's division to the new division (this is the only update we make)
                // All other data (fees, grades) will be created as new records
                student.DivisionID = studentRequest.NewDivisionID;
                _context.Entry(student).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                // Save student division change first
                await _context.SaveChangesAsync();

                // Create new fees for student in new class (only create, never update)
                var newClassID = newDivision.Class.ClassID;
                var feeClasses = await _context.FeeClass
                    .Where(fc => fc.ClassID == newClassID)
                    .ToListAsync();

                var studentClassFees = new List<StudentClassFees>();
                foreach (var feeClass in feeClasses)
                {
                    // Check if student already has this fee for this class/year combination
                    // We only create new fees, never update existing ones
                    var existingFee = await _context.StudentClassFees
                        .Include(scf => scf.FeeClass)
                            .ThenInclude(fc => fc.Class)
                        .FirstOrDefaultAsync(scf => scf.StudentID == student.StudentID && 
                                                   scf.FeeClassID == feeClass.FeeClassID);

                    // Only create if it doesn't exist
                    if (existingFee == null)
                    {
                        studentClassFees.Add(new StudentClassFees
                        {
                            StudentID = student.StudentID,
                            FeeClassID = feeClass.FeeClassID,
                            Mandatory = feeClass.Mandatory,
                            AmountDiscount = null,
                            NoteDiscount = null
                        });
                    }
                }

                // Only add new fees (never update existing ones)
                if (studentClassFees.Count > 0)
                {
                    await _context.StudentClassFees.AddRangeAsync(studentClassFees);
                    await _context.SaveChangesAsync();
                }

                // Copy course plan and create grades for new class
                var coursePlans = await _context.CoursePlans
                    .Where(cp => cp.DivisionID == newDivision.DivisionID &&
                                cp.ClassID == newClassID &&
                                cp.YearID == targetYear.YearID)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} course plans for DivisionID: {DivisionID}, ClassID: {ClassID}, YearID: {YearID}", 
                    coursePlans.Count, newDivision.DivisionID, newClassID, targetYear.YearID);

                // If no course plans found for target year, try to copy from active year
                if (coursePlans.Count == 0)
                {
                    _logger.LogWarning("No course plans found for promoted student {StudentID} in DivisionID: {DivisionID}, ClassID: {ClassID}, YearID: {YearID}. Attempting to copy from active year.", 
                        student.StudentID, newDivision.DivisionID, newClassID, targetYear.YearID);

                    // Get active year
                    var activeYear = await _context.Years
                        .Where(y => y.Active)
                        .FirstOrDefaultAsync();

                    if (activeYear != null && activeYear.YearID != targetYear.YearID)
                    {
                        // Try to find course plans from active year for the same division and class
                        var activeYearCoursePlans = await _context.CoursePlans
                            .Where(cp => cp.DivisionID == newDivision.DivisionID &&
                                        cp.ClassID == newClassID &&
                                        cp.YearID == activeYear.YearID)
                            .ToListAsync();

                        if (activeYearCoursePlans.Count > 0)
                        {
                            _logger.LogInformation("Found {Count} course plans in active year {ActiveYearID}. Copying to target year {TargetYearID}.",
                                activeYearCoursePlans.Count, activeYear.YearID, targetYear.YearID);

                            // Copy course plans to target year
                            var newCoursePlans = activeYearCoursePlans.Select(cp => new CoursePlan
                            {
                                YearID = targetYear.YearID,
                                TermID = cp.TermID,
                                SubjectID = cp.SubjectID,
                                TeacherID = cp.TeacherID,
                                ClassID = cp.ClassID,
                                DivisionID = cp.DivisionID
                            }).ToList();

                            // Check for duplicates before adding
                            var existingCoursePlans = await _context.CoursePlans
                                .Where(cp => cp.DivisionID == newDivision.DivisionID &&
                                            cp.ClassID == newClassID &&
                                            cp.YearID == targetYear.YearID)
                                .Select(cp => new { cp.TermID, cp.SubjectID, cp.TeacherID })
                                .ToListAsync();

                            var plansToAdd = newCoursePlans
                                .Where(ncp => !existingCoursePlans.Any(ecp => 
                                    ecp.TermID == ncp.TermID &&
                                    ecp.SubjectID == ncp.SubjectID &&
                                    ecp.TeacherID == ncp.TeacherID))
                                .ToList();

                            if (plansToAdd.Count > 0)
                            {
                                await _context.CoursePlans.AddRangeAsync(plansToAdd);
                                await _context.SaveChangesAsync();
                                _logger.LogInformation("Copied {Count} course plans from active year {ActiveYearID} to target year {TargetYearID}.",
                                    plansToAdd.Count, activeYear?.YearID ?? 0, targetYear.YearID);
                            }

                            // Reload course plans for target year
                            coursePlans = await _context.CoursePlans
                                .Where(cp => cp.DivisionID == newDivision.DivisionID &&
                                            cp.ClassID == newClassID &&
                                            cp.YearID == targetYear.YearID)
                                .ToListAsync();
                        }
                        else
                        {
                            // Try to find course plans from the same class but different division in active year
                            var allSameClassPlans = await _context.CoursePlans
                                .Where(cp => cp.ClassID == newClassID &&
                                            cp.YearID == activeYear.YearID)
                                .ToListAsync();
                            
                            // Group by TermID, SubjectID, TeacherID and take first of each group
                            var sameClassCoursePlans = allSameClassPlans
                                .GroupBy(cp => new { cp.TermID, cp.SubjectID, cp.TeacherID })
                                .Select(g => g.First())
                                .ToList();

                            if (sameClassCoursePlans.Count > 0)
                            {
                                _logger.LogInformation("Found {Count} course plans for same class in active year. Copying to target year with new division.",
                                    sameClassCoursePlans.Count);

                                var newCoursePlans = sameClassCoursePlans.Select(cp => new CoursePlan
                                {
                                    YearID = targetYear.YearID,
                                    TermID = cp.TermID,
                                    SubjectID = cp.SubjectID,
                                    TeacherID = cp.TeacherID,
                                    ClassID = cp.ClassID,
                                    DivisionID = newDivision.DivisionID
                                }).ToList();

                                // Check for duplicates
                                var existingCoursePlans = await _context.CoursePlans
                                    .Where(cp => cp.DivisionID == newDivision.DivisionID &&
                                                cp.ClassID == newClassID &&
                                                cp.YearID == targetYear.YearID)
                                    .Select(cp => new { cp.TermID, cp.SubjectID, cp.TeacherID })
                                    .ToListAsync();

                                var plansToAdd = newCoursePlans
                                    .Where(ncp => !existingCoursePlans.Any(ecp => 
                                        ecp.TermID == ncp.TermID &&
                                        ecp.SubjectID == ncp.SubjectID &&
                                        ecp.TeacherID == ncp.TeacherID))
                                    .ToList();

                                if (plansToAdd.Count > 0)
                                {
                                    await _context.CoursePlans.AddRangeAsync(plansToAdd);
                                    await _context.SaveChangesAsync();
                                    _logger.LogInformation("Copied {Count} course plans from same class in active year to target year with new division.",
                                        plansToAdd.Count);
                                }

                                // Reload course plans
                                coursePlans = await _context.CoursePlans
                                    .Where(cp => cp.DivisionID == newDivision.DivisionID &&
                                                cp.ClassID == newClassID &&
                                                cp.YearID == targetYear.YearID)
                                    .ToListAsync();
                            }
                        }
                    }

                    if (coursePlans.Count == 0)
                    {
                        _logger.LogWarning("No course plans found or copied for promoted student {StudentID} in DivisionID: {DivisionID}, ClassID: {ClassID}, YearID: {YearID}. Grades will not be created.", 
                            student.StudentID, newDivision.DivisionID, newClassID, targetYear.YearID);
                        result.ErrorMessage = "تمت الترقية بنجاح، لكن لا توجد خطة دراسية للقسم الجديد";
                    }
                }
                else
                {
                    // Get months and grade types for target year
                    var months = await _context.YearTermMonths
                        .Where(m => m.YearID == targetYear.YearID)
                        .Select(m => new { m.MonthID, m.TermID })
                        .ToListAsync();

                    // If no months found for target year, try to copy from active year or create default
                    if (months.Count == 0)
                    {
                        _logger.LogWarning("No YearTermMonths found for target YearID: {YearID}. Attempting to copy from active year or create default.", targetYear.YearID);
                        
                        var activeYear = await _context.Years
                            .Where(y => y.Active)
                            .FirstOrDefaultAsync();

                        List<YearTermMonth> monthsToAdd = new List<YearTermMonth>();

                        // Try to copy from active year if it's different
                        if (activeYear != null && activeYear.YearID != targetYear.YearID)
                        {
                            var activeYearMonths = await _context.YearTermMonths
                                .Where(m => m.YearID == activeYear.YearID)
                                .ToListAsync();

                            if (activeYearMonths.Count > 0)
                            {
                                monthsToAdd = activeYearMonths.Select(aym => new YearTermMonth
                                {
                                    YearID = targetYear.YearID,
                                    TermID = aym.TermID,
                                    MonthID = aym.MonthID
                                }).ToList();

                                _logger.LogInformation("Will copy {Count} YearTermMonths from active year {ActiveYearID} to target year {TargetYearID}", 
                                    monthsToAdd.Count, activeYear.YearID, targetYear.YearID);
                            }
                        }

                        // If no months found to copy, create default YearTermMonths
                        if (monthsToAdd.Count == 0)
                        {
                            _logger.LogInformation("Creating default YearTermMonths for target YearID: {YearID}", targetYear.YearID);
                            // Default: Term 1 (months 5-8), Term 2 (months 9-12)
                            monthsToAdd = new List<YearTermMonth>
                            {
                                new YearTermMonth { YearID = targetYear.YearID, TermID = 1, MonthID = 5 },
                                new YearTermMonth { YearID = targetYear.YearID, TermID = 1, MonthID = 6 },
                                new YearTermMonth { YearID = targetYear.YearID, TermID = 1, MonthID = 7 },
                                new YearTermMonth { YearID = targetYear.YearID, TermID = 1, MonthID = 8 },
                                new YearTermMonth { YearID = targetYear.YearID, TermID = 2, MonthID = 9 },
                                new YearTermMonth { YearID = targetYear.YearID, TermID = 2, MonthID = 10 },
                                new YearTermMonth { YearID = targetYear.YearID, TermID = 2, MonthID = 11 },
                                new YearTermMonth { YearID = targetYear.YearID, TermID = 2, MonthID = 12 }
                            };
                        }

                        if (monthsToAdd.Count > 0)
                        {
                            await _context.YearTermMonths.AddRangeAsync(monthsToAdd);
                            await _context.SaveChangesAsync();
                            
                            _logger.LogInformation("Created {Count} YearTermMonths for target year {TargetYearID}", 
                                monthsToAdd.Count, targetYear.YearID);

                            // Reload months for target year
                            months = await _context.YearTermMonths
                                .Where(m => m.YearID == targetYear.YearID)
                                .Select(m => new { m.MonthID, m.TermID })
                                .ToListAsync();
                        }
                    }

                    var gradeTypes = await _context.GradeTypes
                        .Where(g => g.IsActive)
                        .Select(g => g.GradeTypeID)
                        .ToListAsync();

                    _logger.LogInformation("Found {MonthCount} months and {GradeTypeCount} grade types for YearID: {YearID}", 
                        months.Count, gradeTypes.Count, targetYear.YearID);

                    if (months.Count == 0 || gradeTypes.Count == 0)
                    {
                        _logger.LogWarning("No months or grade types found for YearID: {YearID}. Months: {MonthCount}, GradeTypes: {GradeTypeCount}", 
                            targetYear.YearID, months.Count, gradeTypes.Count);
                        result.ErrorMessage = $"تمت الترقية بنجاح، لكن لا توجد أشهر دراسية للسنة المستهدفة (YearID: {targetYear.YearID})";
                    }

                    var monthlyGrades = new List<MonthlyGrade>();
                    var termlyGrades = new List<TermlyGrade>();

                    // Bulk fetch existing grades to avoid multiple database queries
                    var existingTermlyGrades = await _context.TermlyGrades
                        .Where(tg => tg.StudentID == student.StudentID &&
                                    tg.YearID == targetYear.YearID &&
                                    tg.ClassID == newClassID)
                        .Select(tg => new { tg.StudentID, tg.YearID, tg.TermID, tg.ClassID, tg.SubjectID })
                        .ToListAsync();

                    var existingMonthlyGrades = await _context.MonthlyGrades
                        .Where(mg => mg.StudentID == student.StudentID &&
                                    mg.YearID == targetYear.YearID &&
                                    mg.ClassID == newClassID)
                        .Select(mg => new { mg.StudentID, mg.YearID, mg.TermID, mg.ClassID, mg.SubjectID, mg.MonthID, mg.GradeTypeID })
                        .ToListAsync();

                    _logger.LogInformation("Found {ExistingTermlyCount} existing TermlyGrades and {ExistingMonthlyCount} existing MonthlyGrades for student {StudentID} in YearID {YearID}, ClassID {ClassID}",
                        existingTermlyGrades.Count, existingMonthlyGrades.Count, student.StudentID, targetYear.YearID, newClassID);

                    foreach (var plan in coursePlans)
                    {
                        // Create NEW TermlyGrade for this subject in the new year/class
                        // Only create if it doesn't exist - never update existing grades
                        var termlyExists = existingTermlyGrades.Any(tg => 
                            tg.StudentID == student.StudentID &&
                            tg.YearID == targetYear.YearID &&
                            tg.TermID == plan.TermID &&
                            tg.ClassID == newClassID &&
                            tg.SubjectID == plan.SubjectID);

                        if (!termlyExists)
                        {
                            // Create new TermlyGrade record (never update existing)
                            termlyGrades.Add(new TermlyGrade
                            {
                                StudentID = student.StudentID,
                                YearID = targetYear.YearID,
                                TermID = plan.TermID,
                                ClassID = newClassID,
                                SubjectID = plan.SubjectID,
                                Grade = null,
                                Note = null
                            });
                            _logger.LogDebug("Added TermlyGrade for StudentID: {StudentID}, YearID: {YearID}, TermID: {TermID}, ClassID: {ClassID}, SubjectID: {SubjectID}",
                                student.StudentID, targetYear.YearID, plan.TermID, newClassID, plan.SubjectID);
                        }

                        // Create NEW MonthlyGrade for each month and grade type
                        // Only create if it doesn't exist - never update existing grades
                        var monthsForTerm = months.Where(m => m.TermID == plan.TermID).ToList();
                        _logger.LogDebug("Processing {MonthCount} months for TermID: {TermID}, SubjectID: {SubjectID}",
                            monthsForTerm.Count, plan.TermID, plan.SubjectID);

                        foreach (var month in monthsForTerm)
                        {
                            foreach (var gradeType in gradeTypes)
                            {
                                // Check composite key to ensure we don't create duplicates
                                var monthlyExists = existingMonthlyGrades.Any(mg => 
                                    mg.StudentID == student.StudentID &&
                                    mg.YearID == targetYear.YearID &&
                                    mg.TermID == plan.TermID &&
                                    mg.ClassID == newClassID &&
                                    mg.SubjectID == plan.SubjectID &&
                                    mg.MonthID == month.MonthID &&
                                    mg.GradeTypeID == gradeType);

                                if (!monthlyExists)
                                {
                                    // Create new MonthlyGrade record (never update existing)
                                    monthlyGrades.Add(new MonthlyGrade
                                    {
                                        StudentID = student.StudentID,
                                        YearID = targetYear.YearID,
                                        TermID = plan.TermID,
                                        ClassID = newClassID,
                                        SubjectID = plan.SubjectID,
                                        MonthID = month.MonthID,
                                        GradeTypeID = gradeType,
                                        Grade = null
                                    });
                                }
                            }
                        }
                    }

                    _logger.LogInformation("Prepared {TermlyCount} TermlyGrades and {MonthlyCount} MonthlyGrades for student {StudentID} (before saving)", 
                        termlyGrades.Count, monthlyGrades.Count, student.StudentID);

                    // Add all grades to context first, then save once
                    if (termlyGrades.Count > 0)
                    {
                        await _context.TermlyGrades.AddRangeAsync(termlyGrades);
                        _logger.LogInformation("Added {Count} TermlyGrades to context for student {StudentID}", 
                            termlyGrades.Count, student.StudentID);
                    }

                    if (monthlyGrades.Count > 0)
                    {
                        await _context.MonthlyGrades.AddRangeAsync(monthlyGrades);
                        _logger.LogInformation("Added {Count} MonthlyGrades to context for student {StudentID}", 
                            monthlyGrades.Count, student.StudentID);
                    }

                    // Save all changes in a single transaction
                    if (termlyGrades.Count > 0 || monthlyGrades.Count > 0)
                    {
                        try
                        {
                            var savedCount = await _context.SaveChangesAsync();
                            _logger.LogInformation("✅ Successfully saved {SavedCount} changes for student {StudentID}. TermlyGrades: {TermlyCount}, MonthlyGrades: {MonthlyCount}", 
                                savedCount, student.StudentID, termlyGrades.Count, monthlyGrades.Count);
                            
                            // Verify the grades were actually saved
                            var savedTermlyCount = await _context.TermlyGrades
                                .CountAsync(tg => tg.StudentID == student.StudentID && 
                                                 tg.YearID == targetYear.YearID && 
                                                 tg.ClassID == newClassID);
                            
                            var savedMonthlyCount = await _context.MonthlyGrades
                                .CountAsync(mg => mg.StudentID == student.StudentID && 
                                                mg.YearID == targetYear.YearID && 
                                                mg.ClassID == newClassID);
                            
                            _logger.LogInformation("Verified saved grades for student {StudentID}: {TermlyCount} TermlyGrades, {MonthlyCount} MonthlyGrades in database", 
                                student.StudentID, savedTermlyCount, savedMonthlyCount);
                        }
                        catch (DbUpdateException ex)
                        {
                            // Log the error but don't fail the entire promotion
                            _logger.LogError(ex, "❌ Error saving grades for student {StudentID}. TermlyGrades: {TermlyCount}, MonthlyGrades: {MonthlyCount}. Error: {ErrorMessage}", 
                                student.StudentID, termlyGrades.Count, monthlyGrades.Count, ex.Message);
                            
                            // Log inner exception if available
                            if (ex.InnerException != null)
                            {
                                _logger.LogError("Inner exception: {InnerException}", ex.InnerException.Message);
                            }
                            
                            result.ErrorMessage = $"تمت الترقية بنجاح، لكن حدث خطأ في حفظ الدرجات: {ex.Message}";
                            // Continue with promotion even if grades fail to save
                        }
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ No grades to save for student {StudentID}. All grades may already exist. TermlyGrades: {TermlyCount}, MonthlyGrades: {MonthlyCount}", 
                            student.StudentID, termlyGrades.Count, monthlyGrades.Count);
                    }
                }

                result.Success = true;
                response.Results.Add(result);
                response.SuccessCount++;
            }
            catch (Exception ex)
            {
                // If this student fails, mark as failed but continue with others
                result.Success = false;
                result.ErrorMessage = $"خطأ: {ex.Message}";
                if (string.IsNullOrEmpty(result.StudentName))
                {
                    result.StudentName = $"طالب #{studentRequest.StudentID}";
                }
                response.Results.Add(result);
                response.FailedCount++;
                _logger.LogError(ex, "Error promoting student {StudentID}", studentRequest.StudentID);
            }
        }

        return response;
    }
}
