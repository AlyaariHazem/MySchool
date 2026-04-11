using Backend.Data;
using Backend.DTOS.School.Exams;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School;

public class ExamRepository : IExamRepository
{
    private readonly TenantDbContext _db;

    public ExamRepository(TenantDbContext db)
    {
        _db = db;
    }

    private static string FormatName(Name? n)
    {
        if (n == null) return string.Empty;
        return string.Join(" ", new[] { n.FirstName, n.MiddleName, n.LastName }
            .Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
    }

    public Task<int?> GetStudentIdByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
        _db.Students.AsNoTracking()
            .Where(s => s.UserID == userId)
            .Select(s => (int?)s.StudentID)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<int?> GetGuardianIdByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
        _db.Guardians.AsNoTracking()
            .Where(g => g.UserID == userId)
            .Select(g => (int?)g.GuardianID)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<ExamTypeDto>> GetExamTypesAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        var q = _db.ExamTypes.AsNoTracking().AsQueryable();
        if (!includeInactive)
            q = q.Where(t => t.IsActive);
        var list = await q.OrderBy(t => t.SortOrder).ThenBy(t => t.Name).ToListAsync(cancellationToken);
        return list.Select(t => new ExamTypeDto
        {
            ExamTypeID = t.ExamTypeID,
            Name = t.Name,
            SortOrder = t.SortOrder,
            IsActive = t.IsActive
        }).ToList();
    }

    public async Task<ExamTypeDto?> UpdateExamTypeAsync(int examTypeId, string name, int sortOrder, bool isActive, CancellationToken cancellationToken = default)
    {
        var t = await _db.ExamTypes.FirstOrDefaultAsync(x => x.ExamTypeID == examTypeId, cancellationToken);
        if (t == null) return null;
        t.Name = name;
        t.SortOrder = sortOrder;
        t.IsActive = isActive;
        await _db.SaveChangesAsync(cancellationToken);
        return new ExamTypeDto { ExamTypeID = t.ExamTypeID, Name = t.Name, SortOrder = t.SortOrder, IsActive = t.IsActive };
    }

    public async Task<IReadOnlyList<ExamSessionDto>> GetExamSessionsAsync(int? yearId, int? termId, CancellationToken cancellationToken = default)
    {
        var q = _db.ExamSessions.AsNoTracking().AsQueryable();
        if (yearId.HasValue) q = q.Where(s => s.YearID == yearId.Value);
        if (termId.HasValue) q = q.Where(s => s.TermID == termId.Value);
        var list = await q.OrderByDescending(s => s.ExamSessionID).ToListAsync(cancellationToken);
        return list.Select(s => new ExamSessionDto
        {
            ExamSessionID = s.ExamSessionID,
            YearID = s.YearID,
            TermID = s.TermID,
            Name = s.Name,
            IsActive = s.IsActive
        }).ToList();
    }

