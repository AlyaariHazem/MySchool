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

    public MonthlyGradeRepository(TenantDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /* ----------  CREATE  ---------- */
    public async Task<Result<MonthlyGradeDTO>> AddAsync(MonthlyGradeDTO dto)
    {
        if (dto == null)
            return Result<MonthlyGradeDTO>.Fail("MonthlyGrade payload is null.");

        var entity = _mapper.Map<MonthlyGrade>(dto);
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
    public async Task<Result<List<MonthlyGradesReternDTO>>> GetAllAsync(
        int term, int monthId, int classId, int subjectId, int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1)
            return Result<List<MonthlyGradesReternDTO>>.Fail("Page number must be greater than 0.");

        // Get the active year - monthly grades should always be read from the active year
        var activeYear = await _context.Years
            .Where(y => y.Active == true)
            .OrderBy(y => y.YearID)
            .FirstOrDefaultAsync();

        if (activeYear == null)
            return Result<List<MonthlyGradesReternDTO>>.Fail("No active year found. Please activate a year before viewing monthly grades.");

        var baseQuery = _context.MonthlyGrades
            .Where(g => g.TermID == term &&
                        g.YearID == activeYear.YearID &&
                        g.MonthID == monthId &&
                        g.ClassID == classId);

        if (subjectId != 0)
            baseQuery = baseQuery.Where(g => g.SubjectID == subjectId);

        // Bring all matching records
        var grades = await baseQuery
            .Include(g => g.Student).ThenInclude(s => s.FullName)
            .Include(g => g.Subject)
            .Include(g => g.GradeType)
            .ToListAsync();

        // Now group in memory
        var grouped = grades
            .GroupBy(g => new
            {
                g.StudentID,
                g.Student.FullName,
                g.SubjectID,
                g.Subject.SubjectName,
                g.Student.ImageURL
            })
            .Select(grp => new MonthlyGradesReternDTO
            {
                StudentID = grp.Key.StudentID,
                StudentName = $"{grp.Key.FullName.FirstName} {grp.Key.FullName.MiddleName} {grp.Key.FullName.LastName}",
                StudentURL =  $"https://localhost:7258/uploads/StudentPhotos/{grp.Key.ImageURL}",
                SubjectID = grp.Key.SubjectID,
                SubjectName = grp.Key.SubjectName,
                Grades = grp.Select(g => new GradeTypeMonthDTO
                {
                    GradeTypeID = g.GradeTypeID,
                    MaxGrade = g.Grade
                }).ToList() 
            })
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
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

        var changed = false;
        var notFound = new List<string>();

        foreach (var dto in dtos)
        {
            // Use active year instead of dto.YearID
            var grade = await _context.MonthlyGrades.FirstOrDefaultAsync(g =>
                g.StudentID == dto.StudentID &&
                g.YearID == activeYear.YearID &&
                g.SubjectID == dto.SubjectID &&
                g.MonthID == dto.MonthID &&
                g.TermID == dto.TermID &&
                g.ClassID == dto.ClassID &&
                g.GradeTypeID == dto.GradeTypeID);

            if (grade == null)
            {
                notFound.Add($"StudentID: {dto.StudentID}, SubjectID: {dto.SubjectID}, MonthID: {dto.MonthID}, GradeTypeID: {dto.GradeTypeID}");
                continue;
            }

            // Update if the grade value is different (allow updating to 0 or null)
            if (grade.Grade != dto.Grade)
            {
                grade.Grade = dto.Grade;
                changed = true;
            }
        }

        if (notFound.Count > 0)
        {
            return Result<bool>.Fail($"Some grades were not found: {string.Join("; ", notFound.Take(5))}" + 
                (notFound.Count > 5 ? $" (and {notFound.Count - 5} more)" : ""));
        }

        if (!changed)
            return Result<bool>.Fail("Nothing to update. All grades are already set to the provided values.");

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

    public async Task<int> GetTotalMonthlyGradesCountAsync(int term, int monthId, int classId, int subjectId)
    {
        // Get the active year - keep count consistent with GetAllAsync
        var activeYear = await _context.Years
            .Where(y => y.Active == true)
            .OrderBy(y => y.YearID)
            .FirstOrDefaultAsync();

        if (activeYear == null)
            return 0;

        var query = _context.MonthlyGrades
            .Where(g => g.TermID == term &&
                        g.YearID == activeYear.YearID &&
                        g.MonthID == monthId &&
                        g.ClassID == classId);

        if (subjectId != 0)
            query = query.Where(g => g.SubjectID == subjectId);

        return await query
            .Select(g => g.StudentID)
            .Distinct()
            .CountAsync();
    }
}
