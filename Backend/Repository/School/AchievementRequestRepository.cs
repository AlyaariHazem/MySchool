using Backend.Data;
using Backend.DTOS.School.Achievement;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School;

public class AchievementRequestRepository : IAchievementRequestRepository
{
    private readonly TenantDbContext _db;

    public AchievementRequestRepository(TenantDbContext db)
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

    public async Task<IReadOnlyList<AchievementCatalogItemDto>> ListCatalogAsync(int schoolId, int? academicYearId, CancellationToken cancellationToken = default)
    {
        var effectiveYearId = academicYearId is > 0
            ? academicYearId
            : await GetActiveYearIdForSchoolAsync(schoolId, cancellationToken);
        var q = _db.Achievements.AsNoTracking()
            .Where(a => a.SchoolID == schoolId && a.IsActive);
        if (effectiveYearId is > 0)
            q = q.Where(a => a.AcademicYearID == null || a.AcademicYearID == effectiveYearId);
        return await q
            .OrderBy(a => a.SortOrder)
            .ThenBy(a => a.Title)
            .Select(a => new AchievementCatalogItemDto
            {
                AchievementID = a.AchievementID,
                Code = a.Code,
                Title = a.Title,
                DefaultPoints = a.DefaultPoints,
                AcademicYearID = a.AcademicYearID
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AchievementRequestListItemDto>> ListAsync(AchievementRequestFilterDto filter, CancellationToken cancellationToken = default)
    {
        filter ??= new AchievementRequestFilterDto();
        var q = _db.AchievementRequests.AsNoTracking().AsQueryable();

        if (filter.SchoolID is > 0)
        {
            q = q.Where(r => r.SchoolID == filter.SchoolID);
            var activeYear = await GetActiveYearIdForSchoolAsync(filter.SchoolID.Value, cancellationToken);
            if (activeYear is > 0 && filter.AcademicYearID is not > 0)
                q = q.Where(r => r.AcademicYearID == activeYear.Value);
        }

        if (filter.AcademicYearID is > 0)
            q = q.Where(r => r.AcademicYearID == filter.AcademicYearID);
        if (filter.EmployeeProfileID is > 0)
            q = q.Where(r => r.EmployeeProfileID == filter.EmployeeProfileID);
        if (filter.Status is >= 0)
            q = q.Where(r => (int)r.Status == filter.Status);

        var raw = await q
            .OrderByDescending(r => r.SubmittedAtUtc)
            .ThenByDescending(r => r.AchievementRequestID)
            .Select(r => new
            {
                r.AchievementRequestID,
                r.SchoolID,
                r.AcademicYearID,
                r.EmployeeProfileID,
                EFirst = r.EmployeeProfile.FullName.FirstName,
                EMid = r.EmployeeProfile.FullName.MiddleName,
                ELast = r.EmployeeProfile.FullName.LastName,
                r.AchievementID,
                CatTitle = r.Achievement != null ? r.Achievement.Title : null,
                r.CustomTitle,
                Status = (int)r.Status,
                r.SubmittedAtUtc,
                r.ResolvedAtUtc
            })
            .ToListAsync(cancellationToken);

        return raw.Select(r => new AchievementRequestListItemDto
        {
            AchievementRequestID = r.AchievementRequestID,
            SchoolID = r.SchoolID,
            AcademicYearID = r.AcademicYearID,
            EmployeeProfileID = r.EmployeeProfileID,
            EmployeeName = FormatPersonName(new Name { FirstName = r.EFirst, MiddleName = r.EMid, LastName = r.ELast }),
            AchievementID = r.AchievementID,
            AchievementTitle = r.CatTitle,
            CustomTitle = r.CustomTitle,
            Status = r.Status,
            SubmittedAtUtc = r.SubmittedAtUtc,
            ResolvedAtUtc = r.ResolvedAtUtc
        }).ToList();
    }

    public async Task<AchievementRequestDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var row = await _db.AchievementRequests.AsNoTracking()
            .Include(r => r.EmployeeProfile)
            .Include(r => r.Achievement)
            .Include(r => r.Approvals).ThenInclude(a => a.ApproverEmployeeProfile)
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.AchievementRequestID == id, cancellationToken);
        if (row == null)
            return null;

        var ledger = await _db.AchievementPointsLedgers.AsNoTracking()
            .Where(l => l.AchievementRequestID == id)
            .OrderByDescending(l => l.CreatedAtUtc)
            .Select(l => new AchievementPointsLedgerReadDto
            {
                AchievementPointsLedgerID = l.AchievementPointsLedgerID,
                DeltaPoints = l.DeltaPoints,
                Reason = l.Reason,
                CreatedAtUtc = l.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        var empName = FormatPersonName(row.EmployeeProfile?.FullName);
        var catTitle = row.Achievement?.Title;

        return new AchievementRequestDetailDto
        {
            AchievementRequestID = row.AchievementRequestID,
            SchoolID = row.SchoolID,
            AcademicYearID = row.AcademicYearID,
            EmployeeProfileID = row.EmployeeProfileID,
            EmployeeName = empName,
            AchievementID = row.AchievementID,
            AchievementTitle = catTitle,
            CustomTitle = row.CustomTitle,
            Status = (int)row.Status,
            SubmittedAtUtc = row.SubmittedAtUtc,
            ResolvedAtUtc = row.ResolvedAtUtc,
            Notes = row.Notes,
            UpdatedAtUtc = row.UpdatedAtUtc,
            Approvals = row.Approvals.OrderBy(a => a.SortOrder).Select(a => new AchievementApprovalReadDto
            {
                AchievementApprovalID = a.AchievementApprovalID,
                ApproverEmployeeProfileID = a.ApproverEmployeeProfileID,
                ApproverName = FormatPersonName(a.ApproverEmployeeProfile?.FullName),
                Decision = (int)a.Decision,
                Comment = a.Comment,
                SortOrder = a.SortOrder,
                DecidedAtUtc = a.DecidedAtUtc,
                CreatedAtUtc = a.CreatedAtUtc
            }).ToList(),
            Attachments = row.Attachments.OrderByDescending(x => x.UploadedAtUtc).Select(x => new AchievementAttachmentReadDto
            {
                AchievementAttachmentID = x.AchievementAttachmentID,
                FileName = x.FileName,
                ContentType = x.ContentType,
                FileSizeBytes = x.FileSizeBytes,
                UploadedAtUtc = x.UploadedAtUtc
            }).ToList(),
            LedgerEntries = ledger
        };
    }

    public async Task<int> CreateAsync(AchievementRequestWriteDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.AchievementID is not > 0 && string.IsNullOrWhiteSpace(dto.CustomTitle))
            throw new InvalidOperationException("Either AchievementID or CustomTitle is required.");

        if (dto.AchievementID is > 0)
        {
            var exists = await _db.Achievements.AsNoTracking()
                .AnyAsync(a => a.AchievementID == dto.AchievementID && a.SchoolID == dto.SchoolID, cancellationToken);
            if (!exists)
                throw new InvalidOperationException("Achievement definition was not found for this school.");
        }

        var empOk = await _db.EmployeeProfiles.AsNoTracking()
            .AnyAsync(e => e.EmployeeProfileID == dto.EmployeeProfileID && e.SchoolID == dto.SchoolID, cancellationToken);
        if (!empOk)
            throw new InvalidOperationException("Employee profile was not found for this school.");

        var academicYearId = dto.AcademicYearID is > 0
            ? dto.AcademicYearID.Value
            : await GetActiveYearIdForSchoolAsync(dto.SchoolID, cancellationToken) ?? 0;
        if (academicYearId <= 0)
            throw new InvalidOperationException("No academic year is configured for this school.");
        var yearOk = await _db.Years.AsNoTracking()
            .AnyAsync(y => y.YearID == academicYearId && y.SchoolID == dto.SchoolID, cancellationToken);
        if (!yearOk)
            throw new InvalidOperationException("Academic year was not found for this school.");

        var entity = new AchievementRequest
        {
            SchoolID = dto.SchoolID,
            AcademicYearID = academicYearId,
            EmployeeProfileID = dto.EmployeeProfileID,
            AchievementID = dto.AchievementID is > 0 ? dto.AchievementID : null,
            CustomTitle = string.IsNullOrWhiteSpace(dto.CustomTitle) ? null : dto.CustomTitle.Trim(),
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
            Status = (AchievementRequestStatus)dto.Status,
            SubmittedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _db.AchievementRequests.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.AchievementRequestID;
    }

    public async Task UpdateAsync(int id, AchievementRequestWriteDto dto, CancellationToken cancellationToken = default)
    {
        var row = await _db.AchievementRequests
            .FirstOrDefaultAsync(r => r.AchievementRequestID == id, cancellationToken)
            ?? throw new KeyNotFoundException("Request was not found.");

        if (dto.AchievementID is not > 0 && string.IsNullOrWhiteSpace(dto.CustomTitle))
            throw new InvalidOperationException("Either AchievementID or CustomTitle is required.");

        if (dto.AchievementID is > 0)
        {
            var exists = await _db.Achievements.AsNoTracking()
                .AnyAsync(a => a.AchievementID == dto.AchievementID && a.SchoolID == dto.SchoolID, cancellationToken);
            if (!exists)
                throw new InvalidOperationException("Achievement definition was not found for this school.");
        }

        row.SchoolID = dto.SchoolID;
        if (dto.AcademicYearID is > 0)
        {
            var yOk = await _db.Years.AsNoTracking()
                .AnyAsync(y => y.YearID == dto.AcademicYearID && y.SchoolID == dto.SchoolID, cancellationToken);
            if (!yOk)
                throw new InvalidOperationException("Academic year was not found for this school.");
            row.AcademicYearID = dto.AcademicYearID.Value;
        }

        row.EmployeeProfileID = dto.EmployeeProfileID;
        row.AchievementID = dto.AchievementID is > 0 ? dto.AchievementID : null;
        row.CustomTitle = string.IsNullOrWhiteSpace(dto.CustomTitle) ? null : dto.CustomTitle.Trim();
        row.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
        row.Status = (AchievementRequestStatus)dto.Status;
        row.UpdatedAtUtc = DateTime.UtcNow;
        if (row.Status is AchievementRequestStatus.Approved or AchievementRequestStatus.Rejected or AchievementRequestStatus.Cancelled)
            row.ResolvedAtUtc ??= DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var row = await _db.AchievementRequests
            .FirstOrDefaultAsync(r => r.AchievementRequestID == id, cancellationToken);
        if (row == null)
            return false;
        _db.AchievementRequests.Remove(row);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<int?> GetSchoolIdForRequestAsync(int requestId, CancellationToken cancellationToken = default)
    {
        return _db.AchievementRequests.AsNoTracking()
            .Where(r => r.AchievementRequestID == requestId)
            .Select(r => (int?)r.SchoolID)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
