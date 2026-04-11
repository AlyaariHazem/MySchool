using Backend.Data;
using Backend.DTOS.School.Homework;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School;

public class HomeworkRepository : IHomeworkRepository
{
    private readonly TenantDbContext _db;

    public HomeworkRepository(TenantDbContext db)
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

    private async Task EnsureTeacherMayAssignAsync(int teacherId, CreateHomeworkTaskDto dto, bool skipCoursePlanCheck, CancellationToken cancellationToken)
    {
        if (skipCoursePlanCheck)
            return;

        var ok = await _db.CoursePlans.AnyAsync(cp =>
            cp.TeacherID == teacherId &&
            cp.YearID == dto.YearID &&
            cp.TermID == dto.TermID &&
            cp.ClassID == dto.ClassID &&
            cp.DivisionID == dto.DivisionID &&
            cp.SubjectID == dto.SubjectID, cancellationToken);

        if (!ok)
            throw new InvalidOperationException(
                "You are not assigned to teach this class, section, and subject for this year and term (course plan).");
    }

    private async Task SyncSubmissionStubsAsync(int homeworkTaskId, int divisionId, CancellationToken cancellationToken)
    {
        var studentIds = await _db.Students.AsNoTracking()
            .Where(s => s.DivisionID == divisionId)
            .Select(s => s.StudentID)
            .ToListAsync(cancellationToken);

        var existing = await _db.HomeworkSubmissions
            .Where(x => x.HomeworkTaskID == homeworkTaskId)
            .Select(x => x.StudentID)
            .ToListAsync(cancellationToken);

        foreach (var sid in studentIds.Where(id => !existing.Contains(id)))
        {
            await _db.HomeworkSubmissions.AddAsync(new HomeworkSubmission
            {
                HomeworkTaskID = homeworkTaskId,
                StudentID = sid,
                Status = HomeworkSubmissionStatus.Pending,
                FeedbackPublished = false
            }, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static HomeworkTaskLinkDto MapLink(HomeworkTaskLink l) => new()
    {
        HomeworkTaskLinkID = l.HomeworkTaskLinkID,
        Url = l.Url,
        Label = l.Label,
        SortOrder = l.SortOrder
    };

    private async Task<HomeworkTaskDetailDto?> MapTaskDetailAsync(HomeworkTask t, CancellationToken cancellationToken)
    {
        var links = await _db.HomeworkTaskLinks.AsNoTracking()
            .Where(l => l.HomeworkTaskID == t.HomeworkTaskID)
            .OrderBy(l => l.SortOrder)
            .ToListAsync(cancellationToken);

        var pending = await _db.HomeworkSubmissions.AsNoTracking()
            .CountAsync(s => s.HomeworkTaskID == t.HomeworkTaskID && s.Status == HomeworkSubmissionStatus.Pending, cancellationToken);
        var done = await _db.HomeworkSubmissions.AsNoTracking()
            .CountAsync(s => s.HomeworkTaskID == t.HomeworkTaskID &&
                s.Status != HomeworkSubmissionStatus.Pending &&
                s.Status != HomeworkSubmissionStatus.Missing, cancellationToken);

        return new HomeworkTaskDetailDto
        {
            HomeworkTaskID = t.HomeworkTaskID,
            TeacherID = t.TeacherID,
            TeacherName = t.Teacher != null ? FormatName(t.Teacher.FullName) : null,
            YearID = t.YearID,
            TermID = t.TermID,
            ClassID = t.ClassID,
            ClassName = t.Class?.ClassName,
            DivisionID = t.DivisionID,
            DivisionName = t.Division?.DivisionName,
            SubjectID = t.SubjectID,
            SubjectName = t.Subject?.SubjectName,
            Title = t.Title,
            Description = t.Description,
            DueDateUtc = t.DueDateUtc,
            SubmissionRequired = t.SubmissionRequired,
            SubmissionCount = done,
            PendingCount = pending,
            CreatedAtUtc = t.CreatedAtUtc,
            Links = links.Select(MapLink).ToList()
        };
    }

    public async Task<HomeworkTaskDetailDto> CreateTaskAsync(int teacherId, CreateHomeworkTaskDto dto, bool skipCoursePlanCheck, CancellationToken cancellationToken = default)
    {
        await EnsureTeacherMayAssignAsync(teacherId, dto, skipCoursePlanCheck, cancellationToken);

        var task = new HomeworkTask
        {
            TeacherID = teacherId,
            YearID = dto.YearID,
            TermID = dto.TermID,
            ClassID = dto.ClassID,
            DivisionID = dto.DivisionID,
            SubjectID = dto.SubjectID,
            Title = dto.Title.Trim(),
            Description = dto.Description,
            DueDateUtc = dto.DueDateUtc,
            SubmissionRequired = dto.SubmissionRequired,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _db.HomeworkTasks.AddAsync(task, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var order = 0;
        if (dto.Links != null)
        {
            foreach (var l in dto.Links.OrderBy(x => x.SortOrder))
            {
                await _db.HomeworkTaskLinks.AddAsync(new HomeworkTaskLink
                {
                    HomeworkTaskID = task.HomeworkTaskID,
                    Url = l.Url.Trim(),
                    Label = l.Label,
                    SortOrder = l.SortOrder != 0 ? l.SortOrder : order++
                }, cancellationToken);
            }
            await _db.SaveChangesAsync(cancellationToken);
        }

        await SyncSubmissionStubsAsync(task.HomeworkTaskID, dto.DivisionID, cancellationToken);

        var full = await _db.HomeworkTasks.AsNoTracking()
            .Include(x => x.Teacher)
            .Include(x => x.Class)
            .Include(x => x.Division)
            .Include(x => x.Subject)
            .FirstAsync(x => x.HomeworkTaskID == task.HomeworkTaskID, cancellationToken);

        return (await MapTaskDetailAsync(full, cancellationToken))!;
    }

    public async Task<HomeworkTaskDetailDto?> UpdateTaskAsync(int homeworkTaskId, int teacherId, UpdateHomeworkTaskDto dto, bool skipCoursePlanCheck, bool privileged, CancellationToken cancellationToken = default)
    {
        var task = await _db.HomeworkTasks
            .Include(t => t.Teacher)
            .Include(t => t.Class)
            .Include(t => t.Division)
            .Include(t => t.Subject)
            .FirstOrDefaultAsync(t => t.HomeworkTaskID == homeworkTaskId, cancellationToken);

        if (task == null) return null;
        if (!privileged && task.TeacherID != teacherId)
            throw new UnauthorizedAccessException("You cannot edit this homework task.");

        await EnsureTeacherMayAssignAsync(privileged ? task.TeacherID : teacherId, dto, skipCoursePlanCheck, cancellationToken);

        if (task.DivisionID != dto.DivisionID || task.ClassID != dto.ClassID)
            throw new InvalidOperationException("Changing class or division is not supported. Delete the task and create a new one.");

        task.YearID = dto.YearID;
        task.TermID = dto.TermID;
        task.SubjectID = dto.SubjectID;
        task.Title = dto.Title.Trim();
        task.Description = dto.Description;
        task.DueDateUtc = dto.DueDateUtc;
        task.SubmissionRequired = dto.SubmissionRequired;
        task.UpdatedAtUtc = DateTime.UtcNow;

        var oldLinks = await _db.HomeworkTaskLinks.Where(l => l.HomeworkTaskID == homeworkTaskId).ToListAsync(cancellationToken);
        _db.HomeworkTaskLinks.RemoveRange(oldLinks);

        var order = 0;
        if (dto.Links != null)
        {
            foreach (var l in dto.Links.OrderBy(x => x.SortOrder))
            {
                await _db.HomeworkTaskLinks.AddAsync(new HomeworkTaskLink
                {
                    HomeworkTaskID = homeworkTaskId,
                    Url = l.Url.Trim(),
                    Label = l.Label,
                    SortOrder = l.SortOrder != 0 ? l.SortOrder : order++
                }, cancellationToken);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        var full = await _db.HomeworkTasks.AsNoTracking()
            .Include(x => x.Teacher)
            .Include(x => x.Class)
            .Include(x => x.Division)
            .Include(x => x.Subject)
            .FirstAsync(x => x.HomeworkTaskID == homeworkTaskId, cancellationToken);

        return await MapTaskDetailAsync(full, cancellationToken);
    }

    public async Task<bool> DeleteTaskAsync(int homeworkTaskId, int teacherId, bool privileged, CancellationToken cancellationToken = default)
    {
        var task = await _db.HomeworkTasks.FirstOrDefaultAsync(t => t.HomeworkTaskID == homeworkTaskId, cancellationToken);
        if (task == null) return false;
        if (!privileged && task.TeacherID != teacherId)
            throw new UnauthorizedAccessException("You cannot delete this homework task.");

        _db.HomeworkTasks.Remove(task);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<HomeworkTaskDetailDto?> GetTaskByIdAsync(int homeworkTaskId, CancellationToken cancellationToken = default)
    {
        var t = await _db.HomeworkTasks.AsNoTracking()
            .Include(x => x.Teacher)
            .Include(x => x.Class)
            .Include(x => x.Division)
            .Include(x => x.Subject)
            .FirstOrDefaultAsync(x => x.HomeworkTaskID == homeworkTaskId, cancellationToken);

        return t == null ? null : await MapTaskDetailAsync(t, cancellationToken);
    }

    private async Task<IReadOnlyList<HomeworkTaskListDto>> MapTaskListAsync(IQueryable<HomeworkTask> query, CancellationToken cancellationToken)
    {
        var list = await query
            .Include(x => x.Teacher)
            .Include(x => x.Class)
            .Include(x => x.Division)
            .Include(x => x.Subject)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var ids = list.Select(t => t.HomeworkTaskID).ToList();
        var counts = await _db.HomeworkSubmissions.AsNoTracking()
            .Where(s => ids.Contains(s.HomeworkTaskID))
            .GroupBy(s => s.HomeworkTaskID)
            .Select(g => new
            {
                Id = g.Key,
                Pending = g.Count(x => x.Status == HomeworkSubmissionStatus.Pending),
                Done = g.Count(x => x.Status != HomeworkSubmissionStatus.Pending && x.Status != HomeworkSubmissionStatus.Missing)
            })
            .ToListAsync(cancellationToken);

        var dict = counts.ToDictionary(x => x.Id);

        return list.Select(t =>
        {
            var c = dict.GetValueOrDefault(t.HomeworkTaskID);
            return new HomeworkTaskListDto
            {
                HomeworkTaskID = t.HomeworkTaskID,
                TeacherID = t.TeacherID,
                TeacherName = t.Teacher != null ? FormatName(t.Teacher.FullName) : null,
                YearID = t.YearID,
                TermID = t.TermID,
                ClassID = t.ClassID,
                ClassName = t.Class?.ClassName,
                DivisionID = t.DivisionID,
                DivisionName = t.Division?.DivisionName,
                SubjectID = t.SubjectID,
                SubjectName = t.Subject?.SubjectName,
                Title = t.Title,
                DueDateUtc = t.DueDateUtc,
                SubmissionRequired = t.SubmissionRequired,
                SubmissionCount = c?.Done ?? 0,
                PendingCount = c?.Pending ?? 0,
                CreatedAtUtc = t.CreatedAtUtc
            };
        }).ToList();
    }

    public Task<IReadOnlyList<HomeworkTaskListDto>> ListTasksForTeacherAsync(int teacherId, HomeworkFilterQuery filter, CancellationToken cancellationToken = default)
    {
        var q = _db.HomeworkTasks.AsNoTracking().Where(t => t.TeacherID == teacherId);
        if (filter.YearID.HasValue) q = q.Where(t => t.YearID == filter.YearID.Value);
        if (filter.TermID.HasValue) q = q.Where(t => t.TermID == filter.TermID.Value);
        if (filter.ClassID.HasValue) q = q.Where(t => t.ClassID == filter.ClassID.Value);
        if (filter.DivisionID.HasValue) q = q.Where(t => t.DivisionID == filter.DivisionID.Value);
        if (filter.SubjectID.HasValue) q = q.Where(t => t.SubjectID == filter.SubjectID.Value);
        return MapTaskListAsync(q, cancellationToken);
    }

    public Task<IReadOnlyList<HomeworkTaskListDto>> ListTasksPrivilegedAsync(HomeworkFilterQuery filter, CancellationToken cancellationToken = default)
    {
        var q = _db.HomeworkTasks.AsNoTracking().AsQueryable();
        if (filter.YearID.HasValue) q = q.Where(t => t.YearID == filter.YearID.Value);
        if (filter.TermID.HasValue) q = q.Where(t => t.TermID == filter.TermID.Value);
        if (filter.ClassID.HasValue) q = q.Where(t => t.ClassID == filter.ClassID.Value);
        if (filter.DivisionID.HasValue) q = q.Where(t => t.DivisionID == filter.DivisionID.Value);
        if (filter.SubjectID.HasValue) q = q.Where(t => t.SubjectID == filter.SubjectID.Value);
        if (filter.TeacherID.HasValue) q = q.Where(t => t.TeacherID == filter.TeacherID.Value);
        return MapTaskListAsync(q, cancellationToken);
    }

    public async Task<IReadOnlyList<HomeworkSubmissionRowDto>> ListSubmissionsForTaskAsync(int homeworkTaskId, CancellationToken cancellationToken = default)
    {
        var rows = await _db.HomeworkSubmissions.AsNoTracking()
            .Where(s => s.HomeworkTaskID == homeworkTaskId)
            .Include(s => s.Student)
            .OrderBy(s => s.StudentID)
            .ToListAsync(cancellationToken);

        var fileIds = rows.Select(r => r.HomeworkSubmissionID).ToList();
        var files = await _db.HomeworkSubmissionFiles.AsNoTracking()
            .Where(f => fileIds.Contains(f.HomeworkSubmissionID))
            .ToListAsync(cancellationToken);
        var filesBySub = files.GroupBy(f => f.HomeworkSubmissionID).ToDictionary(g => g.Key, g => g.ToList());

        return rows.Select(r => new HomeworkSubmissionRowDto
        {
            HomeworkSubmissionID = r.HomeworkSubmissionID,
            StudentID = r.StudentID,
            StudentName = r.Student != null ? FormatName(r.Student.FullName) : null,
            Status = (byte)r.Status,
            SubmittedAtUtc = r.SubmittedAtUtc,
            AnswerText = r.AnswerText,
            Files = (filesBySub.GetValueOrDefault(r.HomeworkSubmissionID) ?? new List<HomeworkSubmissionFile>())
                .Select(f => new HomeworkSubmissionFileDto
                {
                    HomeworkSubmissionFileID = f.HomeworkSubmissionFileID,
                    FileUrl = f.FileUrl,
                    FileName = f.FileName
                }).ToList(),
            TeacherFeedback = r.TeacherFeedback,
            Score = r.Score,
            FeedbackPublished = r.FeedbackPublished
        }).ToList();
    }

    public async Task<HomeworkSubmissionRowDto?> ReviewSubmissionAsync(int homeworkSubmissionId, int teacherId, ReviewHomeworkSubmissionDto dto, bool privileged, CancellationToken cancellationToken = default)
    {
        var sub = await _db.HomeworkSubmissions
            .Include(s => s.HomeworkTask)
            .Include(s => s.Student)
            .FirstOrDefaultAsync(s => s.HomeworkSubmissionID == homeworkSubmissionId, cancellationToken);

        if (sub == null) return null;
        if (!privileged && sub.HomeworkTask.TeacherID != teacherId)
            throw new UnauthorizedAccessException("You cannot review submissions for this task.");

        var st = (HomeworkSubmissionStatus)dto.Status;
        if (!Enum.IsDefined(typeof(HomeworkSubmissionStatus), st))
            throw new ArgumentException("Invalid status value.");

        sub.Status = st;
        sub.TeacherFeedback = dto.TeacherFeedback;
        sub.Score = dto.Score;
        sub.FeedbackPublished = dto.FeedbackPublished;
        sub.ReviewedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        var files = await _db.HomeworkSubmissionFiles.AsNoTracking()
            .Where(f => f.HomeworkSubmissionID == sub.HomeworkSubmissionID)
            .ToListAsync(cancellationToken);

        return new HomeworkSubmissionRowDto
        {
            HomeworkSubmissionID = sub.HomeworkSubmissionID,
            StudentID = sub.StudentID,
            StudentName = sub.Student != null ? FormatName(sub.Student.FullName) : null,
            Status = (byte)sub.Status,
            SubmittedAtUtc = sub.SubmittedAtUtc,
            AnswerText = sub.AnswerText,
            Files = files.Select(f => new HomeworkSubmissionFileDto
            {
                HomeworkSubmissionFileID = f.HomeworkSubmissionFileID,
                FileUrl = f.FileUrl,
                FileName = f.FileName
            }).ToList(),
            TeacherFeedback = sub.TeacherFeedback,
            Score = sub.Score,
            FeedbackPublished = sub.FeedbackPublished
        };
    }

    private static bool StudentFilterMatch(string? filter, HomeworkSubmission s, HomeworkTask t, DateTime utcToday)
    {
        var f = (filter ?? "all").Trim().ToLowerInvariant();
        var due = t.DueDateUtc.Date;

        if (f == "today" || f == "due-today")
            return due == utcToday;

        if (f == "upcoming")
            return due > utcToday && s.Status == HomeworkSubmissionStatus.Pending;

        if (f == "overdue")
            return due < utcToday && s.Status == HomeworkSubmissionStatus.Pending;

        if (f == "completed" || f == "done")
            return s.Status == HomeworkSubmissionStatus.Submitted
                || s.Status == HomeworkSubmissionStatus.Late
                || s.Status == HomeworkSubmissionStatus.Graded
                || s.Status == HomeworkSubmissionStatus.Completed;

        if (f == "pending")
            return s.Status == HomeworkSubmissionStatus.Pending;

        return true;
    }

    private static StudentHomeworkListItemDto MapStudentListItem(HomeworkSubmission s, HomeworkTask t, bool guardianView)
    {
        var showFeedback = !guardianView || s.FeedbackPublished;
        var showScore = s.Status == HomeworkSubmissionStatus.Graded && (!guardianView || s.FeedbackPublished);
        return new StudentHomeworkListItemDto
        {
            HomeworkTaskID = t.HomeworkTaskID,
            HomeworkSubmissionID = s.HomeworkSubmissionID,
            Title = t.Title,
            SubjectName = t.Subject?.SubjectName,
            ClassName = t.Class?.ClassName,
            DivisionName = t.Division?.DivisionName,
            DueDateUtc = t.DueDateUtc,
            SubmissionRequired = t.SubmissionRequired,
            Status = (byte)s.Status,
            SubmittedAtUtc = s.SubmittedAtUtc,
            TeacherFeedback = showFeedback ? s.TeacherFeedback : null,
            Score = showScore ? s.Score : null,
            FeedbackPublished = s.FeedbackPublished
        };
    }

    private static GuardianStudentHomeworkRowDto MapGuardianAggregateRow(HomeworkSubmission s, HomeworkTask t, Student student)
    {
        var item = MapStudentListItem(s, t, guardianView: true);
        return new GuardianStudentHomeworkRowDto
        {
            HomeworkTaskID = item.HomeworkTaskID,
            HomeworkSubmissionID = item.HomeworkSubmissionID,
            Title = item.Title,
            SubjectName = item.SubjectName,
            ClassName = item.ClassName,
            DivisionName = item.DivisionName,
            DueDateUtc = item.DueDateUtc,
            SubmissionRequired = item.SubmissionRequired,
            Status = item.Status,
            SubmittedAtUtc = item.SubmittedAtUtc,
            TeacherFeedback = item.TeacherFeedback,
            Score = item.Score,
            FeedbackPublished = item.FeedbackPublished,
            StudentID = student.StudentID,
            StudentName = FormatName(student.FullName)
        };
    }

    public async Task<IReadOnlyList<StudentHomeworkListItemDto>> ListStudentTasksAsync(int studentId, string? filter, CancellationToken cancellationToken = default)
    {
        var utcToday = DateTime.UtcNow.Date;
        var list = await _db.HomeworkSubmissions.AsNoTracking()
            .Where(s => s.StudentID == studentId)
            .Include(s => s.HomeworkTask!)
                .ThenInclude(t => t.Subject)
            .Include(s => s.HomeworkTask!)
                .ThenInclude(t => t.Class)
            .Include(s => s.HomeworkTask!)
                .ThenInclude(t => t.Division)
            .ToListAsync(cancellationToken);

        return list
            .Where(x => x.HomeworkTask != null && StudentFilterMatch(filter, x, x.HomeworkTask, utcToday))
            .Select(x => MapStudentListItem(x, x.HomeworkTask!, guardianView: false))
            .OrderBy(x => x.DueDateUtc)
            .ToList();
    }

    public async Task<StudentHomeworkDetailDto?> GetStudentTaskDetailAsync(int studentId, int homeworkTaskId, CancellationToken cancellationToken = default)
    {
        var s = await _db.HomeworkSubmissions
            .AsNoTracking()
            .Include(x => x.HomeworkTask!)
                .ThenInclude(t => t.Subject)
            .Include(x => x.HomeworkTask!)
                .ThenInclude(t => t.Class)
            .Include(x => x.HomeworkTask!)
                .ThenInclude(t => t.Division)
            .FirstOrDefaultAsync(x => x.HomeworkTaskID == homeworkTaskId && x.StudentID == studentId, cancellationToken);

        if (s?.HomeworkTask == null) return null;

        var t = s.HomeworkTask;
        var links = await _db.HomeworkTaskLinks.AsNoTracking()
            .Where(l => l.HomeworkTaskID == homeworkTaskId)
            .OrderBy(l => l.SortOrder)
            .ToListAsync(cancellationToken);

        var files = await _db.HomeworkSubmissionFiles.AsNoTracking()
            .Where(f => f.HomeworkSubmissionID == s.HomeworkSubmissionID)
            .ToListAsync(cancellationToken);

        var baseItem = MapStudentListItem(s, t, guardianView: false);
        return new StudentHomeworkDetailDto
        {
            HomeworkTaskID = baseItem.HomeworkTaskID,
            HomeworkSubmissionID = baseItem.HomeworkSubmissionID,
            Title = baseItem.Title,
            SubjectName = baseItem.SubjectName,
            ClassName = baseItem.ClassName,
            DivisionName = baseItem.DivisionName,
            DueDateUtc = baseItem.DueDateUtc,
            SubmissionRequired = baseItem.SubmissionRequired,
            Status = baseItem.Status,
            SubmittedAtUtc = baseItem.SubmittedAtUtc,
            TeacherFeedback = baseItem.TeacherFeedback,
            Score = baseItem.Score,
            FeedbackPublished = baseItem.FeedbackPublished,
            Description = t.Description,
            TaskLinks = links.Select(MapLink).ToList(),
            AnswerText = s.AnswerText,
            Files = files.Select(f => new HomeworkSubmissionFileDto
            {
                HomeworkSubmissionFileID = f.HomeworkSubmissionFileID,
                FileUrl = f.FileUrl,
                FileName = f.FileName
            }).ToList()
        };
    }

    public async Task<StudentHomeworkDetailDto?> SubmitStudentTaskAsync(int studentId, int homeworkTaskId, StudentSubmitHomeworkDto dto, CancellationToken cancellationToken = default)
    {
        var sub = await _db.HomeworkSubmissions
            .Include(s => s.HomeworkTask)
            .FirstOrDefaultAsync(s => s.HomeworkTaskID == homeworkTaskId && s.StudentID == studentId, cancellationToken);

        if (sub?.HomeworkTask == null) return null;

        if (sub.Status is HomeworkSubmissionStatus.Graded or HomeworkSubmissionStatus.Completed or HomeworkSubmissionStatus.Missing)
            throw new InvalidOperationException("This task can no longer be submitted.");

        var hasText = !string.IsNullOrWhiteSpace(dto.AnswerText);
        var fileList = dto.Files?.Where(f => !string.IsNullOrWhiteSpace(f.FileUrl)).ToList() ?? new List<HomeworkSubmissionFileInputDto>();
        if (sub.HomeworkTask.SubmissionRequired && !hasText && fileList.Count == 0)
            throw new InvalidOperationException("This task requires a submission (text or file).");

        sub.AnswerText = dto.AnswerText;
        sub.SubmittedAtUtc = DateTime.UtcNow;

        var due = sub.HomeworkTask.DueDateUtc;
        sub.Status = DateTime.UtcNow > due ? HomeworkSubmissionStatus.Late : HomeworkSubmissionStatus.Submitted;

        var oldFiles = await _db.HomeworkSubmissionFiles.Where(f => f.HomeworkSubmissionID == sub.HomeworkSubmissionID).ToListAsync(cancellationToken);
        _db.HomeworkSubmissionFiles.RemoveRange(oldFiles);

        foreach (var f in fileList)
        {
            await _db.HomeworkSubmissionFiles.AddAsync(new HomeworkSubmissionFile
            {
                HomeworkSubmissionID = sub.HomeworkSubmissionID,
                FileUrl = f.FileUrl.Trim(),
                FileName = f.FileName
            }, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return await GetStudentTaskDetailAsync(studentId, homeworkTaskId, cancellationToken);
    }

    public async Task<IReadOnlyList<StudentHomeworkListItemDto>> ListGuardianStudentTasksAsync(int guardianId, int studentId, string? filter, CancellationToken cancellationToken = default)
    {
        var ok = await _db.Students.AsNoTracking()
            .AnyAsync(s => s.StudentID == studentId && s.GuardianID == guardianId, cancellationToken);
        if (!ok) return Array.Empty<StudentHomeworkListItemDto>();

        var utcToday = DateTime.UtcNow.Date;
        var list = await _db.HomeworkSubmissions.AsNoTracking()
            .Where(s => s.StudentID == studentId)
            .Include(s => s.HomeworkTask!)
                .ThenInclude(t => t.Subject)
            .Include(s => s.HomeworkTask!)
                .ThenInclude(t => t.Class)
            .Include(s => s.HomeworkTask!)
                .ThenInclude(t => t.Division)
            .ToListAsync(cancellationToken);

        return list
            .Where(x => x.HomeworkTask != null && StudentFilterMatch(filter, x, x.HomeworkTask, utcToday))
            .Select(x => MapStudentListItem(x, x.HomeworkTask!, guardianView: true))
            .OrderBy(x => x.DueDateUtc)
            .ToList();
    }

    public async Task<IReadOnlyList<GuardianStudentHomeworkRowDto>> ListAllGuardianStudentTasksAsync(int guardianId, string? filter, CancellationToken cancellationToken = default)
    {
        var utcToday = DateTime.UtcNow.Date;
        var studentIds = await _db.Students.AsNoTracking()
            .Where(st => st.GuardianID == guardianId)
            .Select(st => st.StudentID)
            .ToListAsync(cancellationToken);
        if (studentIds.Count == 0)
            return Array.Empty<GuardianStudentHomeworkRowDto>();

        var list = await _db.HomeworkSubmissions.AsNoTracking()
            .Where(s => studentIds.Contains(s.StudentID))
            .Include(s => s.Student)
            .Include(s => s.HomeworkTask!)
                .ThenInclude(t => t.Subject)
            .Include(s => s.HomeworkTask!)
                .ThenInclude(t => t.Class)
            .Include(s => s.HomeworkTask!)
                .ThenInclude(t => t.Division)
            .ToListAsync(cancellationToken);

        return list
            .Where(x => x.HomeworkTask != null && x.Student != null && StudentFilterMatch(filter, x, x.HomeworkTask, utcToday))
            .Select(x => MapGuardianAggregateRow(x, x.HomeworkTask!, x.Student))
            .OrderBy(x => x.DueDateUtc)
            .ToList();
    }

    public async Task<StudentHomeworkDetailDto?> GetGuardianStudentTaskDetailAsync(int guardianId, int studentId, int homeworkTaskId, CancellationToken cancellationToken = default)
    {
        var ok = await _db.Students.AsNoTracking()
            .AnyAsync(s => s.StudentID == studentId && s.GuardianID == guardianId, cancellationToken);
        if (!ok) return null;

        var detail = await GetStudentTaskDetailAsync(studentId, homeworkTaskId, cancellationToken);
        if (detail == null) return null;

        if (!detail.FeedbackPublished)
        {
            detail.TeacherFeedback = null;
            detail.Score = null;
        }

        return detail;
    }

    public async Task<HomeworkActivitySummaryDto> GetActivitySummaryAsync(int yearId, int termId, int? classId, int? teacherId, CancellationToken cancellationToken = default)
    {
        var tasksQ = _db.HomeworkTasks.AsNoTracking().Where(t => t.YearID == yearId && t.TermID == termId);
        if (classId.HasValue) tasksQ = tasksQ.Where(t => t.ClassID == classId.Value);
        if (teacherId.HasValue) tasksQ = tasksQ.Where(t => t.TeacherID == teacherId.Value);

        var taskCount = await tasksQ.CountAsync(cancellationToken);

        var today = DateTime.UtcNow.Date;
        var subsWithDue = await (
            from s in _db.HomeworkSubmissions.AsNoTracking()
            join ht in _db.HomeworkTasks.AsNoTracking() on s.HomeworkTaskID equals ht.HomeworkTaskID
            where ht.YearID == yearId && ht.TermID == termId
            where !classId.HasValue || ht.ClassID == classId.Value
            where !teacherId.HasValue || ht.TeacherID == teacherId.Value
            select new { s, Due = ht.DueDateUtc.Date }
        ).ToListAsync(cancellationToken);

        var missing = subsWithDue.Count(x =>
            x.s.Status == HomeworkSubmissionStatus.Missing ||
            (x.s.Status == HomeworkSubmissionStatus.Pending && today > x.Due));

        var graded = subsWithDue.Count(x => x.s.Status == HomeworkSubmissionStatus.Graded);

        var teacherGroups = await tasksQ
            .GroupBy(t => t.TeacherID)
            .Select(g => new { TeacherID = g.Key, Cnt = g.Count() })
            .ToListAsync(cancellationToken);

        var teacherIds = teacherGroups.Select(x => x.TeacherID).ToList();
        var teachers = await _db.Teachers.AsNoTracking()
            .Where(t => teacherIds.Contains(t.TeacherID))
            .ToListAsync(cancellationToken);
        var nameById = teachers.ToDictionary(t => t.TeacherID, t => FormatName(t.FullName));

        return new HomeworkActivitySummaryDto
        {
            TaskCount = taskCount,
            MissingSubmissionCount = missing,
            GradedCount = graded,
            Teachers = teacherGroups.Select(x => new HomeworkTeacherActivityDto
            {
                TeacherID = x.TeacherID,
                TeacherName = nameById.GetValueOrDefault(x.TeacherID),
                TasksCreated = x.Cnt
            }).OrderByDescending(x => x.TasksCreated).ToList()
        };
    }
}
