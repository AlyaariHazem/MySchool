using Backend.Data;
using Backend.DTOS.School.Concern;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School;

public class ConcernRepository : IConcernRepository
{
    private readonly TenantDbContext _db;

    public ConcernRepository(TenantDbContext db)
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

    private async Task EnsureDefaultCategoriesAsync(int schoolId, CancellationToken cancellationToken)
    {
        if (await _db.ConcernCategories.AnyAsync(c => c.SchoolID == schoolId, cancellationToken))
            return;

        _db.ConcernCategories.AddRange(
            new ConcernCategory
            {
                SchoolID = schoolId,
                Code = "GENERAL_COMPLAINT",
                CategoryKind = ConcernCategoryKind.Complaint,
                Name = "General complaint",
                NameAr = "شكوى عامة",
                Description = "Default category for complaints.",
                IsActive = true,
            },
            new ConcernCategory
            {
                SchoolID = schoolId,
                Code = "GENERAL_SUGGESTION",
                CategoryKind = ConcernCategoryKind.Suggestion,
                Name = "General suggestion",
                NameAr = "مقترح عام",
                Description = "Default category for suggestions.",
                IsActive = true,
            },
            new ConcernCategory
            {
                SchoolID = schoolId,
                Code = "MIXED_OTHER",
                CategoryKind = ConcernCategoryKind.Both,
                Name = "Other",
                NameAr = "أخرى",
                Description = "Complaints or suggestions that do not fit a specific category.",
                IsActive = true,
            });

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ConcernCategoryListItemDto>> ListCategoriesAsync(ConcernCategoriesFilterDto filter, CancellationToken cancellationToken = default)
    {
        await EnsureDefaultCategoriesAsync(filter.SchoolID, cancellationToken);

        var q = _db.ConcernCategories.AsNoTracking()
            .Where(c => c.SchoolID == filter.SchoolID && c.IsActive);

        if (filter.CategoryKind is int ck && ck is >= 0 and <= 1)
        {
            var fk = (ConcernCategoryKind)ck;
            q = q.Where(c => c.CategoryKind == ConcernCategoryKind.Both || c.CategoryKind == fk);
        }

        return await q
            .OrderBy(c => c.Code)
            .Select(c => new ConcernCategoryListItemDto
            {
                ConcernCategoryID = c.ConcernCategoryID,
                SchoolID = c.SchoolID,
                Code = c.Code,
                CategoryKind = (int)c.CategoryKind,
                Name = c.Name,
                NameAr = c.NameAr,
                Description = c.Description,
                IsActive = c.IsActive,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<int?> GetEmployeeProfileIdForUserInSchoolAsync(string? userId, int schoolId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            return null;
        return await _db.EmployeeProfiles.AsNoTracking()
            .Where(e => e.UserId == userId && e.SchoolID == schoolId && e.IsActive)
            .OrderBy(e => e.EmployeeProfileID)
            .Select(e => (int?)e.EmployeeProfileID)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task ValidateComplaintCategoryAsync(int categoryId, int schoolId, CancellationToken cancellationToken)
    {
        var cat = await _db.ConcernCategories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.ConcernCategoryID == categoryId && c.SchoolID == schoolId && c.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("Concern category was not found for this school.");

        if (cat.CategoryKind != ConcernCategoryKind.Complaint && cat.CategoryKind != ConcernCategoryKind.Both)
            throw new InvalidOperationException("This category cannot be used for complaints.");
    }

    private async Task ValidateSuggestionCategoryAsync(int categoryId, int schoolId, CancellationToken cancellationToken)
    {
        var cat = await _db.ConcernCategories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.ConcernCategoryID == categoryId && c.SchoolID == schoolId && c.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("Concern category was not found for this school.");

        if (cat.CategoryKind != ConcernCategoryKind.Suggestion && cat.CategoryKind != ConcernCategoryKind.Both)
            throw new InvalidOperationException("This category cannot be used for suggestions.");
    }

    public async Task<IReadOnlyList<ComplaintListItemDto>> ListComplaintsAsync(ConcernFilterDto filter, CancellationToken cancellationToken = default)
    {
        filter ??= new ConcernFilterDto();
        var q = _db.Complaints.AsNoTracking().AsQueryable();

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
        if (filter.Status is >= 0)
            q = q.Where(r => (int)r.Status == filter.Status);
        if (filter.SubmitterEmployeeProfileID is > 0)
            q = q.Where(r => r.SubmitterEmployeeProfileID == filter.SubmitterEmployeeProfileID);

        var raw = await q
            .OrderByDescending(r => r.SubmittedAtUtc)
            .ThenByDescending(r => r.ComplaintID)
            .Select(r => new
            {
                r.ComplaintID,
                r.SchoolID,
                r.AcademicYearID,
                r.ConcernCategoryID,
                CatCode = r.ConcernCategory.Code,
                CatName = r.ConcernCategory.Name,
                CatNameAr = r.ConcernCategory.NameAr,
                r.SubmitterEmployeeProfileID,
                SFirst = r.SubmitterEmployeeProfile.FullName.FirstName,
                SMid = r.SubmitterEmployeeProfile.FullName.MiddleName,
                SLast = r.SubmitterEmployeeProfile.FullName.LastName,
                r.AssignedToEmployeeProfileID,
                AFirst = r.AssignedToEmployeeProfile != null ? r.AssignedToEmployeeProfile.FullName.FirstName : null,
                AMid = r.AssignedToEmployeeProfile != null ? r.AssignedToEmployeeProfile.FullName.MiddleName : null,
                ALast = r.AssignedToEmployeeProfile != null ? r.AssignedToEmployeeProfile.FullName.LastName : null,
                r.Title,
                St = (int)r.Status,
                r.SubmittedAtUtc,
                r.UpdatedAtUtc,
                r.ClosedAtUtc,
            })
            .ToListAsync(cancellationToken);

        return raw.Select(r => new ComplaintListItemDto
        {
            ComplaintID = r.ComplaintID,
            SchoolID = r.SchoolID,
            AcademicYearID = r.AcademicYearID,
            ConcernCategoryID = r.ConcernCategoryID,
            CategoryCode = r.CatCode,
            CategoryName = r.CatName,
            CategoryNameAr = r.CatNameAr,
            SubmitterEmployeeProfileID = r.SubmitterEmployeeProfileID,
            SubmitterName = FormatPersonName(new Name { FirstName = r.SFirst, MiddleName = r.SMid, LastName = r.SLast }),
            AssignedToEmployeeProfileID = r.AssignedToEmployeeProfileID,
            AssignedToName = r.AssignedToEmployeeProfileID is > 0
                ? FormatPersonName(new Name { FirstName = r.AFirst!, MiddleName = r.AMid, LastName = r.ALast! })
                : null,
            Title = r.Title,
            Status = r.St,
            SubmittedAtUtc = r.SubmittedAtUtc,
            UpdatedAtUtc = r.UpdatedAtUtc,
            ClosedAtUtc = r.ClosedAtUtc,
        }).ToList();
    }

    public async Task<ComplaintDetailDto?> GetComplaintByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var r = await _db.Complaints.AsNoTracking()
            .Include(x => x.ConcernCategory)
            .Include(x => x.SubmitterEmployeeProfile)
            .Include(x => x.AssignedToEmployeeProfile)
            .Include(x => x.ActionLogs).ThenInclude(a => a.ActorEmployeeProfile)
            .FirstOrDefaultAsync(x => x.ComplaintID == id, cancellationToken);

        if (r == null) return null;

        var baseRow = new ComplaintListItemDto
        {
            ComplaintID = r.ComplaintID,
            SchoolID = r.SchoolID,
            AcademicYearID = r.AcademicYearID,
            ConcernCategoryID = r.ConcernCategoryID,
            CategoryCode = r.ConcernCategory.Code,
            CategoryName = r.ConcernCategory.Name,
            CategoryNameAr = r.ConcernCategory.NameAr,
            SubmitterEmployeeProfileID = r.SubmitterEmployeeProfileID,
            SubmitterName = FormatPersonName(r.SubmitterEmployeeProfile.FullName),
            AssignedToEmployeeProfileID = r.AssignedToEmployeeProfileID,
            AssignedToName = r.AssignedToEmployeeProfile != null ? FormatPersonName(r.AssignedToEmployeeProfile.FullName) : null,
            Title = r.Title,
            Status = (int)r.Status,
            SubmittedAtUtc = r.SubmittedAtUtc,
            UpdatedAtUtc = r.UpdatedAtUtc,
            ClosedAtUtc = r.ClosedAtUtc,
        };

        return new ComplaintDetailDto
        {
            ComplaintID = baseRow.ComplaintID,
            SchoolID = baseRow.SchoolID,
            AcademicYearID = baseRow.AcademicYearID,
            ConcernCategoryID = baseRow.ConcernCategoryID,
            CategoryCode = baseRow.CategoryCode,
            CategoryName = baseRow.CategoryName,
            CategoryNameAr = baseRow.CategoryNameAr,
            SubmitterEmployeeProfileID = baseRow.SubmitterEmployeeProfileID,
            SubmitterName = baseRow.SubmitterName,
            AssignedToEmployeeProfileID = baseRow.AssignedToEmployeeProfileID,
            AssignedToName = baseRow.AssignedToName,
            Title = baseRow.Title,
            Status = baseRow.Status,
            SubmittedAtUtc = baseRow.SubmittedAtUtc,
            UpdatedAtUtc = baseRow.UpdatedAtUtc,
            ClosedAtUtc = baseRow.ClosedAtUtc,
            Details = r.Details,
            ActionLogs = r.ActionLogs
                .OrderBy(a => a.CreatedAtUtc)
                .ThenBy(a => a.ConcernActionLogID)
                .Select(a => new ConcernActionLogReadDto
                {
                    ConcernActionLogID = a.ConcernActionLogID,
                    ActionKind = (int)a.ActionKind,
                    OldStatus = a.OldStatus != null ? (int)a.OldStatus : null,
                    NewStatus = a.NewStatus != null ? (int)a.NewStatus : null,
                    Comment = a.Comment,
                    ActorEmployeeProfileID = a.ActorEmployeeProfileID,
                    ActorName = a.ActorEmployeeProfile != null ? FormatPersonName(a.ActorEmployeeProfile.FullName) : null,
                    CreatedAtUtc = a.CreatedAtUtc,
                })
                .ToList(),
        };
    }

    public async Task<int> CreateComplaintAsync(ComplaintWriteDto dto, int? actorEmployeeProfileId, CancellationToken cancellationToken = default)
    {
        await EnsureDefaultCategoriesAsync(dto.SchoolID, cancellationToken);
        await ValidateComplaintCategoryAsync(dto.ConcernCategoryID, dto.SchoolID, cancellationToken);

        var subOk = await _db.EmployeeProfiles.AsNoTracking()
            .AnyAsync(e => e.EmployeeProfileID == dto.SubmitterEmployeeProfileID && e.SchoolID == dto.SchoolID, cancellationToken);
        if (!subOk)
            throw new InvalidOperationException("Submitter employee profile was not found for this school.");

        if (dto.AssignedToEmployeeProfileID is > 0)
        {
            var aOk = await _db.EmployeeProfiles.AsNoTracking()
                .AnyAsync(e => e.EmployeeProfileID == dto.AssignedToEmployeeProfileID && e.SchoolID == dto.SchoolID, cancellationToken);
            if (!aOk)
                throw new InvalidOperationException("Assignee employee profile was not found for this school.");
        }

        var yearId = dto.AcademicYearID is > 0
            ? dto.AcademicYearID!.Value
            : await GetActiveYearIdForSchoolAsync(dto.SchoolID, cancellationToken)
              ?? throw new InvalidOperationException("No academic year is configured for this school.");

        var yearOk = await _db.Years.AsNoTracking()
            .AnyAsync(y => y.YearID == yearId && y.SchoolID == dto.SchoolID, cancellationToken);
        if (!yearOk)
            throw new InvalidOperationException("Academic year does not belong to this school.");

        var now = DateTime.UtcNow;
        var status = (ConcernStatus)dto.Status;
        var entity = new Complaint
        {
            SchoolID = dto.SchoolID,
            AcademicYearID = yearId,
            ConcernCategoryID = dto.ConcernCategoryID,
            SubmitterEmployeeProfileID = dto.SubmitterEmployeeProfileID,
            AssignedToEmployeeProfileID = dto.AssignedToEmployeeProfileID is > 0 ? dto.AssignedToEmployeeProfileID : null,
            Title = dto.Title,
            Details = dto.Details,
            Status = status,
            SubmittedAtUtc = now,
            UpdatedAtUtc = now,
            ClosedAtUtc = status is ConcernStatus.Resolved or ConcernStatus.Rejected or ConcernStatus.Closed or ConcernStatus.Cancelled
                ? now
                : null,
        };

        _db.Complaints.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        _db.ConcernActionLogs.Add(new ConcernActionLog
        {
            ComplaintID = entity.ComplaintID,
            SuggestionID = null,
            ActorEmployeeProfileID = actorEmployeeProfileId,
            ActionKind = ConcernActionKind.Created,
            OldStatus = null,
            NewStatus = status,
            Comment = null,
            CreatedAtUtc = now,
        });
        await _db.SaveChangesAsync(cancellationToken);

        return entity.ComplaintID;
    }

    public async Task UpdateComplaintAsync(int id, ComplaintWriteDto dto, int? actorEmployeeProfileId, CancellationToken cancellationToken = default)
    {
        var r = await _db.Complaints.FirstOrDefaultAsync(x => x.ComplaintID == id, cancellationToken)
            ?? throw new InvalidOperationException("Complaint was not found.");

        if (r.SchoolID != dto.SchoolID)
            throw new InvalidOperationException("School mismatch for this complaint.");

        await ValidateComplaintCategoryAsync(dto.ConcernCategoryID, dto.SchoolID, cancellationToken);

        var subOk = await _db.EmployeeProfiles.AsNoTracking()
            .AnyAsync(e => e.EmployeeProfileID == dto.SubmitterEmployeeProfileID && e.SchoolID == dto.SchoolID, cancellationToken);
        if (!subOk)
            throw new InvalidOperationException("Submitter employee profile was not found for this school.");

        if (dto.AssignedToEmployeeProfileID is > 0)
        {
            var aOk = await _db.EmployeeProfiles.AsNoTracking()
                .AnyAsync(e => e.EmployeeProfileID == dto.AssignedToEmployeeProfileID && e.SchoolID == dto.SchoolID, cancellationToken);
            if (!aOk)
                throw new InvalidOperationException("Assignee employee profile was not found for this school.");
        }

        var yearId = dto.AcademicYearID is > 0 ? dto.AcademicYearID!.Value : r.AcademicYearID;
        var yearOk = await _db.Years.AsNoTracking()
            .AnyAsync(y => y.YearID == yearId && y.SchoolID == dto.SchoolID, cancellationToken);
        if (!yearOk)
            throw new InvalidOperationException("Academic year does not belong to this school.");

        var now = DateTime.UtcNow;
        var oldStatus = r.Status;
        var newStatus = (ConcernStatus)dto.Status;

        if (oldStatus != newStatus)
        {
            _db.ConcernActionLogs.Add(new ConcernActionLog
            {
                ComplaintID = r.ComplaintID,
                SuggestionID = null,
                ActorEmployeeProfileID = actorEmployeeProfileId,
                ActionKind = ConcernActionKind.StatusChanged,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                Comment = null,
                CreatedAtUtc = now,
            });
        }

        r.AcademicYearID = yearId;
        r.ConcernCategoryID = dto.ConcernCategoryID;
        r.SubmitterEmployeeProfileID = dto.SubmitterEmployeeProfileID;
        r.AssignedToEmployeeProfileID = dto.AssignedToEmployeeProfileID is > 0 ? dto.AssignedToEmployeeProfileID : null;
        r.Title = dto.Title;
        r.Details = dto.Details;
        r.Status = newStatus;
        r.UpdatedAtUtc = now;

        if (newStatus is ConcernStatus.Resolved or ConcernStatus.Rejected or ConcernStatus.Closed or ConcernStatus.Cancelled)
            r.ClosedAtUtc ??= now;
        else
            r.ClosedAtUtc = null;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<int?> GetSchoolIdForComplaintAsync(int complaintId, CancellationToken cancellationToken = default)
    {
        return _db.Complaints.AsNoTracking()
            .Where(x => x.ComplaintID == complaintId)
            .Select(x => (int?)x.SchoolID)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SuggestionListItemDto>> ListSuggestionsAsync(ConcernFilterDto filter, CancellationToken cancellationToken = default)
    {
        filter ??= new ConcernFilterDto();
        var q = _db.Suggestions.AsNoTracking().AsQueryable();

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
        if (filter.Status is >= 0)
            q = q.Where(r => (int)r.Status == filter.Status);
        if (filter.SubmitterEmployeeProfileID is > 0)
            q = q.Where(r => r.SubmitterEmployeeProfileID == filter.SubmitterEmployeeProfileID);

        var raw = await q
            .OrderByDescending(r => r.SubmittedAtUtc)
            .ThenByDescending(r => r.SuggestionID)
            .Select(r => new
            {
                r.SuggestionID,
                r.SchoolID,
                r.AcademicYearID,
                r.ConcernCategoryID,
                CatCode = r.ConcernCategory.Code,
                CatName = r.ConcernCategory.Name,
                CatNameAr = r.ConcernCategory.NameAr,
                r.SubmitterEmployeeProfileID,
                SFirst = r.SubmitterEmployeeProfile.FullName.FirstName,
                SMid = r.SubmitterEmployeeProfile.FullName.MiddleName,
                SLast = r.SubmitterEmployeeProfile.FullName.LastName,
                r.AssignedToEmployeeProfileID,
                AFirst = r.AssignedToEmployeeProfile != null ? r.AssignedToEmployeeProfile.FullName.FirstName : null,
                AMid = r.AssignedToEmployeeProfile != null ? r.AssignedToEmployeeProfile.FullName.MiddleName : null,
                ALast = r.AssignedToEmployeeProfile != null ? r.AssignedToEmployeeProfile.FullName.LastName : null,
                r.Title,
                St = (int)r.Status,
                r.SubmittedAtUtc,
                r.UpdatedAtUtc,
                r.ClosedAtUtc,
            })
            .ToListAsync(cancellationToken);

        return raw.Select(r => new SuggestionListItemDto
        {
            SuggestionID = r.SuggestionID,
            SchoolID = r.SchoolID,
            AcademicYearID = r.AcademicYearID,
            ConcernCategoryID = r.ConcernCategoryID,
            CategoryCode = r.CatCode,
            CategoryName = r.CatName,
            CategoryNameAr = r.CatNameAr,
            SubmitterEmployeeProfileID = r.SubmitterEmployeeProfileID,
            SubmitterName = FormatPersonName(new Name { FirstName = r.SFirst, MiddleName = r.SMid, LastName = r.SLast }),
            AssignedToEmployeeProfileID = r.AssignedToEmployeeProfileID,
            AssignedToName = r.AssignedToEmployeeProfileID is > 0
                ? FormatPersonName(new Name { FirstName = r.AFirst!, MiddleName = r.AMid, LastName = r.ALast! })
                : null,
            Title = r.Title,
            Status = r.St,
            SubmittedAtUtc = r.SubmittedAtUtc,
            UpdatedAtUtc = r.UpdatedAtUtc,
            ClosedAtUtc = r.ClosedAtUtc,
        }).ToList();
    }

    public async Task<SuggestionDetailDto?> GetSuggestionByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var r = await _db.Suggestions.AsNoTracking()
            .Include(x => x.ConcernCategory)
            .Include(x => x.SubmitterEmployeeProfile)
            .Include(x => x.AssignedToEmployeeProfile)
            .Include(x => x.ActionLogs).ThenInclude(a => a.ActorEmployeeProfile)
            .FirstOrDefaultAsync(x => x.SuggestionID == id, cancellationToken);

        if (r == null) return null;

        var baseRow = new SuggestionListItemDto
        {
            SuggestionID = r.SuggestionID,
            SchoolID = r.SchoolID,
            AcademicYearID = r.AcademicYearID,
            ConcernCategoryID = r.ConcernCategoryID,
            CategoryCode = r.ConcernCategory.Code,
            CategoryName = r.ConcernCategory.Name,
            CategoryNameAr = r.ConcernCategory.NameAr,
            SubmitterEmployeeProfileID = r.SubmitterEmployeeProfileID,
            SubmitterName = FormatPersonName(r.SubmitterEmployeeProfile.FullName),
            AssignedToEmployeeProfileID = r.AssignedToEmployeeProfileID,
            AssignedToName = r.AssignedToEmployeeProfile != null ? FormatPersonName(r.AssignedToEmployeeProfile.FullName) : null,
            Title = r.Title,
            Status = (int)r.Status,
            SubmittedAtUtc = r.SubmittedAtUtc,
            UpdatedAtUtc = r.UpdatedAtUtc,
            ClosedAtUtc = r.ClosedAtUtc,
        };

        return new SuggestionDetailDto
        {
            SuggestionID = baseRow.SuggestionID,
            SchoolID = baseRow.SchoolID,
            AcademicYearID = baseRow.AcademicYearID,
            ConcernCategoryID = baseRow.ConcernCategoryID,
            CategoryCode = baseRow.CategoryCode,
            CategoryName = baseRow.CategoryName,
            CategoryNameAr = baseRow.CategoryNameAr,
            SubmitterEmployeeProfileID = baseRow.SubmitterEmployeeProfileID,
            SubmitterName = baseRow.SubmitterName,
            AssignedToEmployeeProfileID = baseRow.AssignedToEmployeeProfileID,
            AssignedToName = baseRow.AssignedToName,
            Title = baseRow.Title,
            Status = baseRow.Status,
            SubmittedAtUtc = baseRow.SubmittedAtUtc,
            UpdatedAtUtc = baseRow.UpdatedAtUtc,
            ClosedAtUtc = baseRow.ClosedAtUtc,
            Details = r.Details,
            ActionLogs = r.ActionLogs
                .OrderBy(a => a.CreatedAtUtc)
                .ThenBy(a => a.ConcernActionLogID)
                .Select(a => new ConcernActionLogReadDto
                {
                    ConcernActionLogID = a.ConcernActionLogID,
                    ActionKind = (int)a.ActionKind,
                    OldStatus = a.OldStatus != null ? (int)a.OldStatus : null,
                    NewStatus = a.NewStatus != null ? (int)a.NewStatus : null,
                    Comment = a.Comment,
                    ActorEmployeeProfileID = a.ActorEmployeeProfileID,
                    ActorName = a.ActorEmployeeProfile != null ? FormatPersonName(a.ActorEmployeeProfile.FullName) : null,
                    CreatedAtUtc = a.CreatedAtUtc,
                })
                .ToList(),
        };
    }

    public async Task<int> CreateSuggestionAsync(SuggestionWriteDto dto, int? actorEmployeeProfileId, CancellationToken cancellationToken = default)
    {
        await EnsureDefaultCategoriesAsync(dto.SchoolID, cancellationToken);
        await ValidateSuggestionCategoryAsync(dto.ConcernCategoryID, dto.SchoolID, cancellationToken);

        var subOk = await _db.EmployeeProfiles.AsNoTracking()
            .AnyAsync(e => e.EmployeeProfileID == dto.SubmitterEmployeeProfileID && e.SchoolID == dto.SchoolID, cancellationToken);
        if (!subOk)
            throw new InvalidOperationException("Submitter employee profile was not found for this school.");

        if (dto.AssignedToEmployeeProfileID is > 0)
        {
            var aOk = await _db.EmployeeProfiles.AsNoTracking()
                .AnyAsync(e => e.EmployeeProfileID == dto.AssignedToEmployeeProfileID && e.SchoolID == dto.SchoolID, cancellationToken);
            if (!aOk)
                throw new InvalidOperationException("Assignee employee profile was not found for this school.");
        }

        var yearId = dto.AcademicYearID is > 0
            ? dto.AcademicYearID!.Value
            : await GetActiveYearIdForSchoolAsync(dto.SchoolID, cancellationToken)
              ?? throw new InvalidOperationException("No academic year is configured for this school.");

        var yearOk = await _db.Years.AsNoTracking()
            .AnyAsync(y => y.YearID == yearId && y.SchoolID == dto.SchoolID, cancellationToken);
        if (!yearOk)
            throw new InvalidOperationException("Academic year does not belong to this school.");

        var now = DateTime.UtcNow;
        var status = (ConcernStatus)dto.Status;
        var entity = new Suggestion
        {
            SchoolID = dto.SchoolID,
            AcademicYearID = yearId,
            ConcernCategoryID = dto.ConcernCategoryID,
            SubmitterEmployeeProfileID = dto.SubmitterEmployeeProfileID,
            AssignedToEmployeeProfileID = dto.AssignedToEmployeeProfileID is > 0 ? dto.AssignedToEmployeeProfileID : null,
            Title = dto.Title,
            Details = dto.Details,
            Status = status,
            SubmittedAtUtc = now,
            UpdatedAtUtc = now,
            ClosedAtUtc = status is ConcernStatus.Resolved or ConcernStatus.Rejected or ConcernStatus.Closed or ConcernStatus.Cancelled
                ? now
                : null,
        };

        _db.Suggestions.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        _db.ConcernActionLogs.Add(new ConcernActionLog
        {
            ComplaintID = null,
            SuggestionID = entity.SuggestionID,
            ActorEmployeeProfileID = actorEmployeeProfileId,
            ActionKind = ConcernActionKind.Created,
            OldStatus = null,
            NewStatus = status,
            Comment = null,
            CreatedAtUtc = now,
        });
        await _db.SaveChangesAsync(cancellationToken);

        return entity.SuggestionID;
    }

    public async Task UpdateSuggestionAsync(int id, SuggestionWriteDto dto, int? actorEmployeeProfileId, CancellationToken cancellationToken = default)
    {
        var r = await _db.Suggestions.FirstOrDefaultAsync(x => x.SuggestionID == id, cancellationToken)
            ?? throw new InvalidOperationException("Suggestion was not found.");

        if (r.SchoolID != dto.SchoolID)
            throw new InvalidOperationException("School mismatch for this suggestion.");

        await ValidateSuggestionCategoryAsync(dto.ConcernCategoryID, dto.SchoolID, cancellationToken);

        var subOk = await _db.EmployeeProfiles.AsNoTracking()
            .AnyAsync(e => e.EmployeeProfileID == dto.SubmitterEmployeeProfileID && e.SchoolID == dto.SchoolID, cancellationToken);
        if (!subOk)
            throw new InvalidOperationException("Submitter employee profile was not found for this school.");

        if (dto.AssignedToEmployeeProfileID is > 0)
        {
            var aOk = await _db.EmployeeProfiles.AsNoTracking()
                .AnyAsync(e => e.EmployeeProfileID == dto.AssignedToEmployeeProfileID && e.SchoolID == dto.SchoolID, cancellationToken);
            if (!aOk)
                throw new InvalidOperationException("Assignee employee profile was not found for this school.");
        }

        var yearId = dto.AcademicYearID is > 0 ? dto.AcademicYearID!.Value : r.AcademicYearID;
        var yearOk = await _db.Years.AsNoTracking()
            .AnyAsync(y => y.YearID == yearId && y.SchoolID == dto.SchoolID, cancellationToken);
        if (!yearOk)
            throw new InvalidOperationException("Academic year does not belong to this school.");

        var now = DateTime.UtcNow;
        var oldStatus = r.Status;
        var newStatus = (ConcernStatus)dto.Status;

        if (oldStatus != newStatus)
        {
            _db.ConcernActionLogs.Add(new ConcernActionLog
            {
                ComplaintID = null,
                SuggestionID = r.SuggestionID,
                ActorEmployeeProfileID = actorEmployeeProfileId,
                ActionKind = ConcernActionKind.StatusChanged,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                Comment = null,
                CreatedAtUtc = now,
            });
        }

        r.AcademicYearID = yearId;
        r.ConcernCategoryID = dto.ConcernCategoryID;
        r.SubmitterEmployeeProfileID = dto.SubmitterEmployeeProfileID;
        r.AssignedToEmployeeProfileID = dto.AssignedToEmployeeProfileID is > 0 ? dto.AssignedToEmployeeProfileID : null;
        r.Title = dto.Title;
        r.Details = dto.Details;
        r.Status = newStatus;
        r.UpdatedAtUtc = now;

        if (newStatus is ConcernStatus.Resolved or ConcernStatus.Rejected or ConcernStatus.Closed or ConcernStatus.Cancelled)
            r.ClosedAtUtc ??= now;
        else
            r.ClosedAtUtc = null;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<int?> GetSchoolIdForSuggestionAsync(int suggestionId, CancellationToken cancellationToken = default)
    {
        return _db.Suggestions.AsNoTracking()
            .Where(x => x.SuggestionID == suggestionId)
            .Select(x => (int?)x.SchoolID)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
