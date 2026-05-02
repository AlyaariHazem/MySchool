using Backend.Data;
using Backend.DTOS.School.Activity;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School;

public class ActivityRepository : IActivityRepository
{
    private readonly TenantDbContext _db;

    public ActivityRepository(TenantDbContext db)
    {
        _db = db;
    }

    private static string FormatPersonName(Name? n)
    {
        if (n == null) return string.Empty;
        return string.Join(" ", new[] { n.FirstName, n.MiddleName, n.LastName }
            .Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
    }

    private async Task<int?> GetActiveYearIdForSchoolAsync(int schoolId, CancellationToken cancellationToken = default)
    {
        var yid = await _db.Years.AsNoTracking()
            .Where(y => y.SchoolID == schoolId && y.Active)
            .OrderBy(y => y.YearID)
            .Select(y => (int?)y.YearID)
            .FirstOrDefaultAsync(cancellationToken);
        if (yid is > 0)
            return yid;
        return await _db.Years.AsNoTracking()
            .Where(y => y.SchoolID == schoolId)
            .OrderByDescending(y => y.YearID)
            .Select(y => (int?)y.YearID)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<int?> GetEmployeeProfileIdForUserInSchoolAsync(string? userId, int schoolId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            return Task.FromResult<int?>(null);
        return _db.EmployeeProfiles.AsNoTracking()
            .Where(e => e.UserId == userId && e.SchoolID == schoolId && e.IsActive)
            .OrderBy(e => e.EmployeeProfileID)
            .Select(e => (int?)e.EmployeeProfileID)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task ValidateEmployeesInSchoolAsync(int schoolId, IEnumerable<int> employeeProfileIds, CancellationToken cancellationToken)
    {
        var ids = employeeProfileIds.Where(id => id > 0).Distinct().ToList();
        if (ids.Count == 0) return;
        var count = await _db.EmployeeProfiles.AsNoTracking()
            .CountAsync(e => ids.Contains(e.EmployeeProfileID) && e.SchoolID == schoolId, cancellationToken);
        if (count != ids.Count)
            throw new InvalidOperationException("One or more employee profiles were not found for this school.");
    }

    private static IEnumerable<int> CollectEmployeeIdsFromWrite(ActivityRequestWriteDto dto)
    {
        yield return dto.EmployeeProfileID;
        foreach (var a in dto.Approvals) yield return a.ApproverEmployeeProfileID;
        foreach (var e in dto.Executions)
        {
            if (e.ResponsibleEmployeeProfileID is > 0)
                yield return e.ResponsibleEmployeeProfileID.Value;
        }
        foreach (var ev in dto.Evaluations) yield return ev.EvaluatorEmployeeProfileID;
        foreach (var p in dto.Points) yield return p.AwardedByEmployeeProfileID;
    }

    public async Task<IReadOnlyList<ActivityListItemDto>> ListAsync(ActivityFilterDto filter, CancellationToken cancellationToken = default)
    {
        filter ??= new ActivityFilterDto();
        var q = _db.ActivityRequests.AsNoTracking().AsQueryable();

        if (filter.SchoolID is > 0)
        {
            q = q.Where(r => r.SchoolID == filter.SchoolID);
            if (filter.AcademicYearID is not > 0)
            {
                var activeYear = await GetActiveYearIdForSchoolAsync(filter.SchoolID.Value, cancellationToken);
                if (activeYear is > 0)
                    q = q.Where(r => r.AcademicYearID == activeYear.Value);
            }
        }

        if (filter.AcademicYearID is > 0)
            q = q.Where(r => r.AcademicYearID == filter.AcademicYearID);
        if (filter.EmployeeProfileID is > 0)
            q = q.Where(r => r.EmployeeProfileID == filter.EmployeeProfileID);
        if (filter.Status is >= 0)
            q = q.Where(r => (int)r.Status == filter.Status);

        var raw = await q
            .OrderByDescending(r => r.SubmittedAtUtc)
            .ThenByDescending(r => r.ActivityRequestID)
            .Select(r => new
            {
                r.ActivityRequestID,
                r.SchoolID,
                r.AcademicYearID,
                r.EmployeeProfileID,
                EFirst = r.EmployeeProfile.FullName.FirstName,
                EMid = r.EmployeeProfile.FullName.MiddleName,
                ELast = r.EmployeeProfile.FullName.LastName,
                r.Title,
                St = (int)r.Status,
                r.SubmittedAtUtc,
                r.UpdatedAtUtc,
                r.ResolvedAtUtc,
            })
            .ToListAsync(cancellationToken);

        return raw.Select(r => new ActivityListItemDto
        {
            ActivityRequestID = r.ActivityRequestID,
            SchoolID = r.SchoolID,
            AcademicYearID = r.AcademicYearID,
            EmployeeProfileID = r.EmployeeProfileID,
            EmployeeName = FormatPersonName(new Name { FirstName = r.EFirst, MiddleName = r.EMid, LastName = r.ELast }),
            Title = r.Title,
            Status = r.St,
            SubmittedAtUtc = r.SubmittedAtUtc,
            UpdatedAtUtc = r.UpdatedAtUtc,
            ResolvedAtUtc = r.ResolvedAtUtc,
        }).ToList();
    }

    public async Task<ActivityDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var r = await _db.ActivityRequests.AsNoTracking()
            .Include(x => x.EmployeeProfile)
            .Include(x => x.Approvals).ThenInclude(a => a.ApproverEmployeeProfile)
            .Include(x => x.Executions).ThenInclude(e => e.ResponsibleEmployeeProfile)
            .Include(x => x.Evaluations).ThenInclude(ev => ev.EvaluatorEmployeeProfile)
            .Include(x => x.Points).ThenInclude(p => p.AwardedByEmployeeProfile)
            .FirstOrDefaultAsync(x => x.ActivityRequestID == id, cancellationToken);

        if (r == null) return null;

        var baseRow = new ActivityListItemDto
        {
            ActivityRequestID = r.ActivityRequestID,
            SchoolID = r.SchoolID,
            AcademicYearID = r.AcademicYearID,
            EmployeeProfileID = r.EmployeeProfileID,
            EmployeeName = FormatPersonName(r.EmployeeProfile.FullName),
            Title = r.Title,
            Status = (int)r.Status,
            SubmittedAtUtc = r.SubmittedAtUtc,
            UpdatedAtUtc = r.UpdatedAtUtc,
            ResolvedAtUtc = r.ResolvedAtUtc,
        };

        return new ActivityDetailDto
        {
            ActivityRequestID = baseRow.ActivityRequestID,
            SchoolID = baseRow.SchoolID,
            AcademicYearID = baseRow.AcademicYearID,
            EmployeeProfileID = baseRow.EmployeeProfileID,
            EmployeeName = baseRow.EmployeeName,
            Title = baseRow.Title,
            Status = baseRow.Status,
            SubmittedAtUtc = baseRow.SubmittedAtUtc,
            UpdatedAtUtc = baseRow.UpdatedAtUtc,
            ResolvedAtUtc = baseRow.ResolvedAtUtc,
            Details = r.Details,
            Approvals = r.Approvals.OrderBy(a => a.SortOrder).ThenBy(a => a.ActivityApprovalID).Select(a => new ActivityApprovalReadDto
            {
                ActivityApprovalID = a.ActivityApprovalID,
                ActivityRequestID = a.ActivityRequestID,
                ApproverEmployeeProfileID = a.ApproverEmployeeProfileID,
                ApproverName = FormatPersonName(a.ApproverEmployeeProfile.FullName),
                SortOrder = a.SortOrder,
                Decision = (int)a.Decision,
                Comment = a.Comment,
                DecidedAtUtc = a.DecidedAtUtc,
                CreatedAtUtc = a.CreatedAtUtc,
            }).ToList(),
            Executions = r.Executions.OrderByDescending(e => e.UpdatedAtUtc).ThenByDescending(e => e.ActivityExecutionID).Select(e => new ActivityExecutionReadDto
            {
                ActivityExecutionID = e.ActivityExecutionID,
                ActivityRequestID = e.ActivityRequestID,
                Status = (int)e.Status,
                Notes = e.Notes,
                ProgressPercent = e.ProgressPercent,
                DueAtUtc = e.DueAtUtc,
                ExecutedAtUtc = e.ExecutedAtUtc,
                UpdatedAtUtc = e.UpdatedAtUtc,
                ResponsibleEmployeeProfileID = e.ResponsibleEmployeeProfileID,
                ResponsibleName = e.ResponsibleEmployeeProfile != null ? FormatPersonName(e.ResponsibleEmployeeProfile.FullName) : null,
            }).ToList(),
            Evaluations = r.Evaluations.OrderByDescending(ev => ev.CreatedAtUtc).Select(ev => new ActivityEvaluationReadDto
            {
                ActivityEvaluationID = ev.ActivityEvaluationID,
                ActivityRequestID = ev.ActivityRequestID,
                EvaluatorEmployeeProfileID = ev.EvaluatorEmployeeProfileID,
                EvaluatorName = FormatPersonName(ev.EvaluatorEmployeeProfile.FullName),
                Score = ev.Score,
                Feedback = ev.Feedback,
                CreatedAtUtc = ev.CreatedAtUtc,
            }).ToList(),
            Points = r.Points.OrderByDescending(p => p.AwardedAtUtc).Select(p => new ActivityPointsReadDto
            {
                ActivityPointsID = p.ActivityPointsID,
                ActivityRequestID = p.ActivityRequestID,
                Points = p.Points,
                Reason = p.Reason,
                AwardedByEmployeeProfileID = p.AwardedByEmployeeProfileID,
                AwardedByName = FormatPersonName(p.AwardedByEmployeeProfile.FullName),
                AwardedAtUtc = p.AwardedAtUtc,
            }).ToList(),
        };
    }

    private void ValidateEvaluations(ActivityRequestWriteDto dto)
    {
        foreach (var ev in dto.Evaluations)
        {
            if (ev.Score is < 1 or > 5)
                throw new InvalidOperationException("Evaluation score must be between 1 and 5.");
        }
    }

    public async Task<int> CreateAsync(ActivityRequestWriteDto dto, CancellationToken cancellationToken = default)
    {
        await ValidateEmployeesInSchoolAsync(dto.SchoolID, CollectEmployeeIdsFromWrite(dto), cancellationToken);
        ValidateEvaluations(dto);

        var yearId = dto.AcademicYearID is > 0
            ? dto.AcademicYearID!.Value
            : await GetActiveYearIdForSchoolAsync(dto.SchoolID, cancellationToken)
              ?? throw new InvalidOperationException("No academic year is configured for this school.");

        var yearOk = await _db.Years.AsNoTracking()
            .AnyAsync(y => y.YearID == yearId && y.SchoolID == dto.SchoolID, cancellationToken);
        if (!yearOk)
            throw new InvalidOperationException("Academic year does not belong to this school.");

        var now = DateTime.UtcNow;
        var status = (ActivityRequestStatus)dto.Status;
        var entity = new ActivityRequest
        {
            SchoolID = dto.SchoolID,
            AcademicYearID = yearId,
            EmployeeProfileID = dto.EmployeeProfileID,
            Title = dto.Title,
            Details = dto.Details,
            Status = status,
            SubmittedAtUtc = now,
            UpdatedAtUtc = now,
            ResolvedAtUtc = status == ActivityRequestStatus.Completed ? now : null,
        };

        _db.ActivityRequests.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        await ReplaceChildrenAsync(entity.ActivityRequestID, dto, now, cancellationToken);
        return entity.ActivityRequestID;
    }

    public async Task UpdateAsync(int id, ActivityRequestWriteDto dto, CancellationToken cancellationToken = default)
    {
        var r = await _db.ActivityRequests
            .FirstOrDefaultAsync(x => x.ActivityRequestID == id, cancellationToken)
            ?? throw new InvalidOperationException("Activity request was not found.");

        if (r.SchoolID != dto.SchoolID)
            throw new InvalidOperationException("School mismatch for this activity request.");

        await ValidateEmployeesInSchoolAsync(dto.SchoolID, CollectEmployeeIdsFromWrite(dto), cancellationToken);
        ValidateEvaluations(dto);

        var yearId = dto.AcademicYearID is > 0 ? dto.AcademicYearID!.Value : r.AcademicYearID;
        var yearOk = await _db.Years.AsNoTracking()
            .AnyAsync(y => y.YearID == yearId && y.SchoolID == dto.SchoolID, cancellationToken);
        if (!yearOk)
            throw new InvalidOperationException("Academic year does not belong to this school.");

        var now = DateTime.UtcNow;
        var status = (ActivityRequestStatus)dto.Status;

        r.AcademicYearID = yearId;
        r.EmployeeProfileID = dto.EmployeeProfileID;
        r.Title = dto.Title;
        r.Details = dto.Details;
        r.Status = status;
        r.UpdatedAtUtc = now;
        r.ResolvedAtUtc = status == ActivityRequestStatus.Completed ? r.ResolvedAtUtc ?? now : null;

        await _db.ActivityApprovals.Where(x => x.ActivityRequestID == id).ExecuteDeleteAsync(cancellationToken);
        await _db.ActivityExecutions.Where(x => x.ActivityRequestID == id).ExecuteDeleteAsync(cancellationToken);
        await _db.ActivityEvaluations.Where(x => x.ActivityRequestID == id).ExecuteDeleteAsync(cancellationToken);
        await _db.ActivityPoints.Where(x => x.ActivityRequestID == id).ExecuteDeleteAsync(cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);

        await ReplaceChildrenAsync(id, dto, now, cancellationToken);
    }

    private async Task ReplaceChildrenAsync(int activityRequestId, ActivityRequestWriteDto dto, DateTime now, CancellationToken cancellationToken)
    {
        foreach (var a in dto.Approvals.OrderBy(x => x.SortOrder))
        {
            _db.ActivityApprovals.Add(new ActivityApproval
            {
                ActivityRequestID = activityRequestId,
                ApproverEmployeeProfileID = a.ApproverEmployeeProfileID,
                SortOrder = a.SortOrder,
                Decision = (ActivityApprovalDecision)a.Decision,
                Comment = a.Comment,
                DecidedAtUtc = a.DecidedAtUtc,
                CreatedAtUtc = now,
            });
        }

        foreach (var e in dto.Executions)
        {
            _db.ActivityExecutions.Add(new ActivityExecution
            {
                ActivityRequestID = activityRequestId,
                Status = (ActivityExecutionStatus)e.Status,
                Notes = e.Notes,
                ProgressPercent = e.ProgressPercent,
                DueAtUtc = e.DueAtUtc,
                ExecutedAtUtc = e.ExecutedAtUtc,
                UpdatedAtUtc = now,
                ResponsibleEmployeeProfileID = e.ResponsibleEmployeeProfileID is > 0 ? e.ResponsibleEmployeeProfileID : null,
            });
        }

        foreach (var ev in dto.Evaluations)
        {
            _db.ActivityEvaluations.Add(new ActivityEvaluation
            {
                ActivityRequestID = activityRequestId,
                EvaluatorEmployeeProfileID = ev.EvaluatorEmployeeProfileID,
                Score = ev.Score,
                Feedback = ev.Feedback,
                CreatedAtUtc = now,
            });
        }

        foreach (var p in dto.Points)
        {
            _db.ActivityPoints.Add(new ActivityPoints
            {
                ActivityRequestID = activityRequestId,
                Points = p.Points,
                Reason = p.Reason,
                AwardedByEmployeeProfileID = p.AwardedByEmployeeProfileID,
                AwardedAtUtc = now,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<int?> GetSchoolIdForRequestAsync(int activityRequestId, CancellationToken cancellationToken = default)
    {
        return _db.ActivityRequests.AsNoTracking()
            .Where(x => x.ActivityRequestID == activityRequestId)
            .Select(x => (int?)x.SchoolID)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
