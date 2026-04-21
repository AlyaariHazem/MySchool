using Backend.Data;
using Backend.DTOS.School.Violation;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School;

public class ViolationRepository : IViolationRepository
{
    private readonly TenantDbContext _db;

    public ViolationRepository(TenantDbContext db)
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

    private async Task<int> GetViolationTypeIdForSchoolKindAsync(int schoolId, ViolationKind kind, CancellationToken cancellationToken)
    {
        var id = await _db.ViolationTypes.AsNoTracking()
            .Where(t => t.SchoolID == schoolId && t.Kind == kind && t.IsActive)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.ViolationTypeID)
            .Select(t => (int?)t.ViolationTypeID)
            .FirstOrDefaultAsync(cancellationToken);
        return id ?? throw new InvalidOperationException(
            $"No active violation type for kind {kind} was found for this school. Apply tenant migrations.");
    }

    public async Task<IReadOnlyList<ViolationTypeListItemDto>> ListTypesAsync(int schoolId, CancellationToken cancellationToken = default)
    {
        return await _db.ViolationTypes.AsNoTracking()
            .Where(t => t.SchoolID == schoolId)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.ViolationTypeID)
            .Select(t => new ViolationTypeListItemDto
            {
                ViolationTypeID = t.ViolationTypeID,
                SchoolID = t.SchoolID,
                Kind = (int)t.Kind,
                Name = t.Name,
                Description = t.Description,
                SortOrder = t.SortOrder,
                IsActive = t.IsActive,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ViolationListItemDto>> ListAsync(ViolationFilterDto filter, CancellationToken cancellationToken = default)
    {
        filter ??= new ViolationFilterDto();
        var q = _db.Violations.AsNoTracking().AsQueryable();

        if (filter.SchoolID is > 0)
        {
            q = q.Where(v => v.SchoolID == filter.SchoolID);
            if (filter.AcademicYearID is not > 0)
            {
                var activeYear = await GetActiveYearIdForSchoolAsync(filter.SchoolID.Value, cancellationToken);
                if (activeYear is > 0)
                    q = q.Where(v => v.AcademicYearID == activeYear.Value);
            }
        }

        if (filter.AcademicYearID is > 0)
            q = q.Where(v => v.AcademicYearID == filter.AcademicYearID);
        if (filter.SubjectEmployeeProfileID is > 0)
            q = q.Where(v => v.SubjectEmployeeProfileID == filter.SubjectEmployeeProfileID);
        if (filter.Status is >= 0)
            q = q.Where(v => (int)v.Status == filter.Status);

        var raw = await q
            .OrderByDescending(v => v.OpenedAtUtc)
            .ThenByDescending(v => v.ViolationID)
            .Select(v => new
            {
                v.ViolationID,
                v.SchoolID,
                v.AcademicYearID,
                v.ViolationTypeID,
                Tk = (int)v.ViolationType.Kind,
                TName = v.ViolationType.Name,
                v.SubjectEmployeeProfileID,
                SFirst = v.SubjectEmployeeProfile.FullName.FirstName,
                SMid = v.SubjectEmployeeProfile.FullName.MiddleName,
                SLast = v.SubjectEmployeeProfile.FullName.LastName,
                v.OpenedByEmployeeProfileID,
                OFirst = v.OpenedByEmployeeProfile != null ? v.OpenedByEmployeeProfile.FullName.FirstName : null,
                OMid = v.OpenedByEmployeeProfile != null ? v.OpenedByEmployeeProfile.FullName.MiddleName : null,
                OLast = v.OpenedByEmployeeProfile != null ? v.OpenedByEmployeeProfile.FullName.LastName : null,
                v.Title,
                Status = (int)v.Status,
                v.OpenedAtUtc,
                v.UpdatedAtUtc,
                v.ClosedAtUtc,
            })
            .ToListAsync(cancellationToken);

        return raw.Select(v => new ViolationListItemDto
        {
            ViolationID = v.ViolationID,
            SchoolID = v.SchoolID,
            AcademicYearID = v.AcademicYearID,
            ViolationTypeID = v.ViolationTypeID,
            ViolationTypeKind = v.Tk,
            ViolationTypeName = v.TName,
            SubjectEmployeeProfileID = v.SubjectEmployeeProfileID,
            SubjectEmployeeName = FormatPersonName(new Name { FirstName = v.SFirst, MiddleName = v.SMid, LastName = v.SLast }),
            OpenedByEmployeeProfileID = v.OpenedByEmployeeProfileID,
            OpenedByName = v.OpenedByEmployeeProfileID is > 0
                ? FormatPersonName(new Name
                {
                    FirstName = v.OFirst ?? string.Empty,
                    MiddleName = v.OMid ?? string.Empty,
                    LastName = v.OLast ?? string.Empty,
                })
                : null,
            Title = v.Title,
            Status = v.Status,
            OpenedAtUtc = v.OpenedAtUtc,
            UpdatedAtUtc = v.UpdatedAtUtc,
            ClosedAtUtc = v.ClosedAtUtc,
        }).ToList();
    }

    public async Task<ViolationDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var v = await _db.Violations.AsNoTracking()
            .Include(x => x.ViolationType)
            .Include(x => x.SubjectEmployeeProfile)
            .Include(x => x.OpenedByEmployeeProfile)
            .Include(x => x.Responses).ThenInclude(r => r.AuthorEmployeeProfile)
            .Include(x => x.Actions).ThenInclude(a => a.PerformedByEmployeeProfile)
            .Include(x => x.EscalationHistory).ThenInclude(h => h.PreviousViolationType)
            .Include(x => x.EscalationHistory).ThenInclude(h => h.NewViolationType)
            .Include(x => x.EscalationHistory).ThenInclude(h => h.ChangedByEmployeeProfile)
            .FirstOrDefaultAsync(x => x.ViolationID == id, cancellationToken);

        if (v == null) return null;

        return new ViolationDetailDto
        {
            ViolationID = v.ViolationID,
            SchoolID = v.SchoolID,
            AcademicYearID = v.AcademicYearID,
            ViolationTypeID = v.ViolationTypeID,
            ViolationTypeKind = (int)v.ViolationType.Kind,
            ViolationTypeName = v.ViolationType.Name,
            SubjectEmployeeProfileID = v.SubjectEmployeeProfileID,
            SubjectEmployeeName = FormatPersonName(v.SubjectEmployeeProfile.FullName),
            OpenedByEmployeeProfileID = v.OpenedByEmployeeProfileID,
            OpenedByName = v.OpenedByEmployeeProfile != null ? FormatPersonName(v.OpenedByEmployeeProfile.FullName) : null,
            Title = v.Title,
            Status = (int)v.Status,
            OpenedAtUtc = v.OpenedAtUtc,
            UpdatedAtUtc = v.UpdatedAtUtc,
            ClosedAtUtc = v.ClosedAtUtc,
            Details = v.Details,
            Responses = v.Responses
                .OrderBy(r => r.CreatedAtUtc)
                .ThenBy(r => r.ViolationResponseID)
                .Select(r => new ViolationResponseReadDto
                {
                    ViolationResponseID = r.ViolationResponseID,
                    ViolationID = r.ViolationID,
                    AuthorEmployeeProfileID = r.AuthorEmployeeProfileID,
                    AuthorName = r.AuthorEmployeeProfile != null ? FormatPersonName(r.AuthorEmployeeProfile.FullName) : null,
                    Body = r.Body,
                    CreatedAtUtc = r.CreatedAtUtc,
                })
                .ToList(),
            Actions = v.Actions
                .OrderByDescending(a => a.PerformedAtUtc)
                .ThenByDescending(a => a.ViolationActionID)
                .Select(a => new ViolationActionReadDto
                {
                    ViolationActionID = a.ViolationActionID,
                    ViolationID = a.ViolationID,
                    Category = (int)a.Category,
                    Title = a.Title,
                    Notes = a.Notes,
                    PerformedByEmployeeProfileID = a.PerformedByEmployeeProfileID,
                    PerformedByName = FormatPersonName(a.PerformedByEmployeeProfile.FullName),
                    PerformedAtUtc = a.PerformedAtUtc,
                })
                .ToList(),
            EscalationHistory = v.EscalationHistory
                .OrderBy(h => h.ChangedAtUtc)
                .ThenBy(h => h.ViolationEscalationHistoryID)
                .Select(h => new ViolationEscalationHistoryReadDto
                {
                    ViolationEscalationHistoryID = h.ViolationEscalationHistoryID,
                    ViolationID = h.ViolationID,
                    PreviousViolationTypeID = h.PreviousViolationTypeID,
                    PreviousKind = h.PreviousViolationType != null ? (int?)h.PreviousViolationType.Kind : null,
                    PreviousTypeName = h.PreviousViolationType?.Name,
                    NewViolationTypeID = h.NewViolationTypeID,
                    NewKind = (int)h.NewViolationType.Kind,
                    NewTypeName = h.NewViolationType.Name,
                    Reason = h.Reason,
                    ChangedByEmployeeProfileID = h.ChangedByEmployeeProfileID,
                    ChangedByName = FormatPersonName(h.ChangedByEmployeeProfile.FullName),
                    ChangedAtUtc = h.ChangedAtUtc,
                })
                .ToList(),
        };
    }

    public async Task<int> CreateAsync(ViolationWriteDto dto, CancellationToken cancellationToken = default)
    {
        var subjectOk = await _db.EmployeeProfiles.AsNoTracking()
            .AnyAsync(e => e.EmployeeProfileID == dto.SubjectEmployeeProfileID && e.SchoolID == dto.SchoolID, cancellationToken);
        if (!subjectOk)
            throw new InvalidOperationException("Subject employee profile was not found for this school.");

        int typeId;
        if (dto.ViolationTypeID > 0)
        {
            var typeOk = await _db.ViolationTypes.AsNoTracking()
                .AnyAsync(t => t.ViolationTypeID == dto.ViolationTypeID && t.SchoolID == dto.SchoolID && t.IsActive, cancellationToken);
            if (!typeOk)
                throw new InvalidOperationException("Violation type was not found for this school.");
            typeId = dto.ViolationTypeID;
        }
        else
        {
            typeId = await GetViolationTypeIdForSchoolKindAsync(dto.SchoolID, ViolationKind.Clarification, cancellationToken);
        }

        var yearId = dto.AcademicYearID is > 0
            ? dto.AcademicYearID
            : await GetActiveYearIdForSchoolAsync(dto.SchoolID, cancellationToken);

        var now = DateTime.UtcNow;
        var entity = new Violation
        {
            SchoolID = dto.SchoolID,
            AcademicYearID = yearId,
            ViolationTypeID = typeId,
            SubjectEmployeeProfileID = dto.SubjectEmployeeProfileID,
            OpenedByEmployeeProfileID = dto.OpenedByEmployeeProfileID,
            Title = dto.Title,
            Details = dto.Details,
            Status = (ViolationStatus)dto.Status,
            OpenedAtUtc = now,
            UpdatedAtUtc = now,
            ClosedAtUtc = ((ViolationStatus)dto.Status is ViolationStatus.Closed or ViolationStatus.Resolved or ViolationStatus.Cancelled)
                ? now
                : null,
        };

        _db.Violations.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.ViolationID;
    }

    public async Task UpdateAsync(int id, ViolationWriteDto dto, CancellationToken cancellationToken = default)
    {
        var v = await _db.Violations.FirstOrDefaultAsync(x => x.ViolationID == id, cancellationToken)
            ?? throw new InvalidOperationException("Violation was not found.");

        if (v.SchoolID != dto.SchoolID)
            throw new InvalidOperationException("School mismatch for this violation.");

        var subjectOk = await _db.EmployeeProfiles.AsNoTracking()
            .AnyAsync(e => e.EmployeeProfileID == dto.SubjectEmployeeProfileID && e.SchoolID == dto.SchoolID, cancellationToken);
        if (!subjectOk)
            throw new InvalidOperationException("Subject employee profile was not found for this school.");

        var now = DateTime.UtcNow;
        var newStatus = (ViolationStatus)dto.Status;
        v.SubjectEmployeeProfileID = dto.SubjectEmployeeProfileID;
        v.OpenedByEmployeeProfileID = dto.OpenedByEmployeeProfileID;
        v.Title = dto.Title;
        v.Details = dto.Details;
        v.Status = newStatus;
        v.UpdatedAtUtc = now;

        if (dto.AcademicYearID is > 0)
            v.AcademicYearID = dto.AcademicYearID;
        else
            v.AcademicYearID = await GetActiveYearIdForSchoolAsync(dto.SchoolID, cancellationToken);

        if (newStatus is ViolationStatus.Closed or ViolationStatus.Resolved or ViolationStatus.Cancelled)
            v.ClosedAtUtc ??= now;
        else
            v.ClosedAtUtc = null;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> AddResponseAsync(int violationId, ViolationResponseWriteDto dto, CancellationToken cancellationToken = default)
    {
        var v = await _db.Violations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ViolationID == violationId, cancellationToken)
            ?? throw new InvalidOperationException("Violation was not found.");

        if (dto.AuthorEmployeeProfileID is > 0)
        {
            var ok = await _db.EmployeeProfiles.AsNoTracking()
                .AnyAsync(e => e.EmployeeProfileID == dto.AuthorEmployeeProfileID && e.SchoolID == v.SchoolID, cancellationToken);
            if (!ok)
                throw new InvalidOperationException("Author employee profile was not found for this school.");
        }

        var row = new ViolationResponse
        {
            ViolationID = violationId,
            AuthorEmployeeProfileID = dto.AuthorEmployeeProfileID,
            Body = dto.Body,
            CreatedAtUtc = DateTime.UtcNow,
        };
        _db.ViolationResponses.Add(row);

        await _db.Violations.Where(x => x.ViolationID == violationId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.UpdatedAtUtc, DateTime.UtcNow), cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return row.ViolationResponseID;
    }

    public async Task<int> AddActionAsync(int violationId, ViolationActionWriteDto dto, CancellationToken cancellationToken = default)
    {
        var v = await _db.Violations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ViolationID == violationId, cancellationToken)
            ?? throw new InvalidOperationException("Violation was not found.");

        var actorOk = await _db.EmployeeProfiles.AsNoTracking()
            .AnyAsync(e => e.EmployeeProfileID == dto.PerformedByEmployeeProfileID && e.SchoolID == v.SchoolID, cancellationToken);
        if (!actorOk)
            throw new InvalidOperationException("Performing employee profile was not found for this school.");

        var row = new ViolationAction
        {
            ViolationID = violationId,
            Category = (ViolationActionCategory)dto.Category,
            Title = dto.Title,
            Notes = dto.Notes,
            PerformedByEmployeeProfileID = dto.PerformedByEmployeeProfileID,
            PerformedAtUtc = DateTime.UtcNow,
        };
        _db.ViolationActions.Add(row);

        await _db.Violations.Where(x => x.ViolationID == violationId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.UpdatedAtUtc, DateTime.UtcNow), cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return row.ViolationActionID;
    }

    public async Task EscalateAsync(int violationId, ViolationEscalateDto dto, CancellationToken cancellationToken = default)
    {
        var v = await _db.Violations
            .Include(x => x.ViolationType)
            .FirstOrDefaultAsync(x => x.ViolationID == violationId, cancellationToken)
            ?? throw new InvalidOperationException("Violation was not found.");

        var newType = await _db.ViolationTypes
            .FirstOrDefaultAsync(t => t.ViolationTypeID == dto.NewViolationTypeID && t.SchoolID == v.SchoolID && t.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("New violation type was not found for this school.");

        if ((int)newType.Kind < (int)v.ViolationType.Kind)
            throw new InvalidOperationException("Cannot move to an earlier escalation step.");

        if (newType.ViolationTypeID == v.ViolationTypeID)
            throw new InvalidOperationException("The violation is already at this type.");

        var changerOk = await _db.EmployeeProfiles.AsNoTracking()
            .AnyAsync(e => e.EmployeeProfileID == dto.ChangedByEmployeeProfileID && e.SchoolID == v.SchoolID, cancellationToken);
        if (!changerOk)
            throw new InvalidOperationException("Changed-by employee profile was not found for this school.");

        var now = DateTime.UtcNow;
        var previousId = v.ViolationTypeID;
        v.ViolationTypeID = newType.ViolationTypeID;
        v.UpdatedAtUtc = now;

        _db.ViolationEscalationHistories.Add(new ViolationEscalationHistory
        {
            ViolationID = violationId,
            PreviousViolationTypeID = previousId,
            NewViolationTypeID = newType.ViolationTypeID,
            Reason = dto.Reason,
            ChangedByEmployeeProfileID = dto.ChangedByEmployeeProfileID,
            ChangedAtUtc = now,
        });

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var v = await _db.Violations
            .Include(x => x.Responses)
            .Include(x => x.Actions)
            .Include(x => x.EscalationHistory)
            .FirstOrDefaultAsync(x => x.ViolationID == id, cancellationToken);
        if (v == null) return false;

        _db.ViolationEscalationHistories.RemoveRange(v.EscalationHistory);
        _db.ViolationActions.RemoveRange(v.Actions);
        _db.ViolationResponses.RemoveRange(v.Responses);
        _db.Violations.Remove(v);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<int?> GetSchoolIdForViolationAsync(int violationId, CancellationToken cancellationToken = default) =>
        _db.Violations.AsNoTracking()
            .Where(v => v.ViolationID == violationId)
            .Select(v => (int?)v.SchoolID)
            .FirstOrDefaultAsync(cancellationToken);
}
