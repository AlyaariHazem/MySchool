using System.Text.Json;
using Backend.Common;
using Backend.Data;
using Backend.DTOS.School.DailyEvaluation;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class DailyEvaluationService : IDailyEvaluationService
{
    private readonly TenantDbContext _db;
    private readonly IAuditTrailService _audit;

    public DailyEvaluationService(TenantDbContext db, IAuditTrailService audit)
    {
        _db = db;
        _audit = audit;
    }

    private static void ValidateCriteriaScores(decimal min, decimal max)
    {
        if (max < min)
            throw new InvalidOperationException("MaxScore must be greater than or equal to MinScore.");
    }

    private async Task EnsureYearSchoolAsync(int schoolId, int yearId, CancellationToken ct)
    {
        var ok = await _db.Years.AsNoTracking().AnyAsync(y => y.YearID == yearId && y.SchoolID == schoolId, ct);
        if (!ok)
            throw new InvalidOperationException($"Academic year {yearId} is not valid for school {schoolId}.");
    }

    /// <summary>Active year for the school (<see cref="Year.Active"/>), else latest year id for that school.</summary>
    private async Task<int?> GetActiveYearIdForSchoolAsync(int schoolId, CancellationToken ct)
    {
        var yid = await _db.Years.AsNoTracking()
            .Where(y => y.SchoolID == schoolId && y.Active)
            .OrderBy(y => y.YearID)
            .Select(y => (int?)y.YearID)
            .FirstOrDefaultAsync(ct);
        if (yid is > 0)
            return yid;
        return await _db.Years.AsNoTracking()
            .Where(y => y.SchoolID == schoolId)
            .OrderByDescending(y => y.YearID)
            .Select(y => (int?)y.YearID)
            .FirstOrDefaultAsync(ct);
    }

    private async Task<bool> IsDateLockedAsync(int schoolId, int yearId, DateOnly date, int? templateId, CancellationToken ct)
    {
        var locks = await _db.EvaluationLocks.AsNoTracking()
            .Where(l => l.SchoolID == schoolId && l.AcademicYearID == yearId && l.LockDate == date
                && l.Status == EvaluationLockStatus.Locked && l.IsActive)
            .ToListAsync(ct);
        if (locks.Count == 0)
            return false;
        if (locks.Any(l => l.DailyEvaluationTemplateID == null))
            return true;
        return templateId.HasValue && locks.Any(l => l.DailyEvaluationTemplateID == templateId);
    }

    private async Task<bool> IsEvaluationLockedAsync(DailyEvaluation e, CancellationToken ct)
    {
        if (e.IsLocked || e.Status == DailyEvaluationStatus.Locked)
            return true;
        return await IsDateLockedAsync(e.SchoolID, e.AcademicYearID, e.EvaluationDate, e.DailyEvaluationTemplateID, ct);
    }

    private static decimal ComputeTotalScore(IEnumerable<DailyEvaluationItem> items) =>
        items.Sum(i => i.Score);

    #region Templates

    public async Task<DailyEvaluationTemplateReadDto> CreateTemplateAsync(DailyEvaluationTemplateCreateDto dto, CancellationToken cancellationToken = default)
    {
        await EnsureYearSchoolAsync(dto.SchoolID, dto.AcademicYearID, cancellationToken);
        if (dto.EmployeeJobTypeID is int jt && jt > 0)
            _ = await _db.EmployeeJobTypes.AsNoTracking().FirstOrDefaultAsync(x => x.EmployeeJobTypeID == jt, cancellationToken)
                ?? throw new KeyNotFoundException($"Job type {jt} not found.");

        var entity = new DailyEvaluationTemplate
        {
            SchoolID = dto.SchoolID,
            AcademicYearID = dto.AcademicYearID,
            EmployeeJobTypeID = dto.EmployeeJobTypeID,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Status = EvaluationTemplateStatus.Draft,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo,
            IsDefault = dto.IsDefault,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _db.DailyEvaluationTemplates.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetTemplateByIdAsync(entity.DailyEvaluationTemplateID, cancellationToken))!;
    }

    public async Task<DailyEvaluationTemplateReadDto> UpdateTemplateAsync(int id, DailyEvaluationTemplateUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var e = await _db.DailyEvaluationTemplates.FirstOrDefaultAsync(t => t.DailyEvaluationTemplateID == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Template {id} not found.");
        if (e.Status == EvaluationTemplateStatus.Archived)
            throw new InvalidOperationException("Archived templates cannot be updated.");
        e.Name = dto.Name.Trim();
        e.Description = dto.Description?.Trim();
        e.EffectiveFrom = dto.EffectiveFrom;
        e.EffectiveTo = dto.EffectiveTo;
        e.IsDefault = dto.IsDefault;
        e.IsActive = dto.IsActive;
        e.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetTemplateByIdAsync(id, cancellationToken))!;
    }

    public async Task<DailyEvaluationTemplateReadDto?> GetTemplateByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var e = await _db.DailyEvaluationTemplates.AsNoTracking()
            .FirstOrDefaultAsync(t => t.DailyEvaluationTemplateID == id, cancellationToken);
        return e == null ? null : MapTemplateRead(e);
    }

    private const int MaxTemplatesPageSize = 500;

    private async Task<DailyEvaluationTemplateFilterDto> ResolveTemplateFilterAsync(
        DailyEvaluationTemplateFilterDto? filter,
        CancellationToken cancellationToken)
    {
        filter ??= new DailyEvaluationTemplateFilterDto();
        if (filter.SchoolID is int schoolForYear && filter.AcademicYearID is null)
        {
            var ay = await GetActiveYearIdForSchoolAsync(schoolForYear, cancellationToken);
            if (ay is int yDef && yDef > 0)
                filter.AcademicYearID = yDef;
        }

        return filter;
    }

    private static IQueryable<DailyEvaluationTemplate> ApplyTemplateFilters(
        IQueryable<DailyEvaluationTemplate> query,
        DailyEvaluationTemplateFilterDto filter)
    {
        if (filter.SchoolID is int sid) query = query.Where(t => t.SchoolID == sid);
        if (filter.AcademicYearID is int y) query = query.Where(t => t.AcademicYearID == y);
        if (filter.EmployeeJobTypeID is int j) query = query.Where(t => t.EmployeeJobTypeID == j);
        if (filter.Status is EvaluationTemplateStatus st) query = query.Where(t => t.Status == st);
        if (filter.IsActive is bool ia) query = query.Where(t => t.IsActive == ia);
        return query;
    }

    public async Task<PagedResult<DailyEvaluationTemplateListDto>> GetTemplatesPageAsync(
        DailyEvaluationTemplatesPageRequestDto request,
        CancellationToken cancellationToken = default)
    {
        request ??= new DailyEvaluationTemplatesPageRequestDto();
        var filter = await ResolveTemplateFilterAsync(request.Filter, cancellationToken);

        var pageIndex = Math.Max(0, request.PageIndex);
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;
        if (pageSize > MaxTemplatesPageSize) pageSize = MaxTemplatesPageSize;

        var baseQuery = ApplyTemplateFilters(_db.DailyEvaluationTemplates.AsNoTracking(), filter);
        var ordered = baseQuery.OrderByDescending(t => t.UpdatedAtUtc);

        var totalCount = await ordered.CountAsync(cancellationToken);

        var items = await ordered
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(t => new DailyEvaluationTemplateListDto
            {
                DailyEvaluationTemplateID = t.DailyEvaluationTemplateID,
                SchoolID = t.SchoolID,
                AcademicYearID = t.AcademicYearID,
                Name = t.Name,
                Status = t.Status,
                IsActive = t.IsActive,
                UpdatedAtUtc = t.UpdatedAtUtc
            })
            .ToListAsync(cancellationToken);

        var totalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedResult<DailyEvaluationTemplateListDto>(
            items,
            pageIndex + 1,
            pageSize,
            totalCount,
            totalPages);
    }

    public async Task<DailyEvaluationTemplateReadDto> ActivateTemplateAsync(int id, CancellationToken cancellationToken = default)
    {
        var e = await _db.DailyEvaluationTemplates.FirstOrDefaultAsync(t => t.DailyEvaluationTemplateID == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Template {id} not found.");
        e.Status = EvaluationTemplateStatus.Active;
        e.IsActive = true;
        e.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetTemplateByIdAsync(id, cancellationToken))!;
    }

    public async Task<DailyEvaluationTemplateReadDto> DeactivateTemplateAsync(int id, CancellationToken cancellationToken = default)
    {
        var e = await _db.DailyEvaluationTemplates.FirstOrDefaultAsync(t => t.DailyEvaluationTemplateID == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Template {id} not found.");
        e.Status = EvaluationTemplateStatus.Inactive;
        e.IsActive = false;
        e.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetTemplateByIdAsync(id, cancellationToken))!;
    }

    public async Task<DailyEvaluationTemplateReadDto> ArchiveTemplateAsync(int id, CancellationToken cancellationToken = default)
    {
        var e = await _db.DailyEvaluationTemplates.FirstOrDefaultAsync(t => t.DailyEvaluationTemplateID == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Template {id} not found.");
        e.Status = EvaluationTemplateStatus.Archived;
        e.IsActive = false;
        e.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetTemplateByIdAsync(id, cancellationToken))!;
    }

    private static DailyEvaluationTemplateReadDto MapTemplateRead(DailyEvaluationTemplate e) => new()
    {
        DailyEvaluationTemplateID = e.DailyEvaluationTemplateID,
        SchoolID = e.SchoolID,
        AcademicYearID = e.AcademicYearID,
        EmployeeJobTypeID = e.EmployeeJobTypeID,
        Name = e.Name,
        Description = e.Description,
        Status = e.Status,
        EffectiveFrom = e.EffectiveFrom,
        EffectiveTo = e.EffectiveTo,
        IsDefault = e.IsDefault,
        IsActive = e.IsActive,
        CreatedAtUtc = e.CreatedAtUtc,
        UpdatedAtUtc = e.UpdatedAtUtc
    };

    #endregion

    #region Criteria

    public async Task<DailyEvaluationCriteriaReadDto> AddCriteriaAsync(int templateId, DailyEvaluationCriteriaCreateDto dto, CancellationToken cancellationToken = default)
    {
        var t = await _db.DailyEvaluationTemplates.FirstOrDefaultAsync(x => x.DailyEvaluationTemplateID == templateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Template {templateId} not found.");
        ValidateCriteriaScores(dto.MinScore, dto.MaxScore);
        var c = new DailyEvaluationCriteria
        {
            DailyEvaluationTemplateID = templateId,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Weight = dto.Weight,
            MaxScore = dto.MaxScore,
            MinScore = dto.MinScore,
            IsMandatory = dto.IsMandatory,
            SortOrder = dto.SortOrder,
            IsActive = true,
            Notes = dto.Notes?.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _db.DailyEvaluationCriteria.Add(c);
        t.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return MapCriteriaRead(await _db.DailyEvaluationCriteria.AsNoTracking()
            .FirstAsync(x => x.DailyEvaluationCriteriaID == c.DailyEvaluationCriteriaID, cancellationToken));
    }

    public async Task<DailyEvaluationCriteriaReadDto> UpdateCriteriaAsync(int criteriaId, DailyEvaluationCriteriaUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var c = await _db.DailyEvaluationCriteria
            .Include(x => x.Template)
            .FirstOrDefaultAsync(x => x.DailyEvaluationCriteriaID == criteriaId, cancellationToken)
            ?? throw new KeyNotFoundException($"Criteria {criteriaId} not found.");
        ValidateCriteriaScores(dto.MinScore, dto.MaxScore);
        c.Name = dto.Name.Trim();
        c.Description = dto.Description?.Trim();
        c.Weight = dto.Weight;
        c.MaxScore = dto.MaxScore;
        c.MinScore = dto.MinScore;
        c.IsMandatory = dto.IsMandatory;
        c.SortOrder = dto.SortOrder;
        c.IsActive = dto.IsActive;
        c.Notes = dto.Notes?.Trim();
        c.UpdatedAtUtc = DateTime.UtcNow;
        c.Template.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return MapCriteriaRead(c);
    }

    public async Task<IReadOnlyList<DailyEvaluationCriteriaReadDto>> GetCriteriaForTemplateAsync(int templateId, CancellationToken cancellationToken = default)
    {
        _ = await _db.DailyEvaluationTemplates.AsNoTracking()
            .FirstOrDefaultAsync(t => t.DailyEvaluationTemplateID == templateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Template {templateId} not found.");
        var list = await _db.DailyEvaluationCriteria.AsNoTracking()
            .Where(c => c.DailyEvaluationTemplateID == templateId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);
        return list.Select(MapCriteriaRead).ToList();
    }

    private static DailyEvaluationCriteriaReadDto MapCriteriaRead(DailyEvaluationCriteria c) => new()
    {
        DailyEvaluationCriteriaID = c.DailyEvaluationCriteriaID,
        DailyEvaluationTemplateID = c.DailyEvaluationTemplateID,
        Name = c.Name,
        Description = c.Description,
        Weight = c.Weight,
        MaxScore = c.MaxScore,
        MinScore = c.MinScore,
        IsMandatory = c.IsMandatory,
        SortOrder = c.SortOrder,
        IsActive = c.IsActive,
        Notes = c.Notes,
        UpdatedAtUtc = c.UpdatedAtUtc
    };

    #endregion

    #region Evaluations

    public async Task<DailyEvaluationReadDto> CreateEvaluationAsync(DailyEvaluationCreateDto dto, CancellationToken cancellationToken = default)
    {
        await EnsureYearSchoolAsync(dto.SchoolID, dto.AcademicYearID, cancellationToken);
        var template = await _db.DailyEvaluationTemplates
            .Include(t => t.Criteria)
            .FirstOrDefaultAsync(t => t.DailyEvaluationTemplateID == dto.DailyEvaluationTemplateID, cancellationToken)
            ?? throw new KeyNotFoundException($"Template {dto.DailyEvaluationTemplateID} not found.");
        if (template.SchoolID != dto.SchoolID || template.AcademicYearID != dto.AcademicYearID)
            throw new InvalidOperationException("Template does not belong to the given school/year.");
        if (template.Status != EvaluationTemplateStatus.Active || !template.IsActive)
            throw new InvalidOperationException("Only active templates can be used for new evaluations.");

        var emp = await _db.EmployeeProfiles.AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployeeProfileID == dto.EvaluatedEmployeeProfileID, cancellationToken)
            ?? throw new KeyNotFoundException($"Employee profile {dto.EvaluatedEmployeeProfileID} not found.");
        if (emp.SchoolID != dto.SchoolID)
            throw new InvalidOperationException("Employee does not belong to the school.");

        var dup = await _db.DailyEvaluations.AsNoTracking()
            .AnyAsync(e => e.EvaluatedEmployeeProfileID == dto.EvaluatedEmployeeProfileID
                && e.EvaluationDate == dto.EvaluationDate && e.DailyEvaluationTemplateID == dto.DailyEvaluationTemplateID, cancellationToken);
        if (dup)
            throw new InvalidOperationException("An evaluation already exists for this employee, date, and template.");

        if (await IsDateLockedAsync(dto.SchoolID, dto.AcademicYearID, dto.EvaluationDate, dto.DailyEvaluationTemplateID, cancellationToken))
            throw new InvalidOperationException("This date is locked for evaluations.");

        var eval = new DailyEvaluation
        {
            SchoolID = dto.SchoolID,
            AcademicYearID = dto.AcademicYearID,
            EvaluatedEmployeeProfileID = dto.EvaluatedEmployeeProfileID,
            EvaluatorUserId = dto.EvaluatorUserId,
            EvaluatorEmployeeProfileID = dto.EvaluatorEmployeeProfileID,
            DailyEvaluationTemplateID = dto.DailyEvaluationTemplateID,
            EvaluationDate = dto.EvaluationDate,
            Status = DailyEvaluationStatus.Draft,
            Notes = dto.Notes?.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _db.DailyEvaluations.Add(eval);

        var seeded = new List<DailyEvaluationItem>();
        foreach (var crit in template.Criteria.Where(c => c.IsActive).OrderBy(c => c.SortOrder))
        {
            var it = new DailyEvaluationItem
            {
                DailyEvaluation = eval,
                DailyEvaluationCriteriaID = crit.DailyEvaluationCriteriaID,
                Score = crit.MinScore,
                IsMandatorySatisfied = !crit.IsMandatory,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };
            seeded.Add(it);
            _db.DailyEvaluationItems.Add(it);
        }
        eval.TotalScore = ComputeTotalScore(seeded);
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetEvaluationByIdAsync(eval.DailyEvaluationID, cancellationToken))!;
    }

    public async Task<DailyEvaluationReadDto?> GetEvaluationByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var e = await _db.DailyEvaluations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.DailyEvaluationID == id, cancellationToken);
        return e == null ? null : MapEvalRead(e);
    }

    public async Task<DailyEvaluationFullDto> GetEvaluationFullAsync(int id, CancellationToken cancellationToken = default)
    {
        var e = await _db.DailyEvaluations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.DailyEvaluationID == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Evaluation {id} not found.");
        var items = await _db.DailyEvaluationItems.AsNoTracking()
            .Where(i => i.DailyEvaluationID == id)
            .Join(_db.DailyEvaluationCriteria.AsNoTracking(),
                i => i.DailyEvaluationCriteriaID,
                c => c.DailyEvaluationCriteriaID,
                (i, c) => new DailyEvaluationItemReadDto
                {
                    DailyEvaluationItemID = i.DailyEvaluationItemID,
                    DailyEvaluationID = i.DailyEvaluationID,
                    DailyEvaluationCriteriaID = i.DailyEvaluationCriteriaID,
                    CriteriaName = c.Name,
                    Score = i.Score,
                    Comment = i.Comment,
                    IsMandatorySatisfied = i.IsMandatorySatisfied
                })
            .OrderBy(i => i.DailyEvaluationCriteriaID)
            .ToListAsync(cancellationToken);
        var read = MapEvalRead(e);
        return new DailyEvaluationFullDto
        {
            DailyEvaluationID = read.DailyEvaluationID,
            SchoolID = read.SchoolID,
            AcademicYearID = read.AcademicYearID,
            EvaluatedEmployeeProfileID = read.EvaluatedEmployeeProfileID,
            EvaluatorUserId = read.EvaluatorUserId,
            EvaluatorEmployeeProfileID = read.EvaluatorEmployeeProfileID,
            DailyEvaluationTemplateID = read.DailyEvaluationTemplateID,
            EvaluationDate = read.EvaluationDate,
            Status = read.Status,
            TotalScore = read.TotalScore,
            Notes = read.Notes,
            SubmittedAtUtc = read.SubmittedAtUtc,
            LockedAtUtc = read.LockedAtUtc,
            IsLocked = read.IsLocked,
            UpdatedAtUtc = read.UpdatedAtUtc,
            Items = items
        };
    }

    private const int MaxEvaluationsPageSize = 500;

    private async Task<DailyEvaluationFilterDto> ResolveEvaluationFilterAsync(
        DailyEvaluationFilterDto? filter,
        CancellationToken cancellationToken)
    {
        filter ??= new DailyEvaluationFilterDto();
        if (filter.SchoolID is int schoolForYear && filter.AcademicYearID is null)
        {
            var ay = await GetActiveYearIdForSchoolAsync(schoolForYear, cancellationToken);
            if (ay is int yDef && yDef > 0)
                filter.AcademicYearID = yDef;
        }

        return filter;
    }

    private static IQueryable<DailyEvaluation> ApplyEvaluationFilters(
        IQueryable<DailyEvaluation> query,
        DailyEvaluationFilterDto filter)
    {
        if (filter.SchoolID is int s) query = query.Where(e => e.SchoolID == s);
        if (filter.AcademicYearID is int y) query = query.Where(e => e.AcademicYearID == y);
        if (filter.EvaluatedEmployeeProfileID is int eid) query = query.Where(e => e.EvaluatedEmployeeProfileID == eid);
        if (filter.DailyEvaluationTemplateID is int tid) query = query.Where(e => e.DailyEvaluationTemplateID == tid);
        if (filter.FromDate is DateOnly fd) query = query.Where(e => e.EvaluationDate >= fd);
        if (filter.ToDate is DateOnly td) query = query.Where(e => e.EvaluationDate <= td);
        if (filter.Status is DailyEvaluationStatus st) query = query.Where(e => e.Status == st);
        if (!string.IsNullOrWhiteSpace(filter.EvaluatorUserId))
            query = query.Where(e => e.EvaluatorUserId == filter.EvaluatorUserId);
        return query;
    }

    public async Task<PagedResult<DailyEvaluationListDto>> GetEvaluationsPageAsync(
        DailyEvaluationsPageRequestDto request,
        CancellationToken cancellationToken = default)
    {
        request ??= new DailyEvaluationsPageRequestDto();
        var filter = await ResolveEvaluationFilterAsync(request.Filter, cancellationToken);

        var pageIndex = Math.Max(0, request.PageIndex);
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;
        if (pageSize > MaxEvaluationsPageSize) pageSize = MaxEvaluationsPageSize;

        var baseQuery = ApplyEvaluationFilters(_db.DailyEvaluations.AsNoTracking(), filter);
        var ordered = baseQuery.OrderByDescending(e => e.EvaluationDate).ThenByDescending(e => e.DailyEvaluationID);

        var totalCount = await ordered.CountAsync(cancellationToken);

        var items = await ordered
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(e => new DailyEvaluationListDto
            {
                DailyEvaluationID = e.DailyEvaluationID,
                EvaluatedEmployeeProfileID = e.EvaluatedEmployeeProfileID,
                DailyEvaluationTemplateID = e.DailyEvaluationTemplateID,
                EvaluationDate = e.EvaluationDate,
                Status = e.Status,
                TotalScore = e.TotalScore,
                IsLocked = e.IsLocked,
                UpdatedAtUtc = e.UpdatedAtUtc
            })
            .ToListAsync(cancellationToken);

        var totalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedResult<DailyEvaluationListDto>(
            items,
            pageIndex + 1,
            pageSize,
            totalCount,
            totalPages);
    }

    public async Task<IReadOnlyList<TeacherEvaluationOptionDto>> GetTeachersForStudentEvaluationAsync(
        int schoolId,
        string? studentUserId,
        CancellationToken cancellationToken = default)
    {
        async Task<IReadOnlyList<TeacherEvaluationOptionDto>> AllSchoolTeachersAsync()
        {
            var rows = await _db.EmployeeProfiles.AsNoTracking()
                .Where(ep => ep.SchoolID == schoolId && ep.TeacherID != null && ep.TeacherID > 0 && ep.IsActive)
                .OrderBy(ep => ep.EmployeeCode)
                .Select(ep => new { ep.EmployeeProfileID, ep.FullName, ep.EmployeeCode })
                .ToListAsync(cancellationToken);
            return rows.Select(r => new TeacherEvaluationOptionDto
            {
                EmployeeProfileID = r.EmployeeProfileID,
                DisplayName = FormatPersonName(r.FullName) is { Length: > 0 } n ? n : r.EmployeeCode,
            }).ToList();
        }

        // Non-student callers (e.g. school admin) — full teaching staff list for the school.
        if (string.IsNullOrWhiteSpace(studentUserId))
            return await AllSchoolTeachersAsync();

        studentUserId = studentUserId.Trim();

        var studentInfo = await _db.Students.AsNoTracking()
            .Where(s => s.UserID != null && s.UserID == studentUserId)
            .Select(s => new
            {
                s.DivisionID,
                ClassTeacherId = s.Division.Class.TeacherID,
                StudentSchoolId = s.Division.Class.Stage.Year.SchoolID,
            })
            .FirstOrDefaultAsync(cancellationToken);

        // If the login user is not linked to a Student row (UserID not set), still return school teachers so the UI works.
        if (studentInfo == null)
            return await AllSchoolTeachersAsync();

        if (studentInfo.StudentSchoolId != schoolId)
            throw new InvalidOperationException("School does not match the student's enrollment.");

        var teacherIds = new HashSet<int>();
        if (studentInfo.ClassTeacherId is int homeroomId && homeroomId > 0)
            teacherIds.Add(homeroomId);

        var activeYearId = await GetActiveYearIdForSchoolAsync(schoolId, cancellationToken);
        var cpBase = _db.CoursePlans.AsNoTracking()
            .Where(cp => cp.DivisionID == studentInfo.DivisionID);
        List<int> cpTeacherIds;
        if (activeYearId is int yearFilter && yearFilter > 0)
        {
            cpTeacherIds = await cpBase.Where(cp => cp.YearID == yearFilter)
                .Select(cp => cp.TeacherID).Distinct().ToListAsync(cancellationToken);
            // Course plans often keyed by a different year than "Active" — use division teachers from any year if none for active year.
            if (cpTeacherIds.Count == 0)
            {
                cpTeacherIds = await cpBase.Select(cp => cp.TeacherID).Distinct().ToListAsync(cancellationToken);
            }
        }
        else
        {
            cpTeacherIds = await cpBase.Select(cp => cp.TeacherID).Distinct().ToListAsync(cancellationToken);
        }

        foreach (var tid in cpTeacherIds)
        {
            if (tid > 0)
                teacherIds.Add(tid);
        }

        if (teacherIds.Count == 0)
            return await AllSchoolTeachersAsync();

        var profiles = await _db.EmployeeProfiles.AsNoTracking()
            .Where(ep => ep.SchoolID == schoolId && ep.TeacherID != null && teacherIds.Contains(ep.TeacherID!.Value) && ep.IsActive)
            .OrderBy(ep => ep.EmployeeCode)
            .Select(ep => new { ep.EmployeeProfileID, ep.FullName, ep.EmployeeCode })
            .ToListAsync(cancellationToken);

        if (profiles.Count == 0)
            return await AllSchoolTeachersAsync();

        return profiles.Select(r => new TeacherEvaluationOptionDto
        {
            EmployeeProfileID = r.EmployeeProfileID,
            DisplayName = FormatPersonName(r.FullName) is { Length: > 0 } n ? n : r.EmployeeCode,
        }).ToList();
    }

    public async Task<string?> ValidateStudentEvaluationCreateAsync(DailyEvaluationCreateDto body, string studentUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentUserId))
            return "Student user id is required.";

        var studentSchoolId = await _db.Students.AsNoTracking()
            .Where(s => s.UserID == studentUserId)
            .Select(s => (int?)s.Division.Class.Stage.Year.SchoolID)
            .FirstOrDefaultAsync(cancellationToken);
        if (!studentSchoolId.HasValue)
            return "Student record was not found or is not linked to a class.";

        if (studentSchoolId.Value != body.SchoolID)
            return "School does not match the student's enrollment.";

        var ep = await _db.EmployeeProfiles.AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployeeProfileID == body.EvaluatedEmployeeProfileID, cancellationToken);
        if (ep == null)
            return "Evaluated employee was not found.";
        if (ep.SchoolID != body.SchoolID)
            return "The selected person does not belong to this school.";
        if (!ep.TeacherID.HasValue || ep.TeacherID.Value <= 0)
            return "Daily evaluations by students may only target teaching staff.";

        return null;
    }

    private static string FormatPersonName(Name? name)
    {
        if (name == null) return string.Empty;
        return string.Join(" ", new[] { name.FirstName, name.MiddleName, name.LastName }.Where(static x => !string.IsNullOrWhiteSpace(x)));
    }

    public async Task<DailyEvaluationReadDto> UpdateEvaluationAsync(int id, DailyEvaluationUpdateDto dto, string? currentUserId, CancellationToken cancellationToken = default)
    {
        var e = await _db.DailyEvaluations
            .FirstOrDefaultAsync(x => x.DailyEvaluationID == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Evaluation {id} not found.");
        if (await IsEvaluationLockedAsync(e, cancellationToken))
            throw new InvalidOperationException("Evaluation is locked. Use POST override-update with an authorized account.");

        e.Notes = dto.Notes?.Trim();
        e.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetEvaluationByIdAsync(id, cancellationToken))!;
    }

    public async Task<DailyEvaluationReadDto> SubmitEvaluationAsync(int id, CancellationToken cancellationToken = default)
    {
        var e = await _db.DailyEvaluations
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.DailyEvaluationID == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Evaluation {id} not found.");
        if (await IsEvaluationLockedAsync(e, cancellationToken))
            throw new InvalidOperationException("Evaluation is locked.");
        var criteria = await _db.DailyEvaluationCriteria.AsNoTracking()
            .Where(c => c.DailyEvaluationTemplateID == e.DailyEvaluationTemplateID && c.IsActive)
            .ToListAsync(cancellationToken);
        var mandatory = criteria.Where(c => c.IsMandatory).ToList();
        foreach (var m in mandatory)
        {
            var item = e.Items.FirstOrDefault(i => i.DailyEvaluationCriteriaID == m.DailyEvaluationCriteriaID);
            if (item == null)
                throw new InvalidOperationException($"Mandatory criterion '{m.Name}' is missing.");
            if (item.Score < m.MinScore || item.Score > m.MaxScore)
                throw new InvalidOperationException($"Score for '{m.Name}' is out of range.");
        }
        e.Status = DailyEvaluationStatus.Submitted;
        e.SubmittedAtUtc = DateTime.UtcNow;
        e.TotalScore = ComputeTotalScore(e.Items);
        e.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetEvaluationByIdAsync(id, cancellationToken))!;
    }

    public async Task<DailyEvaluationItemReadDto> UpsertItemAsync(int evaluationId, DailyEvaluationItemCreateDto dto, CancellationToken cancellationToken = default)
    {
        var e = await _db.DailyEvaluations
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.DailyEvaluationID == evaluationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Evaluation {evaluationId} not found.");
        if (await IsEvaluationLockedAsync(e, cancellationToken))
            throw new InvalidOperationException("Evaluation is locked.");

        var crit = await _db.DailyEvaluationCriteria.AsNoTracking()
            .FirstOrDefaultAsync(c => c.DailyEvaluationCriteriaID == dto.DailyEvaluationCriteriaID
                && c.DailyEvaluationTemplateID == e.DailyEvaluationTemplateID && c.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("Criterion does not belong to this evaluation template.");

        if (dto.Score < crit.MinScore || dto.Score > crit.MaxScore)
            throw new InvalidOperationException($"Score must be between {crit.MinScore} and {crit.MaxScore}.");

        var item = e.Items.FirstOrDefault(i => i.DailyEvaluationCriteriaID == dto.DailyEvaluationCriteriaID);
        if (item == null)
        {
            item = new DailyEvaluationItem
            {
                DailyEvaluationID = evaluationId,
                DailyEvaluationCriteriaID = dto.DailyEvaluationCriteriaID,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };
            _db.DailyEvaluationItems.Add(item);
            e.Items.Add(item);
        }
        item.Score = dto.Score;
        item.Comment = dto.Comment?.Trim();
        item.IsMandatorySatisfied = true;
        item.UpdatedAtUtc = DateTime.UtcNow;
        e.TotalScore = ComputeTotalScore(e.Items);
        e.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        var name = crit.Name;
        return new DailyEvaluationItemReadDto
        {
            DailyEvaluationItemID = item.DailyEvaluationItemID,
            DailyEvaluationID = evaluationId,
            DailyEvaluationCriteriaID = item.DailyEvaluationCriteriaID,
            CriteriaName = name,
            Score = item.Score,
            Comment = item.Comment,
            IsMandatorySatisfied = item.IsMandatorySatisfied
        };
    }

    public Task<int?> GetEvaluationIdForItemAsync(int dailyEvaluationItemId, CancellationToken cancellationToken = default) =>
        _db.DailyEvaluationItems.AsNoTracking()
            .Where(i => i.DailyEvaluationItemID == dailyEvaluationItemId)
            .Select(i => (int?)i.DailyEvaluationID)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<DailyEvaluationItemReadDto> UpdateItemAsync(int itemId, DailyEvaluationItemUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var item = await _db.DailyEvaluationItems
            .Include(i => i.DailyEvaluation)
            .FirstOrDefaultAsync(i => i.DailyEvaluationItemID == itemId, cancellationToken)
            ?? throw new KeyNotFoundException($"Item {itemId} not found.");
        var e = item.DailyEvaluation;
        if (await IsEvaluationLockedAsync(e, cancellationToken))
            throw new InvalidOperationException("Evaluation is locked.");

        var crit = await _db.DailyEvaluationCriteria.AsNoTracking()
            .FirstAsync(c => c.DailyEvaluationCriteriaID == item.DailyEvaluationCriteriaID, cancellationToken);
        if (dto.Score < crit.MinScore || dto.Score > crit.MaxScore)
            throw new InvalidOperationException($"Score must be between {crit.MinScore} and {crit.MaxScore}.");

        item.Score = dto.Score;
        item.Comment = dto.Comment?.Trim();
        item.UpdatedAtUtc = DateTime.UtcNow;
        var items = await _db.DailyEvaluationItems.Where(i => i.DailyEvaluationID == e.DailyEvaluationID).ToListAsync(cancellationToken);
        e.TotalScore = ComputeTotalScore(items);
        e.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return new DailyEvaluationItemReadDto
        {
            DailyEvaluationItemID = item.DailyEvaluationItemID,
            DailyEvaluationID = item.DailyEvaluationID,
            DailyEvaluationCriteriaID = item.DailyEvaluationCriteriaID,
            CriteriaName = crit.Name,
            Score = item.Score,
            Comment = item.Comment,
            IsMandatorySatisfied = item.IsMandatorySatisfied
        };
    }

    private static DailyEvaluationReadDto MapEvalRead(DailyEvaluation e) => new()
    {
        DailyEvaluationID = e.DailyEvaluationID,
        SchoolID = e.SchoolID,
        AcademicYearID = e.AcademicYearID,
        EvaluatedEmployeeProfileID = e.EvaluatedEmployeeProfileID,
        EvaluatorUserId = e.EvaluatorUserId,
        EvaluatorEmployeeProfileID = e.EvaluatorEmployeeProfileID,
        DailyEvaluationTemplateID = e.DailyEvaluationTemplateID,
        EvaluationDate = e.EvaluationDate,
        Status = e.Status,
        TotalScore = e.TotalScore,
        Notes = e.Notes,
        SubmittedAtUtc = e.SubmittedAtUtc,
        LockedAtUtc = e.LockedAtUtc,
        IsLocked = e.IsLocked,
        UpdatedAtUtc = e.UpdatedAtUtc
    };

    #endregion

    #region Locks

    public async Task<EvaluationLockReadDto> LockDayAsync(EvaluationLockCreateDto dto, string lockedByUserId, CancellationToken cancellationToken = default)
    {
        await EnsureYearSchoolAsync(dto.SchoolID, dto.AcademicYearID, cancellationToken);
        if (dto.DailyEvaluationTemplateID is int tid)
            _ = await _db.DailyEvaluationTemplates.AsNoTracking()
                .FirstOrDefaultAsync(t => t.DailyEvaluationTemplateID == tid && t.SchoolID == dto.SchoolID && t.AcademicYearID == dto.AcademicYearID, cancellationToken)
                ?? throw new InvalidOperationException("Template not found for scope.");

        var exists = await _db.EvaluationLocks.AnyAsync(l =>
            l.SchoolID == dto.SchoolID && l.AcademicYearID == dto.AcademicYearID && l.LockDate == dto.LockDate
            && l.DailyEvaluationTemplateID == dto.DailyEvaluationTemplateID && l.Status == EvaluationLockStatus.Locked && l.IsActive, cancellationToken);
        if (exists)
            throw new InvalidOperationException("This date is already locked for the given scope.");

        var lockEntity = new EvaluationLock
        {
            SchoolID = dto.SchoolID,
            AcademicYearID = dto.AcademicYearID,
            LockDate = dto.LockDate,
            DailyEvaluationTemplateID = dto.DailyEvaluationTemplateID,
            Status = EvaluationLockStatus.Locked,
            LockedAtUtc = DateTime.UtcNow,
            LockedByUserId = lockedByUserId,
            Notes = dto.Notes,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _db.EvaluationLocks.Add(lockEntity);
        await _db.SaveChangesAsync(cancellationToken);

        var evalQuery = _db.DailyEvaluations.Where(ev =>
            ev.SchoolID == dto.SchoolID && ev.AcademicYearID == dto.AcademicYearID && ev.EvaluationDate == dto.LockDate);
        if (dto.DailyEvaluationTemplateID is int t2)
            evalQuery = evalQuery.Where(ev => ev.DailyEvaluationTemplateID == t2);
        var evals = await evalQuery.ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;
        foreach (var ev in evals)
        {
            ev.IsLocked = true;
            ev.LockedAtUtc = now;
            ev.Status = DailyEvaluationStatus.Locked;
            ev.UpdatedAtUtc = now;
        }
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.RecordAsync("DailyEvaluation", "LockDay", new { dto.SchoolID, dto.AcademicYearID, dto.LockDate, lockEntity.EvaluationLockID }, cancellationToken);

        return MapLockRead(lockEntity);
    }

    public async Task<EvaluationLockReadDto?> GetLockByDateAsync(int schoolId, int academicYearId, DateOnly date, int? templateId, CancellationToken cancellationToken = default)
    {
        var q = _db.EvaluationLocks.AsNoTracking()
            .Where(l => l.SchoolID == schoolId && l.AcademicYearID == academicYearId && l.LockDate == date && l.IsActive);
        if (templateId is int t)
            q = q.Where(l => l.DailyEvaluationTemplateID == t);
        else
            q = q.Where(l => l.DailyEvaluationTemplateID == null);
        var l = await q.OrderByDescending(x => x.EvaluationLockID).FirstOrDefaultAsync(cancellationToken);
        return l == null ? null : MapLockRead(l);
    }

    public async Task<EvaluationLockReadDto> ReopenLockAsync(int lockId, EvaluationReopenDto dto, string reopenedByUserId, CancellationToken cancellationToken = default)
    {
        var l = await _db.EvaluationLocks.FirstOrDefaultAsync(x => x.EvaluationLockID == lockId, cancellationToken)
            ?? throw new KeyNotFoundException($"Lock {lockId} not found.");
        if (l.Status != EvaluationLockStatus.Locked)
            throw new InvalidOperationException("Only a locked scope can be reopened.");
        if (string.IsNullOrWhiteSpace(dto.Reason))
            throw new ArgumentException("Reason is required.");

        l.Status = EvaluationLockStatus.Reopened;
        l.ReopenedAtUtc = DateTime.UtcNow;
        l.ReopenedByUserId = reopenedByUserId;
        l.Notes = dto.Notes?.Trim();
        l.UpdatedAtUtc = DateTime.UtcNow;

        var evalQuery = _db.DailyEvaluations.Where(ev =>
            ev.SchoolID == l.SchoolID && ev.AcademicYearID == l.AcademicYearID && ev.EvaluationDate == l.LockDate);
        if (l.DailyEvaluationTemplateID is int tid)
            evalQuery = evalQuery.Where(ev => ev.DailyEvaluationTemplateID == tid);
        var evals = await evalQuery.ToListAsync(cancellationToken);
        foreach (var ev in evals)
        {
            ev.IsLocked = false;
            ev.LockedAtUtc = null;
            if (ev.Status == DailyEvaluationStatus.Locked)
                ev.Status = DailyEvaluationStatus.Submitted;
            ev.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        var log = new EvaluationOverrideLog
        {
            EvaluationLockID = l.EvaluationLockID,
            SchoolID = l.SchoolID,
            AcademicYearID = l.AcademicYearID,
            OverrideActionType = EvaluationOverrideActionType.UnlockDay,
            Reason = dto.Reason.Trim(),
            PerformedByUserId = reopenedByUserId,
            PerformedAtUtc = DateTime.UtcNow,
            Notes = dto.Notes,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.EvaluationOverrideLogs.Add(log);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.RecordAsync("DailyEvaluation", "ReopenLock", new { lockId, reopenedByUserId }, cancellationToken);

        return MapLockRead(l);
    }

    private static EvaluationLockReadDto MapLockRead(EvaluationLock l) => new()
    {
        EvaluationLockID = l.EvaluationLockID,
        SchoolID = l.SchoolID,
        AcademicYearID = l.AcademicYearID,
        LockDate = l.LockDate,
        DailyEvaluationTemplateID = l.DailyEvaluationTemplateID,
        Status = l.Status,
        LockedAtUtc = l.LockedAtUtc,
        LockedByUserId = l.LockedByUserId,
        ReopenedAtUtc = l.ReopenedAtUtc,
        ReopenedByUserId = l.ReopenedByUserId,
        Notes = l.Notes
    };

    #endregion

    #region Overrides

    public async Task<DailyEvaluationReadDto> OverrideUpdateAfterLockAsync(int evaluationId, EvaluationOverrideRequestDto dto, string performedByUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Reason))
            throw new ArgumentException("Reason is required for override.");

        var e = await _db.DailyEvaluations
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.DailyEvaluationID == evaluationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Evaluation {evaluationId} not found.");

        if (!await IsEvaluationLockedAsync(e, cancellationToken))
            throw new InvalidOperationException("Override applies only when the evaluation or calendar day is locked.");

        var prev = JsonSerializer.Serialize(new { e.Notes, Items = e.Items.Select(i => new { i.DailyEvaluationItemID, i.Score, i.Comment }) });

        if (dto.Evaluation is DailyEvaluationUpdateDto u)
            e.Notes = u.Notes?.Trim() ?? e.Notes;

        if (dto.Items != null)
        {
            foreach (var p in dto.Items)
            {
                var item = e.Items.FirstOrDefault(i => i.DailyEvaluationItemID == p.DailyEvaluationItemID)
                    ?? throw new InvalidOperationException($"Item {p.DailyEvaluationItemID} not part of this evaluation.");
                var crit = await _db.DailyEvaluationCriteria.AsNoTracking()
                    .FirstAsync(c => c.DailyEvaluationCriteriaID == item.DailyEvaluationCriteriaID, cancellationToken);
                if (p.Score < crit.MinScore || p.Score > crit.MaxScore)
                    throw new InvalidOperationException($"Score out of range for item {p.DailyEvaluationItemID}.");
                item.Score = p.Score;
                item.Comment = p.Comment?.Trim();
                item.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        e.TotalScore = ComputeTotalScore(e.Items);
        e.UpdatedAtUtc = DateTime.UtcNow;

        var newSnap = JsonSerializer.Serialize(new { e.Notes, Items = e.Items.Select(i => new { i.DailyEvaluationItemID, i.Score, i.Comment }) });

        _db.EvaluationOverrideLogs.Add(new EvaluationOverrideLog
        {
            DailyEvaluationID = evaluationId,
            SchoolID = e.SchoolID,
            AcademicYearID = e.AcademicYearID,
            OverrideActionType = EvaluationOverrideActionType.EditAfterLock,
            Reason = dto.Reason.Trim(),
            PreviousValuesJson = prev,
            NewValuesJson = newSnap,
            PerformedByUserId = performedByUserId,
            PerformedAtUtc = DateTime.UtcNow,
            Notes = dto.Notes,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.RecordAsync("DailyEvaluation", "OverrideUpdate", new { evaluationId, performedByUserId }, cancellationToken);

        return (await GetEvaluationByIdAsync(evaluationId, cancellationToken))!;
    }

    public async Task<IReadOnlyList<EvaluationOverrideLogReadDto>> GetOverrideLogsForEvaluationAsync(int evaluationId, CancellationToken cancellationToken = default)
    {
        return await _db.EvaluationOverrideLogs.AsNoTracking()
            .Where(l => l.DailyEvaluationID == evaluationId)
            .OrderByDescending(l => l.PerformedAtUtc)
            .Select(l => new EvaluationOverrideLogReadDto
            {
                EvaluationOverrideLogID = l.EvaluationOverrideLogID,
                DailyEvaluationID = l.DailyEvaluationID,
                EvaluationLockID = l.EvaluationLockID,
                OverrideActionType = l.OverrideActionType,
                Reason = l.Reason,
                PreviousValuesJson = l.PreviousValuesJson,
                NewValuesJson = l.NewValuesJson,
                PerformedByUserId = l.PerformedByUserId,
                PerformedAtUtc = l.PerformedAtUtc
            }).ToListAsync(cancellationToken);
    }

    #endregion
}
