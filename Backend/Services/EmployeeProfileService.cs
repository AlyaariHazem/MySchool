using Backend.Data;
using Backend.DTOS.School.Employees;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class EmployeeProfileService : IEmployeeProfileService
{
    private readonly TenantDbContext _db;

    public EmployeeProfileService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<EmployeeProfileReadDto> CreateAsync(EmployeeProfileCreateDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto.FullName);
        await ValidateCoreReferencesAsync(dto.SchoolID, dto.CurrentAcademicYearID, dto.EmployeeJobTypeID, cancellationToken);
        await EnsureEmployeeCodeUniqueAsync(dto.SchoolID, dto.EmployeeCode, excludeProfileId: null, cancellationToken);
        await ValidateLegacyLinksAsync(dto.SchoolID, dto.TeacherID, dto.ManagerID, dto.SchoolStaffID, cancellationToken);

        var entity = new EmployeeProfile
        {
            UserId = dto.UserId,
            SchoolID = dto.SchoolID,
            CurrentAcademicYearID = dto.CurrentAcademicYearID,
            EmployeeJobTypeID = dto.EmployeeJobTypeID,
            EmployeeCode = dto.EmployeeCode.Trim(),
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
            TeacherID = dto.TeacherID,
            ManagerID = dto.ManagerID,
            SchoolStaffID = dto.SchoolStaffID,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _db.EmployeeProfiles.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return (await MapToReadDtoAsync(entity.EmployeeProfileID, cancellationToken))!;
    }

    public async Task<EmployeeProfileReadDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await MapToReadDtoAsync(id, cancellationToken);

    public async Task<IReadOnlyList<EmployeeProfileReadDto>> GetAllAsync(EmployeeProfileListFilterDto? filter, CancellationToken cancellationToken = default)
    {
        var q = _db.EmployeeProfiles
            .AsNoTracking()
            .Include(e => e.JobType)
            .AsQueryable();

        if (filter?.SchoolID is > 0)
            q = q.Where(e => e.SchoolID == filter.SchoolID);
        if (filter?.AcademicYearID is > 0)
            q = q.Where(e => e.CurrentAcademicYearID == filter.AcademicYearID);
        if (filter?.EmployeeJobTypeID is > 0)
            q = q.Where(e => e.EmployeeJobTypeID == filter.EmployeeJobTypeID);
        if (filter?.IsActive is bool b)
            q = q.Where(e => e.IsActive == b);
        if (filter?.EmploymentStatus is { } es)
            q = q.Where(e => e.EmploymentStatus == es);

        var rows = await q.OrderBy(e => e.SchoolID).ThenBy(e => e.EmployeeCode).ToListAsync(cancellationToken);
        return rows.Select(MapToReadDtoFromEntity).ToList();
    }

    public async Task<EmployeeProfileReadDto> UpdateAsync(int id, EmployeeProfileUpdateDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto.FullName);
        var entity = await _db.EmployeeProfiles.FirstOrDefaultAsync(e => e.EmployeeProfileID == id, cancellationToken)
                     ?? throw new KeyNotFoundException($"Employee profile {id} was not found.");

        await ValidateCoreReferencesAsync(dto.SchoolID, dto.CurrentAcademicYearID, dto.EmployeeJobTypeID, cancellationToken);
        await EnsureEmployeeCodeUniqueAsync(dto.SchoolID, dto.EmployeeCode, excludeProfileId: id, cancellationToken);
        await ValidateLegacyLinksAsync(dto.SchoolID, dto.TeacherID, dto.ManagerID, dto.SchoolStaffID, cancellationToken);

        entity.UserId = dto.UserId;
        entity.SchoolID = dto.SchoolID;
        entity.CurrentAcademicYearID = dto.CurrentAcademicYearID;
        entity.EmployeeJobTypeID = dto.EmployeeJobTypeID;
        entity.EmployeeCode = dto.EmployeeCode.Trim();
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
        entity.TeacherID = dto.TeacherID;
        entity.ManagerID = dto.ManagerID;
        entity.SchoolStaffID = dto.SchoolStaffID;
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

    private async Task EnsureEmployeeCodeUniqueAsync(int schoolId, string code, int? excludeProfileId, CancellationToken cancellationToken)
    {
        var trimmed = code.Trim();
        var taken = await _db.EmployeeProfiles.AnyAsync(
            e => e.SchoolID == schoolId && e.EmployeeCode == trimmed && (!excludeProfileId.HasValue || e.EmployeeProfileID != excludeProfileId),
            cancellationToken);
        if (taken)
            throw new ArgumentException($"EmployeeCode '{trimmed}' is already used in this school.");
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
