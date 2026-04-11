using AutoMapper;
using Backend.Common;
using Backend.Data;
using Backend.DTOS.School.GradeTypes;
using Backend.DTOS.School.MonthlyGrade;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

public class MonthlyGradeRepository : IMonthlyGradeRepository
{
    private readonly TenantDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAuditTrailService _auditTrail;
    private readonly IApiBaseUrlProvider _apiBaseUrl;

    public MonthlyGradeRepository(TenantDbContext context, IMapper mapper, IAuditTrailService auditTrail, IApiBaseUrlProvider apiBaseUrl)
    {
        _context = context;
        _mapper = mapper;
        _auditTrail = auditTrail;
        _apiBaseUrl = apiBaseUrl;
    }

    public Task<int?> GetGuardianIdByUserIdAsync(string userId) =>
        _context.Guardians.AsNoTracking()
            .Where(g => g.UserID == userId)
            .Select(g => (int?)g.GuardianID)
            .FirstOrDefaultAsync();

    public async Task<List<GuardianMonthlyGradeRowDto>> GetGuardianStudentsMonthlyGradesAsync(int guardianId, int? yearId, int? termId, int? monthId)
    {
        var studentIds = await _context.Students.AsNoTracking()
            .Where(s => s.GuardianID == guardianId)
            .Select(s => s.StudentID)
            .ToListAsync();

        if (studentIds.Count == 0)
            return new List<GuardianMonthlyGradeRowDto>();

        int effectiveYearId;
        if (yearId.HasValue)
        {
            effectiveYearId = yearId.Value;
        }
        else
        {
            var activeYear = await _context.Years
                .AsNoTracking()
                .Where(y => y.Active)
                .OrderBy(y => y.YearID)
                .FirstOrDefaultAsync();
            if (activeYear == null)
                return new List<GuardianMonthlyGradeRowDto>();
            effectiveYearId = activeYear.YearID;
        }

        var q = _context.MonthlyGrades
            .AsNoTracking()
            .Where(g => studentIds.Contains(g.StudentID) && g.YearID == effectiveYearId && g.GradeType.IsActive);

        if (termId.HasValue)
            q = q.Where(g => g.TermID == termId.Value);
        if (monthId.HasValue)
            q = q.Where(g => g.MonthID == monthId.Value);

        var grades = await q
            .Include(g => g.Student).ThenInclude(s => s.FullName)
            .Include(g => g.Subject)
            .Include(g => g.GradeType)
            .Include(g => g.Term)
            .Include(g => g.Month)
            .Include(g => g.Class)
            .Where(g => g.Student != null && g.Subject != null)
            .ToListAsync();

        return grades
            .GroupBy(g => new { g.StudentID, g.SubjectID, g.YearID, g.TermID, g.MonthID, g.ClassID })
            .Select(grp =>
            {
                var first = grp.First();
                var studentName = first.Student?.FullName != null
                    ? $"{first.Student.FullName.FirstName} {first.Student.FullName.MiddleName} {first.Student.FullName.LastName}".Replace("  ", " ").Trim()
                    : "—";
                return new GuardianMonthlyGradeRowDto
                {
                    StudentID = grp.Key.StudentID,
                    StudentName = string.IsNullOrWhiteSpace(studentName) ? "—" : studentName,
                    YearID = grp.Key.YearID,
                    TermID = grp.Key.TermID,
                    TermName = first.Term?.Name,
                    MonthID = grp.Key.MonthID,
                    MonthName = first.Month?.Name,
                    ClassID = grp.Key.ClassID,
                    ClassName = first.Class?.ClassName,
                    SubjectID = grp.Key.SubjectID,
                    SubjectName = first.Subject?.SubjectName ?? "—",
                    Grades = grp.Select(g => new GradeTypeMonthDTO
                    {
                        GradeTypeID = g.GradeTypeID,
                        MaxGrade = g.Grade,
                        GradeTypeName = g.GradeType?.Name
                    })
                    .OrderBy(x => x.GradeTypeID)
                    .ToList()
                };
            })
            .OrderByDescending(r => r.MonthID)
            .ThenBy(r => r.TermID)
            .ThenBy(r => r.StudentName)
            .ThenBy(r => r.SubjectName)
            .ToList();
    }

