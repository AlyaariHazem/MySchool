using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Common;
using Backend.Data;
using Backend.DTOS.School.TermlyGrade;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class TermlyGradeRepository : ITermlyGradeRepository
{
    private readonly TenantDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAuditTrailService _auditTrail;
    private readonly IApiBaseUrlProvider _apiBaseUrl;

    public TermlyGradeRepository(TenantDbContext context, IMapper mapper, IAuditTrailService auditTrail, IApiBaseUrlProvider apiBaseUrl)
    {
        _context = context;
        _mapper = mapper;
        _auditTrail = auditTrail;
        _apiBaseUrl = apiBaseUrl;
    }

    public async Task<Result<TermlyGradeDTO>> AddAsync(TermlyGradeDTO termlyGradeDTO)
    {
        if (termlyGradeDTO == null)
            return Result<TermlyGradeDTO>.Fail("TermlyGradeDTO cannot be null.");

        var entity = _mapper.Map<TermlyGrade>(termlyGradeDTO);
        if (entity.YearID <= 0)
        {
            var activeYear = await _context.Years
                .Where(y => y.Active)
                .OrderBy(y => y.YearID)
                .FirstOrDefaultAsync();
            if (activeYear == null)
                return Result<TermlyGradeDTO>.Fail("No active year found. Activate a year before adding termly grades.");
            entity.YearID = activeYear.YearID;
        }

        _context.TermlyGrades.Add(entity);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return Result<TermlyGradeDTO>.Fail($"DB error: {ex.InnerException?.Message ?? ex.Message}");
        }

        termlyGradeDTO.TermlyGradeID = entity.TermlyGradeID;
        termlyGradeDTO.YearID = entity.YearID;
        return Result<TermlyGradeDTO>.Success(termlyGradeDTO);
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var grade = await _context.TermlyGrades.FindAsync(id);
        if (grade == null)
            return Result<bool>.Fail("TermlyGrade not found.");

        _context.TermlyGrades.Remove(grade);
        await _context.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> UpdateAsync(IEnumerable<TermlyGradeDTO> termlyGradeDTO)
    {
        var changed = false;
        var changeLog = new List<object>();
        foreach (var grade in termlyGradeDTO)
        {
            if (!grade.TermlyGradeID.HasValue)
                return Result<bool>.Fail("TermlyGradeID is required for each row.");

            var existing = await _context.TermlyGrades.FirstOrDefaultAsync(g => g.TermlyGradeID == grade.TermlyGradeID.Value);
            if (existing == null)
                return Result<bool>.Fail($"TermlyGrade {grade.TermlyGradeID} not found.");

            var rowChanged =
                existing.Grade != grade.Grade
                || existing.Note != grade.Note
                || existing.TermID != grade.TermID
                || existing.ClassID != grade.ClassID
                || existing.SubjectID != grade.SubjectID
                || existing.StudentID != grade.StudentID;

            if (rowChanged)
            {
                changeLog.Add(new
                {
                    grade.TermlyGradeID,
                    Before = new
                    {
                        existing.StudentID,
                        existing.SubjectID,
                        existing.TermID,
                        existing.YearID,
                        existing.ClassID,
                        existing.Grade,
                        existing.Note
                    },
                    After = new
                    {
                        grade.StudentID,
                        grade.SubjectID,
                        grade.TermID,
                        YearID = existing.YearID,
                        grade.ClassID,
                        grade.Grade,
                        grade.Note
                    }
                });
            }

            existing.Grade = grade.Grade;
            existing.Note = grade.Note;
            existing.TermID = grade.TermID;
            existing.ClassID = grade.ClassID;
            existing.SubjectID = grade.SubjectID;
            existing.StudentID = grade.StudentID;
            if (rowChanged)
                changed = true;
        }

        if (!changed)
            return Result<bool>.Fail("Nothing to update.");
        try
        {
            await _context.SaveChangesAsync();
            await _auditTrail.RecordAsync(
                "Grades",
                "TermlyGrade.Update",
                new { ChangeCount = changeLog.Count, Changes = changeLog });
            return Result<bool>.Success(true);
        }
        catch (DbUpdateException ex)
        {
            return Result<bool>.Fail($"DB error: {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    public async Task<Result<TermlyGradeDTO>> GetByIdAsync(int id)
    {
        var termlyGrade = await _context.TermlyGrades
            .Include(g => g.Student)
            .Include(g => g.Subject)
            .Include(g => g.Term)
            .FirstOrDefaultAsync(g => g.TermlyGradeID == id);

        if (termlyGrade == null)
            return Result<TermlyGradeDTO>.Fail("TermlyGrade not found.");

        var termlyGradeDto = _mapper.Map<TermlyGradeDTO>(termlyGrade);
        return Result<TermlyGradeDTO>.Success(termlyGradeDto);
    }

    public Task<Result<List<TermlyGradesReturnDTO>>> GetAllAsync(TermlyGradeQueryDTO query)
    {
        if (query == null)
            return Task.FromResult(Result<List<TermlyGradesReturnDTO>>.Fail("Query is required."));
        return GetAllAsync(query.TermId, query.ClassId, query.SubjectId, query.PageNumber, query.PageSize);
    }

    public async Task<Result<List<TermlyGradesReturnDTO>>> GetAllAsync(
        int term, int classId, int subjectId, int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1)
            return Result<List<TermlyGradesReturnDTO>>.Fail("Page number must be greater than 0.");

        var activeYear = await GetActiveYearAsync();
        if (activeYear == null)
            return Result<List<TermlyGradesReturnDTO>>.Fail(
                "No active year found. Please activate a year before viewing termly grades.");

        var baseQuery = FilterTermlyGrades(term, classId, subjectId, activeYear.YearID);

        var rows = await baseQuery
            .Include(g => g.Student).ThenInclude(s => s.FullName)
            .Include(g => g.Subject)
            .ToListAsync();

        var grouped = rows
            .GroupBy(g => new
            {
                g.StudentID,
                g.Student.FullName,
                g.SubjectID,
                g.Subject.SubjectName,
                g.Student.ImageURL
            })
            .Select(grp => new TermlyGradesReturnDTO
            {
                TermlyGradeID = grp.First().TermlyGradeID,
                StudentID = grp.Key.StudentID,
                StudentName = $"{grp.Key.FullName.FirstName} {grp.Key.FullName.MiddleName} {grp.Key.FullName.LastName}",
                SubjectID = grp.Key.SubjectID,
                Note = grp.First().Note,
                SubjectName = grp.Key.SubjectName,
                StudentURL = grp.Key.ImageURL != null
                    ? _apiBaseUrl.UploadsFile($"StudentPhotos/{grp.Key.ImageURL}")
                    : null,
                Grade = grp.Sum(g => g.Grade)
            })
            .ToList();

        var paginatedResult = grouped
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Result<List<TermlyGradesReturnDTO>>.Success(paginatedResult);
    }

    public Task<int> GetTotalTermlyGradesCountAsync(TermlyGradeQueryDTO query)
    {
        if (query == null)
            return Task.FromResult(0);
        return GetTotalTermlyGradesCountAsync(query.TermId, query.ClassId, query.SubjectId);
    }

    public async Task<int> GetTotalTermlyGradesCountAsync(int term, int classId, int subjectId)
    {
        var activeYear = await GetActiveYearAsync();
        if (activeYear == null)
            return 0;

        // Match GetAllAsync grouping: one UI row per (student, subject) pair.
        return await FilterTermlyGrades(term, classId, subjectId, activeYear.YearID)
            .Select(g => new { g.StudentID, g.SubjectID })
            .Distinct()
            .CountAsync();
    }

    private async Task<Year?> GetActiveYearAsync()
    {
        return await _context.Years
            .Where(y => y.Active)
            .OrderBy(y => y.YearID)
            .FirstOrDefaultAsync();
    }

    /// <summary>yearId from route/body is ignored; <paramref name="activeYearId"/> is used.</summary>
    private IQueryable<TermlyGrade> FilterTermlyGrades(int term, int classId, int subjectId, int activeYearId)
    {
        var q = _context.TermlyGrades
            .Where(g => g.TermID == term && g.ClassID == classId && g.YearID == activeYearId);
        if (subjectId != 0)
            q = q.Where(g => g.SubjectID == subjectId);
        return q;
    }
}
