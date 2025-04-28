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
    private readonly DatabaseContext _context;
    private readonly IMapper _mapper;

    public MonthlyGradeRepository(DatabaseContext context, IMapper mapper)
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

        var baseQuery = _context.MonthlyGrades
            .Where(g => g.TermID == term &&
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
        var changed = false;

        foreach (var dto in dtos)
        {
            var grade = await _context.MonthlyGrades.FirstOrDefaultAsync(g =>
                g.StudentID == dto.StudentID &&
                g.YearID == dto.YearID &&
                g.SubjectID == dto.SubjectID &&
                g.MonthID == dto.MonthID &&
                g.TermID == dto.TermID &&
                g.ClassID == dto.ClassID &&
                g.GradeTypeID == dto.GradeTypeID);

            if (grade != null && dto.Grade != 0 && grade.Grade != dto.Grade)
            {
                grade.Grade = dto.Grade;
                changed = true;
            }
        }

        if (!changed)
            return Result<bool>.Fail("Nothing to update.");

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
          var query = _context.MonthlyGrades
        .Where(g => g.TermID == term &&
                    g.MonthID == monthId &&
                    g.ClassID == classId);

    if (subjectId != 0)
        query = query.Where(g => g.SubjectID == subjectId);

    // نحسب عدد الطلاب الفريدين فقط، وليس كل السجلات
    return await query
        .Select(g => g.StudentID)
        .Distinct()
        .CountAsync();
    }
}
