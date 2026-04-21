using System.Text.Json;
using Backend.Data;
using Backend.DTOS.School.TeacherFeedback;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School;

public class TeacherFeedbackRepository : ITeacherFeedbackRepository
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    private readonly TenantDbContext _db;

    public TeacherFeedbackRepository(TenantDbContext db)
    {
        _db = db;
    }

    private static string FormatTeacherName(Name? n)
    {
        if (n == null) return string.Empty;
        return string.Join(" ", new[] { n.FirstName, n.MiddleName, n.LastName }
            .Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
    }

    public Task<int?> GetSchoolIdForCycleAsync(int cycleId, CancellationToken cancellationToken = default) =>
        _db.TeacherFeedbackCycles.AsNoTracking()
            .Where(c => c.TeacherFeedbackCycleID == cycleId)
            .Select(c => (int?)c.SchoolID)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<TeacherFeedbackCycleListItemDto>> ListCyclesAsync(
        TeacherFeedbackCycleFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        filter ??= new TeacherFeedbackCycleFilterDto();
        var q = _db.TeacherFeedbackCycles.AsNoTracking().AsQueryable();
        if (filter.SchoolID is > 0)
            q = q.Where(c => c.SchoolID == filter.SchoolID);
        if (filter.AcademicYearID is > 0)
            q = q.Where(c => c.AcademicYearID == filter.AcademicYearID);
        if (filter.TeacherID is > 0)
            q = q.Where(c => c.TeacherID == filter.TeacherID);
        if (filter.Status is int st && Enum.IsDefined(typeof(TeacherFeedbackCycleStatus), st))
            q = q.Where(c => (int)c.Status == st);

        var rows = await q
            .OrderByDescending(c => c.TeacherFeedbackCycleID)
            .Select(c => new
            {
                c.TeacherFeedbackCycleID,
                c.SchoolID,
                c.AcademicYearID,
                c.TeacherID,
                Fn = c.Teacher.FullName.FirstName,
                Mn = c.Teacher.FullName.MiddleName,
                Ln = c.Teacher.FullName.LastName,
                c.Title,
                c.OpensAtUtc,
                c.ClosesAtUtc,
                c.Status,
                QuestionCount = c.Questions.Count,
                StudentSubmittedCount = c.StudentFeedbacks.Count(s => s.Status == FeedbackSubmissionStatus.Submitted),
                ParentSubmittedCount = c.ParentFeedbacks.Count(p => p.Status == FeedbackSubmissionStatus.Submitted),
            })
            .ToListAsync(cancellationToken);

        return rows.Select(c => new TeacherFeedbackCycleListItemDto
        {
            TeacherFeedbackCycleID = c.TeacherFeedbackCycleID,
            SchoolID = c.SchoolID,
            AcademicYearID = c.AcademicYearID,
            TeacherID = c.TeacherID,
            TeacherName = FormatTeacherName(new Name { FirstName = c.Fn, MiddleName = c.Mn, LastName = c.Ln }),
            Title = c.Title,
            OpensAtUtc = c.OpensAtUtc,
            ClosesAtUtc = c.ClosesAtUtc,
            Status = (int)c.Status,
            QuestionCount = c.QuestionCount,
            StudentSubmittedCount = c.StudentSubmittedCount,
            ParentSubmittedCount = c.ParentSubmittedCount,
        }).ToList();
    }

    public async Task<TeacherFeedbackCycleDetailDto?> GetCycleByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var c = await _db.TeacherFeedbackCycles.AsNoTracking()
            .Include(x => x.Questions)
            .Include(x => x.Summaries)
            .FirstOrDefaultAsync(x => x.TeacherFeedbackCycleID == id, cancellationToken);
        if (c == null) return null;

        var teacherName = await _db.Teachers.AsNoTracking()
            .Where(t => t.TeacherID == c.TeacherID)
            .Select(t => new { t.FullName.FirstName, t.FullName.MiddleName, t.FullName.LastName })
            .FirstOrDefaultAsync(cancellationToken);

        return new TeacherFeedbackCycleDetailDto
        {
            TeacherFeedbackCycleID = c.TeacherFeedbackCycleID,
            SchoolID = c.SchoolID,
            AcademicYearID = c.AcademicYearID,
            TeacherID = c.TeacherID,
            TeacherName = teacherName == null
                ? null
                : FormatTeacherName(new Name
                {
                    FirstName = teacherName.FirstName,
                    MiddleName = teacherName.MiddleName,
                    LastName = teacherName.LastName,
                }),
            Title = c.Title,
            Description = c.Description,
            OpensAtUtc = c.OpensAtUtc,
            ClosesAtUtc = c.ClosesAtUtc,
            Status = (int)c.Status,
            QuestionCount = c.Questions.Count,
            StudentSubmittedCount = await _db.StudentFeedbacks.CountAsync(
                s => s.TeacherFeedbackCycleID == id && s.Status == FeedbackSubmissionStatus.Submitted,
                cancellationToken),
            ParentSubmittedCount = await _db.ParentFeedbacks.CountAsync(
                p => p.TeacherFeedbackCycleID == id && p.Status == FeedbackSubmissionStatus.Submitted,
                cancellationToken),
            Questions = c.Questions.OrderBy(q => q.SortOrder).Select(q => new FeedbackQuestionDto
            {
                FeedbackQuestionID = q.FeedbackQuestionID,
                TeacherFeedbackCycleID = q.TeacherFeedbackCycleID,
                SortOrder = q.SortOrder,
                QuestionText = q.QuestionText,
                QuestionType = (int)q.QuestionType,
                Audience = (int)q.Audience,
                IsRequired = q.IsRequired,
            }).ToList(),
            Summaries = c.Summaries.Select(s => new FeedbackSummaryDto
            {
                FeedbackSummaryID = s.FeedbackSummaryID,
                TeacherFeedbackCycleID = s.TeacherFeedbackCycleID,
                Audience = (int)s.Audience,
                SubmittedCount = s.SubmittedCount,
                AverageNumericScore = s.AverageNumericScore,
                AggregateJson = s.AggregateJson,
                Notes = s.Notes,
                ComputedAtUtc = s.ComputedAtUtc,
            }).ToList(),
        };
    }

    public async Task<int> CreateCycleAsync(TeacherFeedbackCycleWriteDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new TeacherFeedbackCycle
        {
            SchoolID = dto.SchoolID,
            AcademicYearID = dto.AcademicYearID,
            TeacherID = dto.TeacherID,
            Title = dto.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            OpensAtUtc = dto.OpensAtUtc.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dto.OpensAtUtc, DateTimeKind.Utc)
                : dto.OpensAtUtc.ToUniversalTime(),
            ClosesAtUtc = dto.ClosesAtUtc.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dto.ClosesAtUtc, DateTimeKind.Utc)
                : dto.ClosesAtUtc.ToUniversalTime(),
            Status = (TeacherFeedbackCycleStatus)dto.Status,
        };
        if (entity.ClosesAtUtc < entity.OpensAtUtc)
            throw new InvalidOperationException("ClosesAtUtc must be after OpensAtUtc.");

        _db.TeacherFeedbackCycles.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        if (dto.Questions is { Count: > 0 })
            await ReplaceQuestionsAsync(entity.TeacherFeedbackCycleID, dto.Questions, cancellationToken);

        return entity.TeacherFeedbackCycleID;
    }

    public async Task UpdateCycleAsync(int id, TeacherFeedbackCycleWriteDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _db.TeacherFeedbackCycles.FirstOrDefaultAsync(c => c.TeacherFeedbackCycleID == id, cancellationToken)
            ?? throw new InvalidOperationException("Cycle not found.");

        entity.SchoolID = dto.SchoolID;
        entity.AcademicYearID = dto.AcademicYearID;
        entity.TeacherID = dto.TeacherID;
        entity.Title = dto.Title.Trim();
        entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        entity.OpensAtUtc = dto.OpensAtUtc.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(dto.OpensAtUtc, DateTimeKind.Utc)
            : dto.OpensAtUtc.ToUniversalTime();
        entity.ClosesAtUtc = dto.ClosesAtUtc.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(dto.ClosesAtUtc, DateTimeKind.Utc)
            : dto.ClosesAtUtc.ToUniversalTime();
        entity.Status = (TeacherFeedbackCycleStatus)dto.Status;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        if (entity.ClosesAtUtc < entity.OpensAtUtc)
            throw new InvalidOperationException("ClosesAtUtc must be after OpensAtUtc.");

        if (dto.Questions != null)
            await ReplaceQuestionsAsync(id, dto.Questions, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task ReplaceQuestionsAsync(int cycleId, List<FeedbackQuestionWriteDto> questions, CancellationToken ct)
    {
        var existing = await _db.FeedbackQuestions.Where(q => q.TeacherFeedbackCycleID == cycleId).ToListAsync(ct);
        _db.FeedbackQuestions.RemoveRange(existing);

        foreach (var q in questions.OrderBy(x => x.SortOrder))
        {
            _db.FeedbackQuestions.Add(new FeedbackQuestion
            {
                TeacherFeedbackCycleID = cycleId,
                SortOrder = q.SortOrder,
                QuestionText = q.QuestionText.Trim(),
                QuestionType = (FeedbackQuestionType)q.QuestionType,
                Audience = (FeedbackQuestionAudience)q.Audience,
                IsRequired = q.IsRequired,
            });
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteCycleAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.TeacherFeedbackCycles.FirstOrDefaultAsync(c => c.TeacherFeedbackCycleID == id, cancellationToken);
        if (entity == null) return false;
        _db.TeacherFeedbackCycles.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static bool IsInSubmissionWindow(TeacherFeedbackCycle c)
    {
        var now = DateTime.UtcNow;
        return c.Status == TeacherFeedbackCycleStatus.Active && now >= c.OpensAtUtc && now <= c.ClosesAtUtc;
    }

    private static bool QuestionAppliesToStudent(FeedbackQuestionAudience a) =>
        a is FeedbackQuestionAudience.StudentsOnly or FeedbackQuestionAudience.Both;

    private static bool QuestionAppliesToParent(FeedbackQuestionAudience a) =>
        a is FeedbackQuestionAudience.ParentsOnly or FeedbackQuestionAudience.Both;

    private static void ValidateResponses(
        List<FeedbackQuestion> questions,
        List<FeedbackResponseItemDto> responses,
        bool forStudent)
    {
        var map = responses.ToDictionary(r => r.QuestionId, r => r);
        foreach (var q in questions)
        {
            if (forStudent && !QuestionAppliesToStudent(q.Audience)) continue;
            if (!forStudent && !QuestionAppliesToParent(q.Audience)) continue;
            if (!q.IsRequired) continue;
            if (!map.TryGetValue(q.FeedbackQuestionID, out var r))
                throw new InvalidOperationException($"Missing answer for question #{q.FeedbackQuestionID}.");
            switch (q.QuestionType)
            {
                case FeedbackQuestionType.Rating1To5:
                    if (r.Rating is not int rv || rv < 1 || rv > 5)
                        throw new InvalidOperationException($"Question #{q.FeedbackQuestionID} requires a rating 1–5.");
                    break;
                case FeedbackQuestionType.Text:
                    if (string.IsNullOrWhiteSpace(r.Text))
                        throw new InvalidOperationException($"Question #{q.FeedbackQuestionID} requires text.");
                    break;
                case FeedbackQuestionType.YesNo:
                    if (r.YesNo is null)
                        throw new InvalidOperationException($"Question #{q.FeedbackQuestionID} requires yes/no.");
                    break;
            }
        }
    }

    public async Task UpsertStudentFeedbackAsync(int studentId, StudentFeedbackSubmitDto dto, CancellationToken cancellationToken = default)
    {
        var cycle = await _db.TeacherFeedbackCycles
            .Include(c => c.Questions)
            .FirstOrDefaultAsync(c => c.TeacherFeedbackCycleID == dto.TeacherFeedbackCycleID, cancellationToken)
            ?? throw new InvalidOperationException("Cycle not found.");

        if (!IsInSubmissionWindow(cycle))
            throw new InvalidOperationException("This feedback cycle is not open for submissions.");

        if (dto.Submit)
            ValidateResponses(cycle.Questions.ToList(), dto.Responses, forStudent: true);

        var row = await _db.StudentFeedbacks
            .FirstOrDefaultAsync(
                s => s.TeacherFeedbackCycleID == dto.TeacherFeedbackCycleID && s.StudentID == studentId,
                cancellationToken);
        if (row == null)
        {
            row = new StudentFeedback
            {
                TeacherFeedbackCycleID = dto.TeacherFeedbackCycleID,
                StudentID = studentId,
            };
            _db.StudentFeedbacks.Add(row);
        }
        else if (row.Status == FeedbackSubmissionStatus.Submitted)
            throw new InvalidOperationException("Feedback was already submitted and cannot be changed.");

        row.ResponsesJson = JsonSerializer.Serialize(dto.Responses, JsonOpts);
        row.UpdatedAtUtc = DateTime.UtcNow;
        if (dto.Submit)
        {
            row.Status = FeedbackSubmissionStatus.Submitted;
            row.SubmittedAtUtc = DateTime.UtcNow;
        }
        else
            row.Status = FeedbackSubmissionStatus.Draft;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpsertParentFeedbackAsync(int guardianId, ParentFeedbackSubmitDto dto, CancellationToken cancellationToken = default)
    {
        var student = await _db.Students.AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentID == dto.StudentID, cancellationToken)
            ?? throw new InvalidOperationException("Student not found.");
        if (student.GuardianID != guardianId)
            throw new InvalidOperationException("This student is not linked to your guardian account.");

        var cycle = await _db.TeacherFeedbackCycles
            .Include(c => c.Questions)
            .FirstOrDefaultAsync(c => c.TeacherFeedbackCycleID == dto.TeacherFeedbackCycleID, cancellationToken)
            ?? throw new InvalidOperationException("Cycle not found.");

        if (!IsInSubmissionWindow(cycle))
            throw new InvalidOperationException("This feedback cycle is not open for submissions.");

        if (dto.Submit)
            ValidateResponses(cycle.Questions.ToList(), dto.Responses, forStudent: false);

        var row = await _db.ParentFeedbacks.FirstOrDefaultAsync(
            p => p.TeacherFeedbackCycleID == dto.TeacherFeedbackCycleID
                 && p.GuardianID == guardianId
                 && p.StudentID == dto.StudentID,
            cancellationToken);
        if (row == null)
        {
            row = new ParentFeedback
            {
                TeacherFeedbackCycleID = dto.TeacherFeedbackCycleID,
                GuardianID = guardianId,
                StudentID = dto.StudentID,
            };
            _db.ParentFeedbacks.Add(row);
        }
        else if (row.Status == FeedbackSubmissionStatus.Submitted)
            throw new InvalidOperationException("Feedback was already submitted and cannot be changed.");

        row.ResponsesJson = JsonSerializer.Serialize(dto.Responses, JsonOpts);
        row.UpdatedAtUtc = DateTime.UtcNow;
        if (dto.Submit)
        {
            row.Status = FeedbackSubmissionStatus.Submitted;
            row.SubmittedAtUtc = DateTime.UtcNow;
        }
        else
            row.Status = FeedbackSubmissionStatus.Draft;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RecomputeSummariesAsync(int cycleId, CancellationToken cancellationToken = default)
    {
        var ratingQuestions = await _db.FeedbackQuestions.AsNoTracking()
            .Where(q => q.TeacherFeedbackCycleID == cycleId && q.QuestionType == FeedbackQuestionType.Rating1To5)
            .OrderBy(q => q.SortOrder)
            .ToListAsync(cancellationToken);

        var studentJson = await _db.StudentFeedbacks.AsNoTracking()
            .Where(s => s.TeacherFeedbackCycleID == cycleId && s.Status == FeedbackSubmissionStatus.Submitted)
            .Select(s => s.ResponsesJson)
            .ToListAsync(cancellationToken);
        var parentJson = await _db.ParentFeedbacks.AsNoTracking()
            .Where(p => p.TeacherFeedbackCycleID == cycleId && p.Status == FeedbackSubmissionStatus.Submitted)
            .Select(p => p.ResponsesJson)
            .ToListAsync(cancellationToken);

        var olds = await _db.FeedbackSummaries.Where(s => s.TeacherFeedbackCycleID == cycleId).ToListAsync(cancellationToken);
        _db.FeedbackSummaries.RemoveRange(olds);
        await _db.SaveChangesAsync(cancellationToken);

        var qsStudent = ratingQuestions.Where(q => QuestionAppliesToStudent(q.Audience)).ToList();
        var qsParent = ratingQuestions.Where(q => QuestionAppliesToParent(q.Audience)).ToList();
        var qsAll = ratingQuestions.ToList();

        static FeedbackSummary BuildSummary(
            int cycleId,
            FeedbackSummaryAudience audience,
            int submittedCount,
            List<string?> jsonRows,
            List<FeedbackQuestion> qs,
            JsonSerializerOptions jsonOpts)
        {
            var allRatings = new List<decimal>();
            var perQ = qs.ToDictionary(q => q.FeedbackQuestionID, _ => new List<int>());

            foreach (var j in jsonRows)
            {
                if (string.IsNullOrWhiteSpace(j)) continue;
                try
                {
                    var items = JsonSerializer.Deserialize<List<FeedbackResponseItemDto>>(j, jsonOpts);
                    if (items == null) continue;
                    foreach (var it in items)
                    {
                        if (it.Rating is not int rv || rv < 1 || rv > 5) continue;
                        var qmeta = qs.FirstOrDefault(q => q.FeedbackQuestionID == it.QuestionId);
                        if (qmeta == null) continue;
                        allRatings.Add(rv);
                        perQ[qmeta.FeedbackQuestionID].Add(rv);
                    }
                }
                catch
                {
                    /* skip malformed */
                }
            }

            var byQuestion = qs.Select(q => new
            {
                questionId = q.FeedbackQuestionID,
                avg = perQ[q.FeedbackQuestionID].Count == 0 ? (double?)null : perQ[q.FeedbackQuestionID].Average(),
                count = perQ[q.FeedbackQuestionID].Count,
            }).ToList();

            var agg = JsonSerializer.Serialize(new { byQuestion }, jsonOpts);
            var avgOverall = allRatings.Count == 0 ? (decimal?)null : (decimal)allRatings.Average(d => (double)d);

            return new FeedbackSummary
            {
                TeacherFeedbackCycleID = cycleId,
                Audience = audience,
                SubmittedCount = submittedCount,
                AverageNumericScore = avgOverall,
                AggregateJson = agg,
                ComputedAtUtc = DateTime.UtcNow,
            };
        }

        _db.FeedbackSummaries.Add(BuildSummary(
            cycleId, FeedbackSummaryAudience.Students, studentJson.Count, studentJson, qsStudent, JsonOpts));
        _db.FeedbackSummaries.Add(BuildSummary(
            cycleId, FeedbackSummaryAudience.Parents, parentJson.Count, parentJson, qsParent, JsonOpts));
        _db.FeedbackSummaries.Add(BuildSummary(
            cycleId,
            FeedbackSummaryAudience.Combined,
            studentJson.Count + parentJson.Count,
            studentJson.Concat(parentJson).ToList(),
            qsAll,
            JsonOpts));

        await _db.SaveChangesAsync(cancellationToken);
    }
}