    /* ----------  CREATE  ---------- */
    public async Task<Result<MonthlyGradeDTO>> AddAsync(MonthlyGradeDTO dto)
    {
        if (dto == null)
            return Result<MonthlyGradeDTO>.Fail("MonthlyGrade payload is null.");

        var entity = _mapper.Map<MonthlyGrade>(dto);
        if (entity.YearID <= 0)
        {
            var activeYear = await _context.Years
                .Where(y => y.Active)
                .OrderBy(y => y.YearID)
                .FirstOrDefaultAsync();
            if (activeYear == null)
                return Result<MonthlyGradeDTO>.Fail("No active year found. Activate a year before adding monthly grades.");
            entity.YearID = activeYear.YearID;
        }

        await _context.MonthlyGrades.AddAsync(entity);

        try
        {
            await _context.SaveChangesAsync();
            return Result<MonthlyGradeDTO>.Success(dto);
        }
        catch (DbUpdateException ex)
        {
            return Result<MonthlyGradeDTO>.Fail($"DB error: {ex.Message}");
        }
    }

    /* ----------  READ  ---------- */
    public async Task<Result<List<MonthlyGradesReternDTO>>> GetAllAsync(MonthlyGradesQueryDTO query)
    {
        if (query == null)
            return Result<List<MonthlyGradesReternDTO>>.Fail("Query payload is null.");

        if (query.PageNumber < 1 || query.PageSize < 1)
            return Result<List<MonthlyGradesReternDTO>>.Fail("Page number must be greater than 0.");

        // Get the active year - monthly grades should always be read from the active year
        var activeYear = await _context.Years
            .Where(y => y.Active == true)
            .OrderBy(y => y.YearID)
            .FirstOrDefaultAsync();

        if (activeYear == null)
            return Result<List<MonthlyGradesReternDTO>>.Fail("No active year found. Please activate a year before viewing monthly grades.");

        var baseQuery = _context.MonthlyGrades
            .Where(g => g.TermID == query.TermId &&
                        g.YearID == activeYear.YearID &&
                        g.MonthID == query.MonthId &&
                        g.ClassID == query.ClassId &&
                        g.GradeType.IsActive);

        if (query.SubjectId != 0)
            baseQuery = baseQuery.Where(g => g.SubjectID == query.SubjectId);

        // Bring all matching records
        var grades = await baseQuery
            .Include(g => g.Student).ThenInclude(s => s.FullName)
            .Include(g => g.Subject)
            .Include(g => g.GradeType)
            .Where(g => g.Student != null && g.Subject != null) // Filter out records with null navigation properties
            .ToListAsync();

        // Now group in memory by StudentID and SubjectID only
        var grouped = grades
            .GroupBy(g => new
            {
                g.StudentID,
                g.SubjectID
            })
            .Select(grp => new MonthlyGradesReternDTO
            {
                StudentID = grp.Key.StudentID,
                StudentName = grp.First().Student?.FullName != null
                    ? $"{grp.First().Student.FullName.FirstName} {grp.First().Student.FullName.MiddleName} {grp.First().Student.FullName.LastName}".Trim()
                    : "Unknown Student",
                StudentURL = grp.First().Student?.ImageURL != null
                    ? _apiBaseUrl.UploadsFile($"StudentPhotos/{grp.First().Student!.ImageURL}")
                    : null,
                SubjectID = grp.Key.SubjectID,
                SubjectName = grp.First().Subject?.SubjectName ?? "Unknown Subject",
                Grades = grp.Select(g => new GradeTypeMonthDTO
                {
                    GradeTypeID = g.GradeTypeID,
                    MaxGrade = g.Grade,
                    GradeTypeName = g.GradeType?.Name
                })
                .OrderBy(g => g.GradeTypeID)
                .ToList() 
            })
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        return Result<List<MonthlyGradesReternDTO>>.Success(grouped);
    }

