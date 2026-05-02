using Backend.Data;
using Backend.DTOS.School.EmployeeRequest;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School;

public class EmployeeRequestRepository : IEmployeeRequestRepository
{
    private readonly TenantDbContext _db;

    public EmployeeRequestRepository(TenantDbContext db)
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

    private async Task TouchRequestAsync(int employeeRequestId, CancellationToken cancellationToken)
    {
        await _db.EmployeeRequests.Where(x => x.EmployeeRequestID == employeeRequestId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.UpdatedAtUtc, DateTime.UtcNow), cancellationToken);
    }

    public async Task<IReadOnlyList<EmployeeRequestTypeListItemDto>> ListTypesAsync(int schoolId, CancellationToken cancellationToken = default)
    {
        return await _db.RequestTypes.AsNoTracking()
            .Where(t => t.SchoolID == schoolId)
            .OrderBy(t => t.Code)
            .ThenBy(t => t.RequestTypeID)
            .Select(t => new EmployeeRequestTypeListItemDto
            {
                RequestTypeID = t.RequestTypeID,
                SchoolID = t.SchoolID,
                Code = t.Code,
                Category = (int)t.Category,
                Name = t.Name,
                NameAr = t.NameAr,
                Description = t.Description,
                IsActive = t.IsActive,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EmployeeRequestListItemDto>> ListAsync(EmployeeRequestFilterDto filter, CancellationToken cancellationToken = default)
    {
        filter ??= new EmployeeRequestFilterDto();
        var q = _db.EmployeeRequests.AsNoTracking().AsQueryable();

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
            .ThenByDescending(r => r.EmployeeRequestID)
            .Select(r => new
            {
                r.EmployeeRequestID,
                r.SchoolID,
                r.AcademicYearID,
                r.EmployeeProfileID,
                EFirst = r.EmployeeProfile.FullName.FirstName,
                EMid = r.EmployeeProfile.FullName.MiddleName,
                ELast = r.EmployeeProfile.FullName.LastName,
                r.RequestTypeID,
                r.RequestType.Code,
                Cat = (int)r.RequestType.Category,
                r.RequestType.Name,
                r.RequestType.NameAr,
                r.Title,
                St = (int)r.Status,
                r.RequestedAmount,
                r.SubmittedAtUtc,
                r.UpdatedAtUtc,
                r.ResolvedAtUtc,
            })
            .ToListAsync(cancellationToken);

        return raw.Select(r => new EmployeeRequestListItemDto
        {
            EmployeeRequestID = r.EmployeeRequestID,
            SchoolID = r.SchoolID,
            AcademicYearID = r.AcademicYearID,
            EmployeeProfileID = r.EmployeeProfileID,
            EmployeeName = FormatPersonName(new Name { FirstName = r.EFirst, MiddleName = r.EMid, LastName = r.ELast }),
            RequestTypeID = r.RequestTypeID,
            RequestTypeCode = r.Code,
            RequestTypeCategory = r.Cat,
            RequestTypeName = r.Name,
            RequestTypeNameAr = r.NameAr,
            Title = r.Title,
            Status = r.St,
            RequestedAmount = r.RequestedAmount,
            SubmittedAtUtc = r.SubmittedAtUtc,
            UpdatedAtUtc = r.UpdatedAtUtc,
            ResolvedAtUtc = r.ResolvedAtUtc,
        }).ToList();
    }

    public async Task<EmployeeRequestDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var r = await _db.EmployeeRequests.AsNoTracking()
            .Include(x => x.EmployeeProfile)
            .Include(x => x.RequestType)
            .Include(x => x.ApprovalSteps).ThenInclude(s => s.ApproverEmployeeProfile)
            .Include(x => x.Executions).ThenInclude(e => e.ResponsibleEmployeeProfile)
            .Include(x => x.DailySummaries).ThenInclude(d => d.CreatedByEmployeeProfile)
            .FirstOrDefaultAsync(x => x.EmployeeRequestID == id, cancellationToken);

        if (r == null) return null;

        var baseRow = new EmployeeRequestListItemDto
        {
            EmployeeRequestID = r.EmployeeRequestID,
            SchoolID = r.SchoolID,
            AcademicYearID = r.AcademicYearID,
            EmployeeProfileID = r.EmployeeProfileID,
            EmployeeName = FormatPersonName(r.EmployeeProfile.FullName),
            RequestTypeID = r.RequestTypeID,
            RequestTypeCode = r.RequestType.Code,
            RequestTypeCategory = (int)r.RequestType.Category,
            RequestTypeName = r.RequestType.Name,
            RequestTypeNameAr = r.RequestType.NameAr,
            Title = r.Title,
            Status = (int)r.Status,
            RequestedAmount = r.RequestedAmount,
            SubmittedAtUtc = r.SubmittedAtUtc,
            UpdatedAtUtc = r.UpdatedAtUtc,
            ResolvedAtUtc = r.ResolvedAtUtc,
        };

        return new EmployeeRequestDetailDto
        {
            EmployeeRequestID = baseRow.EmployeeRequestID,
            SchoolID = baseRow.SchoolID,
            AcademicYearID = baseRow.AcademicYearID,
            EmployeeProfileID = baseRow.EmployeeProfileID,
            EmployeeName = baseRow.EmployeeName,
            RequestTypeID = baseRow.RequestTypeID,
            RequestTypeCode = baseRow.RequestTypeCode,
            RequestTypeCategory = baseRow.RequestTypeCategory,
            RequestTypeName = baseRow.RequestTypeName,
            RequestTypeNameAr = baseRow.RequestTypeNameAr,
            Title = baseRow.Title,
            Status = baseRow.Status,
            RequestedAmount = baseRow.RequestedAmount,
            SubmittedAtUtc = baseRow.SubmittedAtUtc,
            UpdatedAtUtc = baseRow.UpdatedAtUtc,
            ResolvedAtUtc = baseRow.ResolvedAtUtc,
            Details = r.Details,
            ApprovalSteps = r.ApprovalSteps
                .OrderBy(s => s.StepOrder)
                .ThenBy(s => s.RequestApprovalStepID)
                .Select(s => new EmployeeRequestApprovalReadDto
                {
                    RequestApprovalStepID = s.RequestApprovalStepID,
                    EmployeeRequestID = s.EmployeeRequestID,
                    ApproverEmployeeProfileID = s.ApproverEmployeeProfileID,
                    ApproverName = FormatPersonName(s.ApproverEmployeeProfile.FullName),
                    StepOrder = s.StepOrder,
                    Decision = (int)s.Decision,
                    Comment = s.Comment,
                    DecidedAtUtc = s.DecidedAtUtc,
                    CreatedAtUtc = s.CreatedAtUtc,
                })
                .ToList(),
            Executions = r.Executions
                .OrderByDescending(e => e.UpdatedAtUtc)
                .ThenByDescending(e => e.RequestExecutionID)
                .Select(e => new EmployeeRequestExecutionReadDto
                {
                    RequestExecutionID = e.RequestExecutionID,
                    EmployeeRequestID = e.EmployeeRequestID,
                    Status = (int)e.Status,
                    Notes = e.Notes,
                    ProgressPercent = e.ProgressPercent,
                    DueAtUtc = e.DueAtUtc,
                    ExecutedAtUtc = e.ExecutedAtUtc,
                    UpdatedAtUtc = e.UpdatedAtUtc,
                    ResponsibleEmployeeProfileID = e.ResponsibleEmployeeProfileID,
                    ResponsibleName = e.ResponsibleEmployeeProfile != null
                        ? FormatPersonName(e.ResponsibleEmployeeProfile.FullName)
                        : null,
                })
                .ToList(),
            DailySummaries = r.DailySummaries
                .OrderByDescending(d => d.SummaryDate)
                .ThenByDescending(d => d.CreatedAtUtc)
                .Select(d => new EmployeeRequestDailySummaryReadDto
                {
                    RequestDailySummaryID = d.RequestDailySummaryID,
                    EmployeeRequestID = d.EmployeeRequestID,
                    SummaryDate = d.SummaryDate,
                    Summary = d.Summary,
                    ProgressPercent = d.ProgressPercent,
                    IsFinalForDay = d.IsFinalForDay,
                    CreatedByEmployeeProfileID = d.CreatedByEmployeeProfileID,
                    CreatedByName = d.CreatedByEmployeeProfile != null
                        ? FormatPersonName(d.CreatedByEmployeeProfile.FullName)
                        : null,
                    CreatedAtUtc = d.CreatedAtUtc,
                })
                .ToList(),
        };
    }

    public async Task<int> CreateAsync(EmployeeRequestWriteDto dto, CancellationToken cancellationToken = default)
    {
        var empOk = await _db.EmployeeProfiles.AsNoTracking()
            .AnyAsync(e => e.EmployeeProfileID == dto.EmployeeProfileID && e.SchoolID == dto.SchoolID, cancellationToken);
        if (!empOk)
            throw new InvalidOperationException("Employee profile was not found for this school.");

        var typeOk = await _db.RequestTypes.AsNoTracking()
            .AnyAsync(t => t.RequestTypeID == dto.RequestTypeID && t.SchoolID == dto.SchoolID && t.IsActive, cancellationToken);
        if (!typeOk)
            throw new InvalidOperationException("Request type was not found for this school.");

        var yearId = dto.AcademicYearID is > 0
            ? dto.AcademicYearID.Value
            : await GetActiveYearIdForSchoolAsync(dto.SchoolID, cancellationToken)
              ?? throw new InvalidOperationException("No academic year is configured for this school.");

        var yearOk = await _db.Years.AsNoTracking()
            .AnyAsync(y => y.YearID == yearId && y.SchoolID == dto.SchoolID, cancellationToken);
        if (!yearOk)
            throw new InvalidOperationException("Academic year does not belong to this school.");

        var now = DateTime.UtcNow;
        var status = (EmployeeRequestStatus)dto.Status;
        var entity = new EmployeeRequest
        {
            SchoolID = dto.SchoolID,
            AcademicYearID = yearId,
            EmployeeProfileID = dto.EmployeeProfileID,
            RequestTypeID = dto.RequestTypeID,
            Title = dto.Title,
            Details = dto.Details,
            RequestedAmount = dto.RequestedAmount,
            Status = status,
            SubmittedAtUtc = now,
            UpdatedAtUtc = now,
            ResolvedAtUtc = status is EmployeeRequestStatus.Completed or EmployeeRequestStatus.Rejected or EmployeeRequestStatus.Cancelled
                ? now
                : null,
        };

        _db.EmployeeRequests.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.EmployeeRequestID;
    }

    public async Task UpdateAsync(int id, EmployeeRequestWriteDto dto, CancellationToken cancellationToken = default)
    {
        var r = await _db.EmployeeRequests.FirstOrDefaultAsync(x => x.EmployeeRequestID == id, cancellationToken)
            ?? throw new InvalidOperationException("Employee request was not found.");

        if (r.SchoolID != dto.SchoolID)
            throw new InvalidOperationException("School mismatch for this request.");

        var empOk = await _db.EmployeeProfiles.AsNoTracking()
            .AnyAsync(e => e.EmployeeProfileID == dto.EmployeeProfileID && e.SchoolID == dto.SchoolID, cancellationToken);
        if (!empOk)
            throw new InvalidOperationException("Employee profile was not found for this school.");

        var typeOk = await _db.RequestTypes.AsNoTracking()
            .AnyAsync(t => t.RequestTypeID == dto.RequestTypeID && t.SchoolID == dto.SchoolID && t.IsActive, cancellationToken);
        if (!typeOk)
            throw new InvalidOperationException("Request type was not found for this school.");

        var yearId = dto.AcademicYearID is > 0
            ? dto.AcademicYearID.Value
            : r.AcademicYearID;

        var yearOk = await _db.Years.AsNoTracking()
            .AnyAsync(y => y.YearID == yearId && y.SchoolID == dto.SchoolID, cancellationToken);
        if (!yearOk)
            throw new InvalidOperationException("Academic year does not belong to this school.");

        var now = DateTime.UtcNow;
        var newStatus = (EmployeeRequestStatus)dto.Status;

        r.AcademicYearID = yearId;
        r.EmployeeProfileID = dto.EmployeeProfileID;
        r.RequestTypeID = dto.RequestTypeID;
        r.Title = dto.Title;
        r.Details = dto.Details;
        r.RequestedAmount = dto.RequestedAmount;
        r.Status = newStatus;
        r.UpdatedAtUtc = now;

        if (newStatus is EmployeeRequestStatus.Completed or EmployeeRequestStatus.Rejected or EmployeeRequestStatus.Cancelled)
            r.ResolvedAtUtc ??= now;
        else
            r.ResolvedAtUtc = null;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> AddExecutionAsync(int employeeRequestId, EmployeeRequestExecutionWriteDto dto, CancellationToken cancellationToken = default)
    {
        var r = await _db.EmployeeRequests.AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeRequestID == employeeRequestId, cancellationToken)
            ?? throw new InvalidOperationException("Employee request was not found.");

        if (dto.ResponsibleEmployeeProfileID is > 0)
        {
            var ok = await _db.EmployeeProfiles.AsNoTracking()
                .AnyAsync(e => e.EmployeeProfileID == dto.ResponsibleEmployeeProfileID && e.SchoolID == r.SchoolID, cancellationToken);
            if (!ok)
                throw new InvalidOperationException("Responsible employee profile was not found for this school.");
        }

        var now = DateTime.UtcNow;
        var row = new RequestExecution
        {
            EmployeeRequestID = employeeRequestId,
            Status = (RequestExecutionStatus)dto.Status,
            Notes = dto.Notes,
            ProgressPercent = dto.ProgressPercent,
            DueAtUtc = dto.DueAtUtc,
            ExecutedAtUtc = (RequestExecutionStatus)dto.Status == RequestExecutionStatus.Completed ? now : null,
            UpdatedAtUtc = now,
            ResponsibleEmployeeProfileID = dto.ResponsibleEmployeeProfileID is > 0 ? dto.ResponsibleEmployeeProfileID : null,
        };
        _db.RequestExecutions.Add(row);

        if (r.Status == EmployeeRequestStatus.Approved)
        {
            await _db.EmployeeRequests.Where(x => x.EmployeeRequestID == employeeRequestId)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, EmployeeRequestStatus.InExecution), cancellationToken);
        }

        await TouchRequestAsync(employeeRequestId, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return row.RequestExecutionID;
    }

    public async Task<int> AddDailySummaryAsync(int employeeRequestId, EmployeeRequestDailySummaryWriteDto dto, CancellationToken cancellationToken = default)
    {
        var r = await _db.EmployeeRequests.AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeRequestID == employeeRequestId, cancellationToken)
            ?? throw new InvalidOperationException("Employee request was not found.");

        if (dto.CreatedByEmployeeProfileID is > 0)
        {
            var ok = await _db.EmployeeProfiles.AsNoTracking()
                .AnyAsync(e => e.EmployeeProfileID == dto.CreatedByEmployeeProfileID && e.SchoolID == r.SchoolID, cancellationToken);
            if (!ok)
                throw new InvalidOperationException("Author employee profile was not found for this school.");
        }

        var row = new RequestDailySummary
        {
            EmployeeRequestID = employeeRequestId,
            SummaryDate = dto.SummaryDate.Date,
            Summary = dto.Summary,
            ProgressPercent = dto.ProgressPercent,
            IsFinalForDay = dto.IsFinalForDay,
            CreatedByEmployeeProfileID = dto.CreatedByEmployeeProfileID is > 0 ? dto.CreatedByEmployeeProfileID : null,
            CreatedAtUtc = DateTime.UtcNow,
        };
        _db.RequestDailySummaries.Add(row);
        await TouchRequestAsync(employeeRequestId, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return row.RequestDailySummaryID;
    }

    public async Task<int> AddApprovalStepAsync(int employeeRequestId, EmployeeRequestApprovalStepWriteDto dto, CancellationToken cancellationToken = default)
    {
        var r = await _db.EmployeeRequests.AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeRequestID == employeeRequestId, cancellationToken)
            ?? throw new InvalidOperationException("Employee request was not found.");

        var approverOk = await _db.EmployeeProfiles.AsNoTracking()
            .AnyAsync(e => e.EmployeeProfileID == dto.ApproverEmployeeProfileID && e.SchoolID == r.SchoolID, cancellationToken);
        if (!approverOk)
            throw new InvalidOperationException("Approver employee profile was not found for this school.");

        var order = dto.StepOrder;
        if (order <= 0)
        {
            var max = await _db.RequestApprovalSteps.AsNoTracking()
                .Where(s => s.EmployeeRequestID == employeeRequestId)
                .Select(s => (int?)s.StepOrder)
                .MaxAsync(cancellationToken) ?? 0;
            order = max + 1;
        }

        var dup = await _db.RequestApprovalSteps.AnyAsync(
            s => s.EmployeeRequestID == employeeRequestId && s.StepOrder == order,
            cancellationToken);
        if (dup)
            throw new InvalidOperationException("An approval step with this order already exists.");

        var row = new RequestApprovalStep
        {
            EmployeeRequestID = employeeRequestId,
            ApproverEmployeeProfileID = dto.ApproverEmployeeProfileID,
            StepOrder = order,
            Decision = RequestApprovalDecision.Pending,
            CreatedAtUtc = DateTime.UtcNow,
        };
        _db.RequestApprovalSteps.Add(row);
        await TouchRequestAsync(employeeRequestId, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return row.RequestApprovalStepID;
    }

    public async Task DecideApprovalStepAsync(int employeeRequestId, int stepId, EmployeeRequestApprovalDecideDto dto, CancellationToken cancellationToken = default)
    {
        var step = await _db.RequestApprovalSteps
            .FirstOrDefaultAsync(s => s.RequestApprovalStepID == stepId && s.EmployeeRequestID == employeeRequestId, cancellationToken)
            ?? throw new InvalidOperationException("Approval step was not found.");

        if (step.Decision != RequestApprovalDecision.Pending)
            throw new InvalidOperationException("This approval step was already decided.");

        var decision = (RequestApprovalDecision)dto.Decision;
        if (decision != RequestApprovalDecision.Approved && decision != RequestApprovalDecision.Rejected)
            throw new InvalidOperationException("Decision must be Approved or Rejected.");

        var now = DateTime.UtcNow;
        step.Decision = decision;
        step.Comment = dto.Comment;
        step.DecidedAtUtc = now;

        var req = await _db.EmployeeRequests.FirstAsync(r => r.EmployeeRequestID == employeeRequestId, cancellationToken);

        if (decision == RequestApprovalDecision.Rejected)
        {
            req.Status = EmployeeRequestStatus.Rejected;
            req.ResolvedAtUtc = now;
        }
        else
        {
            var anyPending = await _db.RequestApprovalSteps.AnyAsync(
                s => s.EmployeeRequestID == employeeRequestId
                     && s.RequestApprovalStepID != stepId
                     && s.Decision == RequestApprovalDecision.Pending,
                cancellationToken);
            if (!anyPending)
            {
                req.Status = EmployeeRequestStatus.Approved;
                req.ResolvedAtUtc = null;
            }
            else if (req.Status == EmployeeRequestStatus.Draft || req.Status == EmployeeRequestStatus.Submitted)
            {
                req.Status = EmployeeRequestStatus.InApproval;
            }
        }

        req.UpdatedAtUtc = now;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<int?> GetSchoolIdForRequestAsync(int employeeRequestId, CancellationToken cancellationToken = default)
    {
        return await _db.EmployeeRequests.AsNoTracking()
            .Where(x => x.EmployeeRequestID == employeeRequestId)
            .Select(x => (int?)x.SchoolID)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
