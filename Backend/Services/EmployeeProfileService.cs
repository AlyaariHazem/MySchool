using System.Globalization;
using System.Security.Claims;
using Backend.Common;
using Backend.Data;
using Backend.DTOS.School.Employees;
using Backend.Interfaces;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class EmployeeProfileService : IEmployeeProfileService
{
    private const int MaxEmployeesPageSize = 200;

    private readonly TenantDbContext _db;
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeYearAssignmentService _yearAssignments;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITimeCapsuleService _timeCapsule;

    public EmployeeProfileService(
        TenantDbContext db,
        IUserRepository userRepository,
        IEmployeeYearAssignmentService yearAssignments,
        IHttpContextAccessor httpContextAccessor,
        ITimeCapsuleService timeCapsule)
    {
        _db = db;
        _userRepository = userRepository;
        _yearAssignments = yearAssignments;
        _httpContextAccessor = httpContextAccessor;
        _timeCapsule = timeCapsule;
    }

    public async Task<IReadOnlyList<EmployeeJobTypeListDto>> GetJobTypesAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _db.EmployeeJobTypes
            .AsNoTracking()
            .OrderBy(j => j.SortOrder)
            .ThenBy(j => j.EmployeeJobTypeID)
            .Select(j => new EmployeeJobTypeListDto
            {
                EmployeeJobTypeID = j.EmployeeJobTypeID,
                Code = j.Code,
                Name = j.Name,
                NameAr = j.NameAr,
                SortOrder = j.SortOrder,
                IsActive = j.IsActive
            })
            .ToListAsync(cancellationToken);

        return rows;
    }

    public async Task<EmployeeProfileReadDto> CreateAsync(EmployeeProfileCreateDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto.FullName);
        var (schoolId, yearId) = await ResolveSchoolAndAcademicYearForCreateAsync(dto, cancellationToken);
        dto.SchoolID = schoolId;
        dto.CurrentAcademicYearID = yearId;
        await ValidateCoreReferencesAsync(dto.SchoolID, dto.CurrentAcademicYearID, dto.EmployeeJobTypeID, cancellationToken);
        if (string.IsNullOrWhiteSpace(dto.EmployeeCode))
            dto.EmployeeCode = await AllocateNextNumericEmployeeCodeAsync(dto.SchoolID, cancellationToken);
        else
            dto.EmployeeCode = dto.EmployeeCode.Trim();
        await EnsureEmployeeCodeUniqueAsync(dto.SchoolID, dto.EmployeeCode, excludeProfileId: null, cancellationToken);

        var teacherId = dto.TeacherID;
        var managerId = dto.ManagerID;
        var schoolStaffId = dto.SchoolStaffID;
        var userId = dto.UserId;

        (teacherId, managerId, schoolStaffId, userId) = await ApplyAutoLegacyRowsForHrAsync(
            MapCreateToUpdate(dto), teacherId, managerId, schoolStaffId, userId, existingTeacherId: null, existingManagerId: null, cancellationToken);

        await ValidateLegacyLinksAsync(dto.SchoolID, teacherId, managerId, schoolStaffId, cancellationToken);

        var entity = new EmployeeProfile
        {
            UserId = userId,
            SchoolID = dto.SchoolID,
            CurrentAcademicYearID = dto.CurrentAcademicYearID,
            EmployeeJobTypeID = dto.EmployeeJobTypeID,
            EmployeeCode = dto.EmployeeCode!,
            FullName = MapName(dto.FullName),
            FullNameAlis = MapNameAlis(dto.FullNameAlis),
            NationalId = dto.NationalId,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address,
            HireDate = dto.HireDate,
            EmploymentStatus = dto.EmploymentStatus,
            Notes = dto.Notes,
            IsActive = dto.IsActive,
            TeacherID = teacherId,
            ManagerID = managerId,
            SchoolStaffID = schoolStaffId,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _db.EmployeeProfiles.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        await _timeCapsule.EnsureCapsuleForEmployeeAsync(entity.EmployeeProfileID, entity.SchoolID, cancellationToken);
        return (await MapToReadDtoAsync(entity.EmployeeProfileID, cancellationToken))!;
    }

    public async Task<EmployeeProfileReadDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await MapToReadDtoAsync(id, cancellationToken);

    /// <summary>
    /// Uses explicit <paramref name="dto"/> school/year when both are set (e.g. recruitment).
    /// Otherwise, for a <c>MANAGER</c> caller, uses that manager's school and its active academic year.
    /// </summary>
    private async Task<(int SchoolId, int YearId)> ResolveSchoolAndAcademicYearForCreateAsync(
        EmployeeProfileCreateDto dto,
        CancellationToken cancellationToken)
    {
        if (dto.SchoolID > 0 && dto.CurrentAcademicYearID > 0)
            return (dto.SchoolID, dto.CurrentAcademicYearID);

        var user = _httpContextAccessor.HttpContext?.User;
        var userType = user?.FindFirstValue("UserType");
        var userId = user?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.Equals(userType, "MANAGER", StringComparison.OrdinalIgnoreCase))
        {
            var sid = await GetSchoolIdForManagerUserAsync(userId, cancellationToken);
            if (sid is not > 0)
                throw new ArgumentException("No school is linked to this manager account.");
            var yid = await GetActiveYearIdForSchoolAsync(sid.Value, cancellationToken);
            if (yid is not > 0)
                throw new ArgumentException("No academic year is configured for this school.");
            return (sid.Value, yid.Value);
        }

        throw new ArgumentException(
            "SchoolID and CurrentAcademicYearID are required when not using a school manager account, or sign in as a school manager.");
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

    public async Task<int?> GetSchoolIdForManagerUserAsync(string? userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;
        return await _db.Managers.AsNoTracking()
            .Where(m => m.UserID == userId)
            .Select(m => (int?)m.SchoolID)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>Shared filters for list and paged endpoints (active year for school is enforced server-side).</summary>
    private async Task<IQueryable<EmployeeProfile>> GetEmployeeProfilesFilteredQueryAsync(
        EmployeeProfileListFilterDto filter, CancellationToken cancellationToken)
    {
        filter ??= new EmployeeProfileListFilterDto();
        int? activeYearForSchool = null;
        if (filter.SchoolID is int sid && sid > 0)
            activeYearForSchool = await GetActiveYearIdForSchoolAsync(sid, cancellationToken);

        var q = _db.EmployeeProfiles.AsNoTracking().AsQueryable();

        if (filter.SchoolID is > 0)
            q = q.Where(e => e.SchoolID == filter.SchoolID);
        if (activeYearForSchool is int y)
            q = q.Where(e => e.CurrentAcademicYearID == y);
        if (filter.EmployeeJobTypeID is > 0)
            q = q.Where(e => e.EmployeeJobTypeID == filter.EmployeeJobTypeID);
        if (filter.IsActive is bool b)
            q = q.Where(e => e.IsActive == b);
        if (filter.EmploymentStatus is { } es)
            q = q.Where(e => e.EmploymentStatus == es);

        return q;
    }

    public async Task<IReadOnlyList<EmployeeProfileReadDto>> GetAllAsync(EmployeeProfileListFilterDto? filter, CancellationToken cancellationToken = default)
    {
        filter ??= new EmployeeProfileListFilterDto();
        var q = await GetEmployeeProfilesFilteredQueryAsync(filter, cancellationToken);
        var rows = await q
            .Include(e => e.JobType)
            .OrderBy(e => e.SchoolID).ThenBy(e => e.EmployeeCode)
            .ToListAsync(cancellationToken);
        return rows.Select(MapToReadDtoFromEntity).ToList();
    }

    public async Task<PagedResult<EmployeeProfileOptionDto>> GetPageAsync(
        EmployeeProfilePageRequestDto request, CancellationToken cancellationToken = default)
    {
        request ??= new EmployeeProfilePageRequestDto();
        var filter = request.Filter ?? new EmployeeProfileListFilterDto();

        var pageIndex = Math.Max(0, request.PageIndex);
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;
        if (pageSize > MaxEmployeesPageSize)
            pageSize = MaxEmployeesPageSize;

        var q = await GetEmployeeProfilesFilteredQueryAsync(filter, cancellationToken);
        var ordered = q.OrderBy(e => e.SchoolID).ThenBy(e => e.EmployeeCode);

        var totalCount = await ordered.CountAsync(cancellationToken);

        var items = await ordered
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(e => new EmployeeProfileOptionDto
            {
                Id = e.EmployeeProfileID,
                FullName = new EmployeeNameDto
                {
                    FirstName = e.FullName.FirstName,
                    MiddleName = e.FullName.MiddleName,
                    LastName = e.FullName.LastName
                }
            })
            .ToListAsync(cancellationToken);

        var totalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedResult<EmployeeProfileOptionDto>(items, pageIndex + 1, pageSize, totalCount, totalPages);
    }

    public async Task<PagedResult<EmployeeProfileReadDto>> GetListPageAsync(
        EmployeeProfilePageRequestDto request,
        CancellationToken cancellationToken = default)
    {
        request ??= new EmployeeProfilePageRequestDto();
        var filter = request.Filter ?? new EmployeeProfileListFilterDto();

        var pageIndex = Math.Max(0, request.PageIndex);
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;
        if (pageSize > MaxEmployeesPageSize)
            pageSize = MaxEmployeesPageSize;

        var q = await GetEmployeeProfilesFilteredQueryAsync(filter, cancellationToken);
        var ordered = q.Include(e => e.JobType).OrderBy(e => e.SchoolID).ThenBy(e => e.EmployeeCode);

        var totalCount = await ordered.CountAsync(cancellationToken);

        var rows = await ordered
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = rows.Select(MapToReadDtoFromEntity).ToList();
        var totalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedResult<EmployeeProfileReadDto>(items, pageIndex + 1, pageSize, totalCount, totalPages);
    }

    public async Task<EmployeeProfileReadDto> UpdateAsync(int id, EmployeeProfileUpdateDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto.FullName);
        var entity = await _db.EmployeeProfiles.FirstOrDefaultAsync(e => e.EmployeeProfileID == id, cancellationToken)
                     ?? throw new KeyNotFoundException($"Employee profile {id} was not found.");

        if (dto.SchoolID <= 0)
            dto.SchoolID = entity.SchoolID;
        if (dto.CurrentAcademicYearID <= 0)
            dto.CurrentAcademicYearID = entity.CurrentAcademicYearID;

        if (string.IsNullOrWhiteSpace(dto.EmployeeCode))
            dto.EmployeeCode = entity.EmployeeCode;
        else
            dto.EmployeeCode = dto.EmployeeCode.Trim();

        await ValidateCoreReferencesAsync(dto.SchoolID, dto.CurrentAcademicYearID, dto.EmployeeJobTypeID, cancellationToken);
        await EnsureEmployeeCodeUniqueAsync(dto.SchoolID, dto.EmployeeCode, excludeProfileId: id, cancellationToken);

        var teacherId = dto.TeacherID;
        var managerId = dto.ManagerID;
        var schoolStaffId = dto.SchoolStaffID;
        var userId = dto.UserId;

        (teacherId, managerId, schoolStaffId, userId) = await ApplyAutoLegacyRowsForHrAsync(
            dto, teacherId, managerId, schoolStaffId, userId, entity.TeacherID, entity.ManagerID, cancellationToken);

        await ValidateLegacyLinksAsync(dto.SchoolID, teacherId, managerId, schoolStaffId, cancellationToken);

        entity.UserId = userId;
        entity.SchoolID = dto.SchoolID;
        entity.CurrentAcademicYearID = dto.CurrentAcademicYearID;
        entity.EmployeeJobTypeID = dto.EmployeeJobTypeID;
        entity.EmployeeCode = dto.EmployeeCode!.Trim();
        entity.FullName = MapName(dto.FullName);
        entity.FullNameAlis = MapNameAlis(dto.FullNameAlis);
        entity.NationalId = dto.NationalId;
        entity.DateOfBirth = dto.DateOfBirth;
        entity.Gender = dto.Gender;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        entity.Address = dto.Address;
        entity.HireDate = dto.HireDate;
        entity.EmploymentStatus = dto.EmploymentStatus;
        entity.Notes = dto.Notes;
        entity.IsActive = dto.IsActive;
        entity.TeacherID = teacherId;
        entity.ManagerID = managerId;
        entity.SchoolStaffID = schoolStaffId;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return (await MapToReadDtoAsync(id, cancellationToken))!;
    }

    public async Task<bool> DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.EmployeeProfiles.FirstOrDefaultAsync(e => e.EmployeeProfileID == id, cancellationToken);
        if (entity == null) return false;
        entity.IsActive = false;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<EmployeeProfileFullDto> GetFullProfileAsync(int id, CancellationToken cancellationToken = default)
    {
        var profile = await GetByIdAsync(id, cancellationToken)
                      ?? throw new KeyNotFoundException($"Employee profile {id} was not found.");

        var quals = await _db.EmployeeQualifications.AsNoTracking()
            .Where(q => q.EmployeeProfileID == id)
            .OrderBy(q => q.EmployeeQualificationID)
            .ToListAsync(cancellationToken);

        var specs = await _db.EmployeeSpecializations.AsNoTracking()
            .Where(s => s.EmployeeProfileID == id)
            .OrderBy(s => s.EmployeeSpecializationID)
            .ToListAsync(cancellationToken);

        var hist = await _db.EmployeeHistories.AsNoTracking()
            .Where(h => h.EmployeeProfileID == id)
            .OrderByDescending(h => h.StartDate)
            .ToListAsync(cancellationToken);

        var docs = await _db.EmployeeDocuments.AsNoTracking()
            .Where(d => d.EmployeeProfileID == id)
            .OrderByDescending(d => d.UploadedAtUtc)
            .ToListAsync(cancellationToken);

        var leaves = await _db.EmployeeLeaves.AsNoTracking()
            .Where(l => l.EmployeeProfileID == id)
            .OrderByDescending(l => l.StartDate)
            .ToListAsync(cancellationToken);

        var perf = await _db.EmployeePerformanceSummaries.AsNoTracking()
            .Where(p => p.EmployeeProfileID == id)
            .OrderByDescending(p => p.GeneratedAtUtc)
            .ToListAsync(cancellationToken);

        return new EmployeeProfileFullDto
        {
            Profile = profile,
            Qualifications = quals.Select(MapQualification).ToList(),
            Specializations = specs.Select(MapSpecialization).ToList(),
            HistoryRecords = hist.Select(MapHistory).ToList(),
            Documents = docs.Select(MapDocument).ToList(),
            Leaves = leaves.Select(MapLeave).ToList(),
            PerformanceSummaries = perf.Select(MapPerformance).ToList()
        };
    }

    public async Task<EmployeeQualificationDto> AddQualificationAsync(int employeeProfileId, EmployeeQualificationDto dto, CancellationToken cancellationToken = default)
    {
        await EnsureProfileExistsAsync(employeeProfileId, cancellationToken);
        var row = new EmployeeQualification
        {
            EmployeeProfileID = employeeProfileId,
            DegreeName = dto.DegreeName,
            Major = dto.Major,
            Institution = dto.Institution,
            GraduationYear = dto.GraduationYear,
            GradeOrScore = dto.GradeOrScore,
            Notes = dto.Notes
        };
        _db.EmployeeQualifications.Add(row);
        await _db.SaveChangesAsync(cancellationToken);
        dto.EmployeeQualificationID = row.EmployeeQualificationID;
        return dto;
    }

    public async Task<EmployeeSpecializationDto> AddSpecializationAsync(int employeeProfileId, EmployeeSpecializationDto dto, CancellationToken cancellationToken = default)
    {
        await EnsureProfileExistsAsync(employeeProfileId, cancellationToken);
        var row = new EmployeeSpecialization
        {
            EmployeeProfileID = employeeProfileId,
            Name = dto.Name,
            Category = dto.Category,
            Level = dto.Level,
            Notes = dto.Notes
        };
        _db.EmployeeSpecializations.Add(row);
        await _db.SaveChangesAsync(cancellationToken);
        dto.EmployeeSpecializationID = row.EmployeeSpecializationID;
        return dto;
    }

    public async Task<EmployeeHistoryDto> AddHistoryAsync(int employeeProfileId, EmployeeHistoryDto dto, CancellationToken cancellationToken = default)
    {
        var profile = await _db.EmployeeProfiles.AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployeeProfileID == employeeProfileId, cancellationToken)
            ?? throw new KeyNotFoundException($"Employee profile {employeeProfileId} was not found.");

        if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.EndDate < dto.StartDate)
            throw new ArgumentException("History EndDate cannot be before StartDate.");

        await ValidateYearForSchoolAsync(dto.AcademicYearID, dto.SchoolID, cancellationToken);
        if (dto.SchoolID != profile.SchoolID)
            throw new ArgumentException("History SchoolID must match the employee profile school.");

        if (dto.EmployeeJobTypeID is > 0)
            await EnsureJobTypeExistsAsync(dto.EmployeeJobTypeID.Value, cancellationToken);

        var row = new EmployeeHistory
        {
            EmployeeProfileID = employeeProfileId,
            AcademicYearID = dto.AcademicYearID,
            SchoolID = dto.SchoolID,
            EmployeeJobTypeID = dto.EmployeeJobTypeID,
            JobTitle = dto.JobTitle,
            Department = dto.Department,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = dto.Status,
            Notes = dto.Notes
        };
        _db.EmployeeHistories.Add(row);
        await _db.SaveChangesAsync(cancellationToken);
        dto.EmployeeHistoryID = row.EmployeeHistoryID;
        return dto;
    }

    public async Task<EmployeeDocumentDto> AddDocumentAsync(int employeeProfileId, EmployeeDocumentDto dto, CancellationToken cancellationToken = default)
    {
        await EnsureProfileExistsAsync(employeeProfileId, cancellationToken);
        var uploaded = dto.UploadedAtUtc ?? DateTime.UtcNow;
        var row = new EmployeeDocument
        {
            EmployeeProfileID = employeeProfileId,
            DocumentType = dto.DocumentType,
            Title = dto.Title,
            FileName = dto.FileName,
            FileUrl = dto.FileUrl,
            UploadedAtUtc = uploaded,
            ExpiryDate = dto.ExpiryDate,
            Notes = dto.Notes,
            IsActive = dto.IsActive
        };
        _db.EmployeeDocuments.Add(row);
        await _db.SaveChangesAsync(cancellationToken);
        dto.EmployeeDocumentID = row.EmployeeDocumentID;
        dto.UploadedAtUtc = row.UploadedAtUtc;
        return dto;
    }

    public async Task<EmployeeLeaveDto> AddLeaveAsync(int employeeProfileId, EmployeeLeaveDto dto, CancellationToken cancellationToken = default)
    {
        var profile = await _db.EmployeeProfiles.AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployeeProfileID == employeeProfileId, cancellationToken)
            ?? throw new KeyNotFoundException($"Employee profile {employeeProfileId} was not found.");

        if (dto.EndDate < dto.StartDate)
            throw new ArgumentException("Leave EndDate cannot be before StartDate.");

        await ValidateYearForSchoolAsync(dto.AcademicYearID, profile.SchoolID, cancellationToken);

        var total = dto.TotalDays;
        if (total <= 0)
            total = (decimal)(dto.EndDate.Date - dto.StartDate.Date).TotalDays + 1;

        var row = new EmployeeLeave
        {
            EmployeeProfileID = employeeProfileId,
            AcademicYearID = dto.AcademicYearID,
            LeaveType = dto.LeaveType,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TotalDays = total,
            Reason = dto.Reason,
            ApprovalStatus = dto.ApprovalStatus,
            ApprovedByUserId = dto.ApprovedByUserId,
            ApprovedAtUtc = dto.ApprovedAtUtc,
            Notes = dto.Notes
        };
        _db.EmployeeLeaves.Add(row);
        await _db.SaveChangesAsync(cancellationToken);
        dto.EmployeeLeaveID = row.EmployeeLeaveID;
        dto.TotalDays = row.TotalDays;
        return dto;
    }

    public async Task<EmployeePerformanceSummaryDto> AddPerformanceSummaryAsync(int employeeProfileId, EmployeePerformanceSummaryDto dto, CancellationToken cancellationToken = default)
    {
        var profile = await _db.EmployeeProfiles.AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployeeProfileID == employeeProfileId, cancellationToken)
            ?? throw new KeyNotFoundException($"Employee profile {employeeProfileId} was not found.");

        if (dto.SchoolID != profile.SchoolID)
            throw new ArgumentException("Performance summary SchoolID must match the employee profile school.");

        await ValidateYearForSchoolAsync(dto.AcademicYearID, profile.SchoolID, cancellationToken);

        if (dto.EmployeeJobTypeID is > 0)
            await EnsureJobTypeExistsAsync(dto.EmployeeJobTypeID.Value, cancellationToken);

        var generated = dto.GeneratedAtUtc ?? DateTime.UtcNow;
        var row = new EmployeePerformanceSummary
        {
            EmployeeProfileID = employeeProfileId,
            AcademicYearID = dto.AcademicYearID,
            SchoolID = dto.SchoolID,
            EmployeeJobTypeID = dto.EmployeeJobTypeID,
            JobTitle = dto.JobTitle,
            EvaluationScore = dto.EvaluationScore,
            AchievementPoints = dto.AchievementPoints,
            ViolationPoints = dto.ViolationPoints,
            RequestCount = dto.RequestCount,
            ActivityCount = dto.ActivityCount,
            PerformanceLevel = dto.PerformanceLevel,
            StrengthsSummary = dto.StrengthsSummary,
            WeaknessesSummary = dto.WeaknessesSummary,
            Recommendations = dto.Recommendations,
            FinalNotes = dto.FinalNotes,
            GeneratedAtUtc = generated
        };
        _db.EmployeePerformanceSummaries.Add(row);
        await _db.SaveChangesAsync(cancellationToken);
        dto.EmployeePerformanceSummaryID = row.EmployeePerformanceSummaryID;
        dto.GeneratedAtUtc = row.GeneratedAtUtc;
        return dto;
    }

    private async Task EnsureProfileExistsAsync(int employeeProfileId, CancellationToken cancellationToken)
    {
        if (!await _db.EmployeeProfiles.AnyAsync(e => e.EmployeeProfileID == employeeProfileId, cancellationToken))
            throw new KeyNotFoundException($"Employee profile {employeeProfileId} was not found.");
    }

    private async Task ValidateCoreReferencesAsync(int schoolId, int yearId, int jobTypeId, CancellationToken cancellationToken)
    {
        if (!await _db.Schools.AnyAsync(s => s.SchoolID == schoolId, cancellationToken))
            throw new ArgumentException("SchoolID is invalid.");
        await ValidateYearForSchoolAsync(yearId, schoolId, cancellationToken);
        await EnsureJobTypeExistsAsync(jobTypeId, cancellationToken);
    }

    private async Task ValidateYearForSchoolAsync(int yearId, int schoolId, CancellationToken cancellationToken)
    {
        var ok = await _db.Years.AnyAsync(y => y.YearID == yearId && y.SchoolID == schoolId, cancellationToken);
        if (!ok)
            throw new ArgumentException("Academic year is invalid for the selected school.");
    }

    private async Task EnsureJobTypeExistsAsync(int jobTypeId, CancellationToken cancellationToken)
    {
        if (!await _db.EmployeeJobTypes.AnyAsync(j => j.EmployeeJobTypeID == jobTypeId && j.IsActive, cancellationToken))
            throw new ArgumentException("EmployeeJobTypeID is invalid or inactive.");
    }

    /// <summary>Next numeric code for the school (max parsed integer + 1), skipping collisions with any existing string code.</summary>
    private async Task<string> AllocateNextNumericEmployeeCodeAsync(int schoolId, CancellationToken cancellationToken)
    {
        var codes = await _db.EmployeeProfiles.AsNoTracking()
            .Where(e => e.SchoolID == schoolId)
            .Select(e => e.EmployeeCode)
            .ToListAsync(cancellationToken);
        var maxNum = 0;
        foreach (var c in codes)
        {
            if (int.TryParse(c?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) && n > maxNum)
                maxNum = n;
        }

        var candidate = maxNum + 1;
        for (var guard = 0; guard < 10_000; guard++)
        {
            var codeStr = candidate.ToString(CultureInfo.InvariantCulture);
            var taken = await _db.EmployeeProfiles.AsNoTracking().AnyAsync(
                e => e.SchoolID == schoolId && e.EmployeeCode == codeStr,
                cancellationToken);
            if (!taken)
                return codeStr;
            candidate++;
        }

        throw new InvalidOperationException("Could not allocate a unique employee code for this school.");
    }

    private async Task EnsureEmployeeCodeUniqueAsync(int schoolId, string code, int? excludeProfileId, CancellationToken cancellationToken)
    {
        var trimmed = code.Trim();
        var taken = await _db.EmployeeProfiles.AnyAsync(
            e => e.SchoolID == schoolId && e.EmployeeCode == trimmed && (!excludeProfileId.HasValue || e.EmployeeProfileID != excludeProfileId),
            cancellationToken);
        if (taken)
            throw new ArgumentException($"EmployeeCode '{trimmed}' is already used in this school.");
    }

    private static EmployeeProfileUpdateDto MapCreateToUpdate(EmployeeProfileCreateDto c) => new()
    {
        SchoolID = c.SchoolID,
        CurrentAcademicYearID = c.CurrentAcademicYearID,
        EmployeeJobTypeID = c.EmployeeJobTypeID,
        EmployeeCode = c.EmployeeCode,
        FullName = c.FullName,
        FullNameAlis = c.FullNameAlis,
        NationalId = c.NationalId,
        DateOfBirth = c.DateOfBirth,
        Gender = c.Gender,
        Phone = c.Phone,
        Email = c.Email,
        Address = c.Address,
        HireDate = c.HireDate,
        EmploymentStatus = c.EmploymentStatus,
        Notes = c.Notes,
        IsActive = c.IsActive,
        UserId = c.UserId,
        TeacherID = c.TeacherID,
        ManagerID = c.ManagerID,
        SchoolStaffID = c.SchoolStaffID
    };

    /// <summary>
    /// When HR creates a teacher/manager profile without linking an existing legacy row, create the corresponding
    /// <see cref="Teacher"/> or <see cref="Manager"/> row so lists like GET /api/Teacher stay in sync.
    /// </summary>
    private async Task<(int? TeacherId, int? ManagerId, int? SchoolStaffId, string? UserId)> ApplyAutoLegacyRowsForHrAsync(
        EmployeeProfileUpdateDto dto,
        int? teacherId,
        int? managerId,
        int? schoolStaffId,
        string? userId,
        int? existingTeacherId,
        int? existingManagerId,
        CancellationToken cancellationToken)
    {
        if (teacherId is null or 0 && existingTeacherId is > 0)
            teacherId = existingTeacherId;
        if (managerId is null or 0 && existingManagerId is > 0)
            managerId = existingManagerId;

        if (CountLegacyLinks(teacherId, managerId, schoolStaffId) > 0)
            return (teacherId, managerId, schoolStaffId, userId);

        var jt = await _db.EmployeeJobTypes.AsNoTracking()
            .FirstOrDefaultAsync(j => j.EmployeeJobTypeID == dto.EmployeeJobTypeID, cancellationToken);
        if (jt == null)
            return (teacherId, managerId, schoolStaffId, userId);

        if (string.Equals(jt.Code, "TEACHER", StringComparison.OrdinalIgnoreCase) && !(teacherId is > 0))
        {
            var r = await CreateTeacherRowForHrAsync(dto, userId, cancellationToken);
            // Always persist canonical Identity user id on the profile (client may send UserName like "Hazem").
            return (r.TeacherId, managerId, schoolStaffId, r.UserId);
        }

        if (string.Equals(jt.Code, "MANAGER", StringComparison.OrdinalIgnoreCase) && !(managerId is > 0))
        {
            var r = await CreateManagerRowForHrAsync(dto, userId, cancellationToken);
            return (teacherId, r.ManagerId, schoolStaffId, r.UserId);
        }

        return (teacherId, managerId, schoolStaffId, userId);
    }

    private async Task<int> ResolveDefaultManagerIdForSchoolAsync(int schoolId, CancellationToken cancellationToken)
    {
        var mid = await _db.Managers.AsNoTracking()
            .Where(m => m.SchoolID == schoolId)
            .OrderBy(m => m.ManagerID)
            .Select(m => (int?)m.ManagerID)
            .FirstOrDefaultAsync(cancellationToken);
        if (mid is null or <= 0)
            throw new ArgumentException(
                "The school has no manager record yet. Add a manager first, then create teachers, or link an existing teacher.");
        return mid.Value;
    }

    private async Task<(int TeacherId, string UserId)> CreateTeacherRowForHrAsync(
        EmployeeProfileUpdateDto dto,
        string? preferredUserId,
        CancellationToken cancellationToken)
    {
        ApplicationUser? resolvedUser = null;
        if (!string.IsNullOrWhiteSpace(preferredUserId))
        {
            resolvedUser = await _userRepository.GetUserByIdOrNameAsync(preferredUserId);
            if (resolvedUser != null)
            {
                var linked = await _db.Teachers.AsNoTracking()
                    .FirstOrDefaultAsync(t => t.UserID == resolvedUser.Id, cancellationToken);
                if (linked != null)
                    return (linked.TeacherID, resolvedUser.Id);
            }
        }

        var managerLegacyId = await ResolveDefaultManagerIdForSchoolAsync(dto.SchoolID, cancellationToken);
        ApplicationUser userRow;
        if (resolvedUser != null)
        {
            userRow = resolvedUser;
        }
        else if (!string.IsNullOrWhiteSpace(preferredUserId))
        {
            throw new ArgumentException(
                "UserId is invalid. Use the account user id (GUID) or the login user name (e.g. Hazem).");
        }
        else
        {
            var userName = "HrTeacher_" + Guid.NewGuid().ToString("N")[..8];
            userRow = await _userRepository.CreateUserAsync(new ApplicationUser
            {
                UserName = userName,
                Email = string.IsNullOrWhiteSpace(dto.Email) ? $"{userName}@school.local" : dto.Email!,
                Address = dto.Address,
                Gender = string.IsNullOrWhiteSpace(dto.Gender) ? "Male" : dto.Gender,
                HireDate = dto.HireDate ?? DateTime.UtcNow,
                PhoneNumber = dto.Phone,
                UserType = "TEACHER"
            }, "TEACHER", "TEACHER");
        }

        var teacher = new Teacher
        {
            FullName = MapName(dto.FullName),
            DOB = dto.DateOfBirth ?? DateTime.UtcNow,
            UserID = userRow.Id,
            ManagerID = managerLegacyId
        };
        _db.Teachers.Add(teacher);
        await _db.SaveChangesAsync(cancellationToken);
        await _yearAssignments.EnsureActiveAssignmentAsync(
            EmployeeYearAssignmentRoles.Teacher, teacher.TeacherID, null, _db);
        return (teacher.TeacherID, userRow.Id);
    }

    private async Task<(int ManagerId, string UserId)> CreateManagerRowForHrAsync(
        EmployeeProfileUpdateDto dto,
        string? preferredUserId,
        CancellationToken cancellationToken)
    {
        ApplicationUser? resolvedUser = null;
        if (!string.IsNullOrWhiteSpace(preferredUserId))
        {
            resolvedUser = await _userRepository.GetUserByIdOrNameAsync(preferredUserId);
            if (resolvedUser != null)
            {
                var linked = await _db.Managers.AsNoTracking()
                    .FirstOrDefaultAsync(m => m.UserID == resolvedUser.Id && m.SchoolID == dto.SchoolID, cancellationToken);
                if (linked != null)
                    return (linked.ManagerID, resolvedUser.Id);
            }
        }

        ApplicationUser userRow;
        if (resolvedUser != null)
        {
            userRow = resolvedUser;
        }
        else if (!string.IsNullOrWhiteSpace(preferredUserId))
        {
            throw new ArgumentException(
                "UserId is invalid. Use the account user id (GUID) or the login user name (e.g. Hazem).");
        }
        else
        {
            var userName = "HrManager_" + Guid.NewGuid().ToString("N")[..8];
            userRow = await _userRepository.CreateUserAsync(new ApplicationUser
            {
                UserName = userName,
                Email = string.IsNullOrWhiteSpace(dto.Email) ? $"{userName}@school.local" : dto.Email!,
                Address = dto.Address,
                Gender = string.IsNullOrWhiteSpace(dto.Gender) ? "Male" : dto.Gender,
                HireDate = dto.HireDate ?? DateTime.UtcNow,
                PhoneNumber = dto.Phone,
                UserType = "MANAGER"
            }, "MANAGER", "MANAGER");
        }

        var manager = new Manager
        {
            FullName = MapName(dto.FullName),
            DOB = dto.DateOfBirth ?? DateTime.UtcNow,
            UserID = userRow.Id,
            SchoolID = dto.SchoolID
        };
        _db.Managers.Add(manager);
        await _db.SaveChangesAsync(cancellationToken);
        await _yearAssignments.EnsureActiveAssignmentAsync(
            EmployeeYearAssignmentRoles.Manager, manager.ManagerID, null, _db);
        return (manager.ManagerID, userRow.Id);
    }

    private static int CountLegacyLinks(int? teacherId, int? managerId, int? staffId)
    {
        var n = 0;
        if (teacherId is > 0) n++;
        if (managerId is > 0) n++;
        if (staffId is > 0) n++;
        return n;
    }

    private async Task ValidateLegacyLinksAsync(int schoolId, int? teacherId, int? managerId, int? schoolStaffId, CancellationToken cancellationToken)
    {
        if (CountLegacyLinks(teacherId, managerId, schoolStaffId) > 1)
            throw new ArgumentException("At most one of TeacherID, ManagerID, SchoolStaffID may be set.");

        if (teacherId is > 0)
        {
            var t = await _db.Teachers.Include(x => x.Manager)
                .FirstOrDefaultAsync(x => x.TeacherID == teacherId, cancellationToken)
                ?? throw new ArgumentException("TeacherID is invalid.");
            if (t.Manager == null || t.Manager.SchoolID != schoolId)
                throw new ArgumentException("Teacher does not belong to the selected school.");
        }

        if (managerId is > 0)
        {
            var m = await _db.Managers.AsNoTracking().FirstOrDefaultAsync(x => x.ManagerID == managerId, cancellationToken)
                    ?? throw new ArgumentException("ManagerID is invalid.");
            if (m.SchoolID != schoolId)
                throw new ArgumentException("Manager does not belong to the selected school.");
        }

        if (schoolStaffId is > 0)
        {
            var s = await _db.SchoolStaff.AsNoTracking().FirstOrDefaultAsync(x => x.SchoolStaffID == schoolStaffId, cancellationToken)
                    ?? throw new ArgumentException("SchoolStaffID is invalid.");
            if (s.SchoolID != schoolId)
                throw new ArgumentException("School staff does not belong to the selected school.");
        }
    }

    private async Task<EmployeeProfileReadDto?> MapToReadDtoAsync(int id, CancellationToken cancellationToken)
    {
        var e = await _db.EmployeeProfiles.AsNoTracking()
            .Include(x => x.JobType)
            .FirstOrDefaultAsync(x => x.EmployeeProfileID == id, cancellationToken);
        return e == null ? null : MapToReadDtoFromEntity(e);
    }

    private static EmployeeProfileReadDto MapToReadDtoFromEntity(EmployeeProfile e) => new()
    {
        EmployeeProfileID = e.EmployeeProfileID,
        UserId = e.UserId,
        SchoolID = e.SchoolID,
        CurrentAcademicYearID = e.CurrentAcademicYearID,
        EmployeeJobTypeID = e.EmployeeJobTypeID,
        JobTypeCode = e.JobType.Code,
        JobTypeName = e.JobType.Name,
        EmployeeCode = e.EmployeeCode,
        FullName = MapNameDto(e.FullName),
        FullNameAlis = e.FullNameAlis == null ? null : MapNameAlisDto(e.FullNameAlis),
        NationalId = e.NationalId,
        DateOfBirth = e.DateOfBirth,
        Gender = e.Gender,
        Phone = e.Phone,
        Email = e.Email,
        Address = e.Address,
        HireDate = e.HireDate,
        EmploymentStatus = e.EmploymentStatus,
        Notes = e.Notes,
        IsActive = e.IsActive,
        CreatedAtUtc = e.CreatedAtUtc,
        UpdatedAtUtc = e.UpdatedAtUtc,
        TeacherID = e.TeacherID,
        ManagerID = e.ManagerID,
        SchoolStaffID = e.SchoolStaffID
    };

    private static Name MapName(EmployeeNameDto dto) => new()
    {
        FirstName = dto.FirstName,
        MiddleName = dto.MiddleName,
        LastName = dto.LastName
    };

    private static NameAlis? MapNameAlis(EmployeeNameAlisDto? dto)
    {
        if (dto == null) return null;
        return new NameAlis
        {
            FirstNameEng = dto.FirstNameEng,
            MiddleNameEng = dto.MiddleNameEng,
            LastNameEng = dto.LastNameEng
        };
    }

    private static EmployeeNameDto MapNameDto(Name n) => new()
    {
        FirstName = n.FirstName,
        MiddleName = n.MiddleName,
        LastName = n.LastName
    };

    private static EmployeeNameAlisDto MapNameAlisDto(NameAlis n) => new()
    {
        FirstNameEng = n.FirstNameEng,
        MiddleNameEng = n.MiddleNameEng,
        LastNameEng = n.LastNameEng
    };

    private static EmployeeQualificationDto MapQualification(EmployeeQualification q) => new()
    {
        EmployeeQualificationID = q.EmployeeQualificationID,
        DegreeName = q.DegreeName,
        Major = q.Major,
        Institution = q.Institution,
        GraduationYear = q.GraduationYear,
        GradeOrScore = q.GradeOrScore,
        Notes = q.Notes
    };

    private static EmployeeSpecializationDto MapSpecialization(EmployeeSpecialization s) => new()
    {
        EmployeeSpecializationID = s.EmployeeSpecializationID,
        Name = s.Name,
        Category = s.Category,
        Level = s.Level,
        Notes = s.Notes
    };

    private static EmployeeHistoryDto MapHistory(EmployeeHistory h) => new()
    {
        EmployeeHistoryID = h.EmployeeHistoryID,
        AcademicYearID = h.AcademicYearID,
        SchoolID = h.SchoolID,
        EmployeeJobTypeID = h.EmployeeJobTypeID,
        JobTitle = h.JobTitle,
        Department = h.Department,
        StartDate = h.StartDate,
        EndDate = h.EndDate,
        Status = h.Status,
        Notes = h.Notes
    };

    private static EmployeeDocumentDto MapDocument(EmployeeDocument d) => new()
    {
        EmployeeDocumentID = d.EmployeeDocumentID,
        DocumentType = d.DocumentType,
        Title = d.Title,
        FileName = d.FileName,
        FileUrl = d.FileUrl,
        UploadedAtUtc = d.UploadedAtUtc,
        ExpiryDate = d.ExpiryDate,
        Notes = d.Notes,
        IsActive = d.IsActive
    };

    private static EmployeeLeaveDto MapLeave(EmployeeLeave l) => new()
    {
        EmployeeLeaveID = l.EmployeeLeaveID,
        AcademicYearID = l.AcademicYearID,
        LeaveType = l.LeaveType,
        StartDate = l.StartDate,
        EndDate = l.EndDate,
        TotalDays = l.TotalDays,
        Reason = l.Reason,
        ApprovalStatus = l.ApprovalStatus,
        ApprovedByUserId = l.ApprovedByUserId,
        ApprovedAtUtc = l.ApprovedAtUtc,
        Notes = l.Notes
    };

    private static EmployeePerformanceSummaryDto MapPerformance(EmployeePerformanceSummary p) => new()
    {
        EmployeePerformanceSummaryID = p.EmployeePerformanceSummaryID,
        AcademicYearID = p.AcademicYearID,
        SchoolID = p.SchoolID,
        EmployeeJobTypeID = p.EmployeeJobTypeID,
        JobTitle = p.JobTitle,
        EvaluationScore = p.EvaluationScore,
        AchievementPoints = p.AchievementPoints,
        ViolationPoints = p.ViolationPoints,
        RequestCount = p.RequestCount,
        ActivityCount = p.ActivityCount,
        PerformanceLevel = p.PerformanceLevel,
        StrengthsSummary = p.StrengthsSummary,
        WeaknessesSummary = p.WeaknessesSummary,
        Recommendations = p.Recommendations,
        FinalNotes = p.FinalNotes,
        GeneratedAtUtc = p.GeneratedAtUtc
    };
}