    /* ----------  UPDATE MANY  ---------- */
    public async Task<Result<bool>> UpdateManyAsync(IEnumerable<MonthlyGradeDTO> dtos)
    {
        // Get the active year - if multiple active years exist, take the first one
        var activeYear = await _context.Years
            .Where(y => y.Active == true)
            .OrderBy(y => y.YearID) // Order by YearID to ensure consistent selection
            .FirstOrDefaultAsync();

        if (activeYear == null)
            return Result<bool>.Fail("No active year found. Please activate a year before updating monthly grades.");

        // Must match GetAllAsync: monthly grades for this screen are always scoped to the active year.
        // Clients often send YearID from localStorage, which can disagree with the DB active flag and cause "not found".
        var yearIdForRow = activeYear.YearID;

        var changed = false;
        var changeLog = new List<object>();

        foreach (var dto in dtos)
        {
            var grade = await _context.MonthlyGrades.FirstOrDefaultAsync(g =>
                g.StudentID == dto.StudentID &&
                g.YearID == yearIdForRow &&
                g.SubjectID == dto.SubjectID &&
                g.MonthID == dto.MonthID &&
                g.TermID == dto.TermID &&
                g.ClassID == dto.ClassID &&
                g.GradeTypeID == dto.GradeTypeID);

            if (grade == null)
            {
                // Upsert: row missing (e.g. after promotion or incomplete seeding) — create so save matches list/query.
                await _context.MonthlyGrades.AddAsync(new MonthlyGrade
                {
                    StudentID = dto.StudentID,
                    YearID = yearIdForRow,
                    SubjectID = dto.SubjectID,
                    MonthID = dto.MonthID,
                    TermID = dto.TermID,
                    ClassID = dto.ClassID,
                    GradeTypeID = dto.GradeTypeID,
                    Grade = dto.Grade
                });
                changeLog.Add(new
                {
                    dto.StudentID,
                    YearID = yearIdForRow,
                    dto.ClassID,
                    dto.SubjectID,
                    dto.MonthID,
                    dto.TermID,
                    dto.GradeTypeID,
                    OldGrade = (decimal?)null,
                    NewGrade = dto.Grade,
                    Inserted = true
                });
                changed = true;
                continue;
            }

            // Update if the grade value is different (allow updating to 0 or null)
            if (grade.Grade != dto.Grade)
            {
                changeLog.Add(new
                {
                    grade.StudentID,
                    grade.YearID,
                    grade.ClassID,
                    grade.SubjectID,
                    grade.MonthID,
                    grade.TermID,
                    grade.GradeTypeID,
                    OldGrade = grade.Grade,
                    NewGrade = dto.Grade
                });
                grade.Grade = dto.Grade;
                changed = true;
            }
        }

        if (!changed)
            return Result<bool>.Fail("Nothing to update. All grades are already set to the provided values.");

        try
        {
            await _context.SaveChangesAsync();
            await _auditTrail.RecordAsync(
                "Grades",
                "MonthlyGrade.BulkUpdate",
                new
                {
                    activeYear.YearID,
                    ChangeCount = changeLog.Count,
                    Changes = changeLog
                });
            return Result<bool>.Success(true);
        }
        catch (DbUpdateException ex)
        {
            return Result<bool>.Fail($"DB error: {ex.Message}");
        }
    }

    /* ----------  DELETE  ---------- */
    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var grade = await _context.MonthlyGrades.FindAsync(id);
        if (grade is null)
            return Result<bool>.Fail($"Monthly grade {id} not found.");

        _context.MonthlyGrades.Remove(grade);

        try
        {
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
        catch (DbUpdateException ex)
        {
            return Result<bool>.Fail($"DB error: {ex.Message}");
        }
    }

    public async Task<int> GetTotalMonthlyGradesCountAsync(MonthlyGradesQueryDTO query)
    {
        if (query == null)
            return 0;

        // Get the active year - keep count consistent with GetAllAsync
        var activeYear = await _context.Years
            .Where(y => y.Active == true)
            .OrderBy(y => y.YearID)
            .FirstOrDefaultAsync();

        if (activeYear == null)
            return 0;

        var q = _context.MonthlyGrades
            .Where(g => g.TermID == query.TermId &&
                        g.YearID == activeYear.YearID &&
                        g.MonthID == query.MonthId &&
                        g.ClassID == query.ClassId &&
                        g.GradeType.IsActive);

        if (query.SubjectId != 0)
            q = q.Where(g => g.SubjectID == query.SubjectId);

        return await q
            .Select(g => g.StudentID)
            .Distinct()
            .CountAsync();
    }
}