    public async Task<ExamSessionDto> CreateExamSessionAsync(CreateExamSessionDto dto, CancellationToken cancellationToken = default)
    {
        var e = new ExamSession
        {
            YearID = dto.YearID,
            TermID = dto.TermID,
            Name = dto.Name,
            IsActive = dto.IsActive
        };
        await _db.ExamSessions.AddAsync(e, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return new ExamSessionDto
        {
            ExamSessionID = e.ExamSessionID,
            YearID = e.YearID,
            TermID = e.TermID,
            Name = e.Name,
            IsActive = e.IsActive
        };
    }

    public async Task<ExamSessionDto?> UpdateExamSessionAsync(UpdateExamSessionDto dto, CancellationToken cancellationToken = default)
    {
        var e = await _db.ExamSessions.FirstOrDefaultAsync(x => x.ExamSessionID == dto.ExamSessionID, cancellationToken);
        if (e == null) return null;
        e.YearID = dto.YearID;
        e.TermID = dto.TermID;
        e.Name = dto.Name;
        e.IsActive = dto.IsActive;
        await _db.SaveChangesAsync(cancellationToken);
        return new ExamSessionDto
        {
            ExamSessionID = e.ExamSessionID,
            YearID = e.YearID,
            TermID = e.TermID,
            Name = e.Name,
            IsActive = e.IsActive
        };
    }

    public async Task DeleteExamSessionAsync(int examSessionId, CancellationToken cancellationToken = default)
    {
        var e = await _db.ExamSessions.FirstOrDefaultAsync(x => x.ExamSessionID == examSessionId, cancellationToken);
        if (e == null) return;
        _db.ExamSessions.Remove(e);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<ScheduledExam> ScheduledQuery() =>
        _db.ScheduledExams
            .AsNoTracking()
            .Include(se => se.ExamType)
            .Include(se => se.Class)
            .Include(se => se.Division)
            .Include(se => se.Subject)
            .Include(se => se.Teacher);

    private static ScheduledExamListDto MapScheduledList(ScheduledExam se)
    {
        var teacherName = se.Teacher != null ? FormatName(se.Teacher.FullName) : null;
        return new ScheduledExamListDto
        {
            ScheduledExamID = se.ScheduledExamID,
            ExamSessionID = se.ExamSessionID,
            ExamTypeID = se.ExamTypeID,
            ExamTypeName = se.ExamType?.Name,
            YearID = se.YearID,
            TermID = se.TermID,
            ClassID = se.ClassID,
            ClassName = se.Class?.ClassName,
            DivisionID = se.DivisionID,
            DivisionName = se.Division?.DivisionName,
            SubjectID = se.SubjectID,
            SubjectName = se.Subject?.SubjectName,
            TeacherID = se.TeacherID,
            TeacherName = teacherName,
            ExamDate = se.ExamDate,
            StartTime = se.StartTime,
            EndTime = se.EndTime,
            Room = se.Room,
            TotalMarks = se.TotalMarks,
            PassingMarks = se.PassingMarks,
            SchedulePublished = se.SchedulePublished,
            ResultsPublished = se.ResultsPublished,
            Notes = se.Notes
        };
    }

    public async Task<IReadOnlyList<ScheduledExamListDto>> GetScheduledExamsAsync(ExamFilterQuery filter, CancellationToken cancellationToken = default)
    {
        var q = ScheduledQuery();
        if (filter.YearID.HasValue) q = q.Where(se => se.YearID == filter.YearID.Value);
        if (filter.TermID.HasValue) q = q.Where(se => se.TermID == filter.TermID.Value);
        if (filter.ClassID.HasValue) q = q.Where(se => se.ClassID == filter.ClassID.Value);
        if (filter.DivisionID.HasValue) q = q.Where(se => se.DivisionID == filter.DivisionID.Value);
        if (filter.SubjectID.HasValue) q = q.Where(se => se.SubjectID == filter.SubjectID.Value);
        if (filter.TeacherID.HasValue) q = q.Where(se => se.TeacherID == filter.TeacherID.Value);
        if (filter.UpcomingOnly == true)
        {
            var today = DateTime.UtcNow.Date;
            q = q.Where(se => se.ExamDate.Date >= today);
        }

        var list = await q.OrderBy(se => se.ExamDate).ThenBy(se => se.StartTime).ToListAsync(cancellationToken);
        return list.Select(MapScheduledList).ToList();
    }

    public async Task<ScheduledExamListDto?> GetScheduledExamByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var se = await ScheduledQuery().FirstOrDefaultAsync(x => x.ScheduledExamID == id, cancellationToken);
        return se == null ? null : MapScheduledList(se);
    }

    private async Task SyncResultStubsForDivisionAsync(int scheduledExamId, int divisionId, CancellationToken cancellationToken)
    {
        var studentIds = await _db.Students.AsNoTracking()
            .Where(s => s.DivisionID == divisionId)
            .Select(s => s.StudentID)
            .ToListAsync(cancellationToken);

        var existing = await _db.ExamResults
            .Where(r => r.ScheduledExamID == scheduledExamId)
            .ToListAsync(cancellationToken);

        var existingSet = existing.Select(r => r.StudentID).ToHashSet();
        foreach (var sid in studentIds.Where(sid => !existingSet.Contains(sid)))
        {
            await _db.ExamResults.AddAsync(new ExamResult
            {
                ScheduledExamID = scheduledExamId,
                StudentID = sid,
                IsAbsent = false,
                Score = null
            }, cancellationToken);
        }

        var idSet = studentIds.ToHashSet();
        foreach (var row in existing.Where(r => !idSet.Contains(r.StudentID)))
            _db.ExamResults.Remove(row);

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<ScheduledExamListDto> CreateScheduledExamAsync(CreateScheduledExamDto dto, CancellationToken cancellationToken = default)
    {
        var se = new ScheduledExam
        {
            ExamSessionID = dto.ExamSessionID,
            ExamTypeID = dto.ExamTypeID,
            YearID = dto.YearID,
            TermID = dto.TermID,
            ClassID = dto.ClassID,
            DivisionID = dto.DivisionID,
            SubjectID = dto.SubjectID,
            TeacherID = dto.TeacherID,
            ExamDate = dto.ExamDate.Date,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Room = dto.Room,
            TotalMarks = dto.TotalMarks,
            PassingMarks = dto.PassingMarks,
            SchedulePublished = dto.SchedulePublished,
            ResultsPublished = dto.ResultsPublished,
            Notes = dto.Notes,
            CreatedAtUtc = DateTime.UtcNow
        };
        await _db.ScheduledExams.AddAsync(se, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        await SyncResultStubsForDivisionAsync(se.ScheduledExamID, se.DivisionID, cancellationToken);

        return (await GetScheduledExamByIdAsync(se.ScheduledExamID, cancellationToken))!;
    }

    public async Task<ScheduledExamListDto?> UpdateScheduledExamAsync(UpdateScheduledExamDto dto, CancellationToken cancellationToken = default)
    {
        var se = await _db.ScheduledExams.FirstOrDefaultAsync(x => x.ScheduledExamID == dto.ScheduledExamID, cancellationToken);
        if (se == null) return null;

        var divChanged = se.DivisionID != dto.DivisionID;

        se.ExamSessionID = dto.ExamSessionID;
        se.ExamTypeID = dto.ExamTypeID;
        se.YearID = dto.YearID;
        se.TermID = dto.TermID;
        se.ClassID = dto.ClassID;
        se.DivisionID = dto.DivisionID;
        se.SubjectID = dto.SubjectID;
        se.TeacherID = dto.TeacherID;
        se.ExamDate = dto.ExamDate.Date;
        se.StartTime = dto.StartTime;
        se.EndTime = dto.EndTime;
        se.Room = dto.Room;
        se.TotalMarks = dto.TotalMarks;
        se.PassingMarks = dto.PassingMarks;
        se.SchedulePublished = dto.SchedulePublished;
        se.ResultsPublished = dto.ResultsPublished;
        se.Notes = dto.Notes;
        se.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        if (divChanged)
            await SyncResultStubsForDivisionAsync(se.ScheduledExamID, se.DivisionID, cancellationToken);

        return await GetScheduledExamByIdAsync(se.ScheduledExamID, cancellationToken);
    }

    public async Task DeleteScheduledExamAsync(int id, CancellationToken cancellationToken = default)
    {
        var se = await _db.ScheduledExams.FirstOrDefaultAsync(x => x.ScheduledExamID == id, cancellationToken);
        if (se == null) return;
        _db.ScheduledExams.Remove(se);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScheduledExamListDto>> GetTeacherScheduledExamsAsync(int teacherId, ExamFilterQuery filter, CancellationToken cancellationToken = default)
    {
        filter.TeacherID = teacherId;
        return await GetScheduledExamsAsync(filter, cancellationToken);
    }

    public async Task<IReadOnlyList<ExamResultRowDto>> GetExamResultsAsync(int scheduledExamId, CancellationToken cancellationToken = default)
    {
        var se = await _db.ScheduledExams.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ScheduledExamID == scheduledExamId, cancellationToken);
        if (se == null) return Array.Empty<ExamResultRowDto>();

        // Ensure one row per student in the division (handles students added after the exam was scheduled, or missing stubs).
        await SyncResultStubsForDivisionAsync(scheduledExamId, se.DivisionID, cancellationToken);

        var rows = await _db.ExamResults.AsNoTracking()
            .Where(r => r.ScheduledExamID == scheduledExamId)
            .Include(r => r.Student)
            .OrderBy(r => r.StudentID)
            .ToListAsync(cancellationToken);

        return rows.Select(r => new ExamResultRowDto
        {
            ExamResultID = r.ExamResultID,
            StudentID = r.StudentID,
            StudentName = r.Student != null ? FormatName(r.Student.FullName) : null,
            Score = r.Score,
            IsAbsent = r.IsAbsent,
            Remarks = r.Remarks
        }).ToList();
    }

    public async Task SaveExamResultsAsync(int scheduledExamId, BulkExamResultsDto dto, CancellationToken cancellationToken = default)
    {
        var se = await _db.ScheduledExams.FirstOrDefaultAsync(x => x.ScheduledExamID == scheduledExamId, cancellationToken)
            ?? throw new InvalidOperationException("Scheduled exam not found.");

        foreach (var row in dto.Rows)
        {
            var er = await _db.ExamResults.FirstOrDefaultAsync(
                r => r.ExamResultID == row.ExamResultID && r.ScheduledExamID == scheduledExamId, cancellationToken);
            if (er == null) continue;
            if (er.StudentID != row.StudentID) continue;

            if (!row.IsAbsent && row.Score.HasValue)
            {
                if (row.Score.Value < 0 || row.Score.Value > se.TotalMarks)
                    throw new InvalidOperationException($"Score must be between 0 and {se.TotalMarks}.");
            }

            er.Score = row.IsAbsent ? null : row.Score;
            er.IsAbsent = row.IsAbsent;
            er.Remarks = row.Remarks;
        }

        se.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task PublishResultsAsync(int scheduledExamId, bool publish, CancellationToken cancellationToken = default)
    {
        var se = await _db.ScheduledExams.FirstOrDefaultAsync(x => x.ScheduledExamID == scheduledExamId, cancellationToken)
            ?? throw new InvalidOperationException("Scheduled exam not found.");
        se.ResultsPublished = publish;
        se.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task PublishScheduleAsync(int scheduledExamId, bool publish, CancellationToken cancellationToken = default)
    {
        var se = await _db.ScheduledExams.FirstOrDefaultAsync(x => x.ScheduledExamID == scheduledExamId, cancellationToken)
            ?? throw new InvalidOperationException("Scheduled exam not found.");
        se.SchedulePublished = publish;
        se.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StudentExamCardDto>> GetStudentExamsAsync(int studentId, bool upcomingOnly, CancellationToken cancellationToken = default)
    {
        var student = await _db.Students.AsNoTracking().FirstOrDefaultAsync(s => s.StudentID == studentId, cancellationToken);
        if (student == null) return Array.Empty<StudentExamCardDto>();

        var today = DateTime.UtcNow.Date;

        var rows = await _db.ExamResults.AsNoTracking()
            .Where(r => r.StudentID == studentId)
            .Include(r => r.ScheduledExam)
            .ThenInclude(se => se!.ExamType)
            .Include(r => r.ScheduledExam)
            .ThenInclude(se => se!.Subject)
            .Include(r => r.ScheduledExam)
            .ThenInclude(se => se!.Class)
            .Include(r => r.ScheduledExam)
            .ThenInclude(se => se!.Division)
            .ToListAsync(cancellationToken);

        IEnumerable<ExamResult> filtered = rows;
        if (upcomingOnly)
            filtered = rows.Where(r =>
                r.ScheduledExam!.ExamDate.Date >= today && r.ScheduledExam.SchedulePublished);

        return filtered
            .OrderBy(r => r.ScheduledExam!.ExamDate)
            .Select(r =>
            {
                var se = r.ScheduledExam!;
                var passed = se.ResultsPublished && !r.IsAbsent && r.Score.HasValue && r.Score >= se.PassingMarks;
                return new StudentExamCardDto
                {
                    ScheduledExamID = se.ScheduledExamID,
                    ExamTypeName = se.ExamType?.Name ?? "",
                    SubjectName = se.Subject?.SubjectName ?? "",
                    ClassName = se.Class?.ClassName ?? "",
                    DivisionName = se.Division?.DivisionName ?? "",
                    ExamDate = se.ExamDate,
                    StartTime = se.StartTime,
                    EndTime = se.EndTime,
                    Room = se.Room,
                    SchedulePublished = se.SchedulePublished,
                    ResultsPublished = se.ResultsPublished,
                    TotalMarks = se.TotalMarks,
                    PassingMarks = se.PassingMarks,
                    Score = se.ResultsPublished ? r.Score : null,
                    IsAbsent = se.ResultsPublished && r.IsAbsent,
                    Remarks = se.ResultsPublished ? r.Remarks : null,
                    Passed = passed
                };
            })
            .ToList();
    }

    public async Task<IReadOnlyList<StudentExamCardDto>> GetGuardianStudentExamsAsync(int guardianId, int studentId, bool upcomingOnly, CancellationToken cancellationToken = default)
    {
        var ok = await _db.Students.AsNoTracking()
            .AnyAsync(s => s.StudentID == studentId && s.GuardianID == guardianId, cancellationToken);
        if (!ok) return Array.Empty<StudentExamCardDto>();
        return await GetStudentExamsAsync(studentId, upcomingOnly, cancellationToken);
    }

    public async Task<ClassExamSheetReportDto?> GetClassExamSheetAsync(int scheduledExamId, CancellationToken cancellationToken = default)
    {
        var se = await _db.ScheduledExams.AsNoTracking()
            .Include(x => x.ExamType)
            .Include(x => x.Class)
            .Include(x => x.Division)
            .Include(x => x.Subject)
            .FirstOrDefaultAsync(x => x.ScheduledExamID == scheduledExamId, cancellationToken);
        if (se == null) return null;

        var rows = await GetExamResultsAsync(scheduledExamId, cancellationToken);
        var list = rows.ToList();
        var graded = list.Where(r => !r.IsAbsent && r.Score.HasValue).ToList();
        var avg = graded.Count == 0 ? 0 : graded.Average(r => r.Score!.Value);
        var pass = list.Count(r => !r.IsAbsent && r.Score.HasValue && r.Score >= se.PassingMarks);
        var fail = list.Count(r => !r.IsAbsent && r.Score.HasValue && r.Score < se.PassingMarks);
        var abs = list.Count(r => r.IsAbsent);

        return new ClassExamSheetReportDto
        {
            ScheduledExamID = se.ScheduledExamID,
            ExamTypeName = se.ExamType?.Name ?? "",
            SubjectName = se.Subject?.SubjectName ?? "",
            ClassName = se.Class?.ClassName ?? "",
            DivisionName = se.Division?.DivisionName ?? "",
            ExamDate = se.ExamDate,
            TotalMarks = se.TotalMarks,
            PassingMarks = se.PassingMarks,
            Rows = list,
            AverageScore = avg,
            PassCount = pass,
            FailCount = fail,
            AbsentCount = abs
        };
    }

    public async Task<IReadOnlyList<SubjectPerformanceReportDto>> GetSubjectPerformanceAsync(int yearId, int termId, int? classId, int? divisionId, CancellationToken cancellationToken = default)
    {
        var q =
            from r in _db.ExamResults.AsNoTracking()
            join se in _db.ScheduledExams.AsNoTracking() on r.ScheduledExamID equals se.ScheduledExamID
            where se.YearID == yearId && se.TermID == termId && se.ResultsPublished && !r.IsAbsent && r.Score != null
            select new { r, se };

        if (classId.HasValue) q = q.Where(x => x.se.ClassID == classId.Value);
        if (divisionId.HasValue) q = q.Where(x => x.se.DivisionID == divisionId.Value);

        var grouped = await q
            .GroupBy(x => x.se.SubjectID)
            .Select(g => new
            {
                SubjectID = g.Key,
                Avg = g.Average(x => x.r.Score!.Value),
                Cnt = g.Count()
            })
            .ToListAsync(cancellationToken);

        var subjectIds = grouped.Select(x => x.SubjectID).ToList();
        var names = await _db.Subjects.AsNoTracking()
            .Where(s => subjectIds.Contains(s.SubjectID))
            .ToDictionaryAsync(s => s.SubjectID, s => s.SubjectName ?? "", cancellationToken);

        return grouped.Select(g => new SubjectPerformanceReportDto
        {
            SubjectID = g.SubjectID,
            SubjectName = names.GetValueOrDefault(g.SubjectID) ?? "",
            ExamCount = g.Cnt,
            AverageScore = g.Avg
        }).OrderBy(x => x.SubjectName).ToList();
    }

    public async Task<IReadOnlyList<TopWeakStudentDto>> GetTopStudentsAsync(int scheduledExamId, int take, CancellationToken cancellationToken = default)
    {
        var rows = await _db.ExamResults.AsNoTracking()
            .Where(r => r.ScheduledExamID == scheduledExamId && !r.IsAbsent && r.Score != null)
            .Include(r => r.Student)
            .OrderByDescending(r => r.Score)
            .Take(take)
            .ToListAsync(cancellationToken);

        return rows.Select(r => new TopWeakStudentDto
        {
            StudentID = r.StudentID,
            StudentName = r.Student != null ? FormatName(r.Student.FullName) : null,
            AverageScore = r.Score!.Value
        }).ToList();
    }

    public async Task<IReadOnlyList<TopWeakStudentDto>> GetWeakStudentsAsync(int scheduledExamId, int take, CancellationToken cancellationToken = default)
    {
        var se = await _db.ScheduledExams.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ScheduledExamID == scheduledExamId, cancellationToken);
        if (se == null) return Array.Empty<TopWeakStudentDto>();

        var rows = await _db.ExamResults.AsNoTracking()
            .Where(r => r.ScheduledExamID == scheduledExamId && !r.IsAbsent && r.Score != null)
            .Include(r => r.Student)
            .OrderBy(r => r.Score)
            .Take(take)
            .ToListAsync(cancellationToken);

        return rows.Select(r => new TopWeakStudentDto
        {
            StudentID = r.StudentID,
            StudentName = r.Student != null ? FormatName(r.Student.FullName) : null,
            AverageScore = r.Score!.Value
        }).ToList();
    }

    public async Task<IReadOnlyList<ExamResultRowDto>> GetAbsentStudentsAsync(int scheduledExamId, CancellationToken cancellationToken = default)
    {
        var rows = await _db.ExamResults.AsNoTracking()
            .Where(r => r.ScheduledExamID == scheduledExamId && r.IsAbsent)
            .Include(r => r.Student)
            .ToListAsync(cancellationToken);

        return rows.Select(r => new ExamResultRowDto
        {
            ExamResultID = r.ExamResultID,
            StudentID = r.StudentID,
            StudentName = r.Student != null ? FormatName(r.Student.FullName) : null,
            Score = r.Score,
            IsAbsent = r.IsAbsent,
            Remarks = r.Remarks
        }).ToList();
    }
}
