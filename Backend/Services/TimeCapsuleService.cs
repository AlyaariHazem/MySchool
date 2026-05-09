using System.Text.Json;
using Backend.Data;
using Backend.DTOS.School.TimeCapsule;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class TimeCapsuleService : ITimeCapsuleService
{
    private static readonly JsonSerializerOptions JsonSnapshotOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly TenantDbContext _db;

    public TimeCapsuleService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task EnsureCapsuleForEmployeeAsync(int employeeProfileId, int schoolId, CancellationToken cancellationToken = default)
    {
        var exists = await _db.TimeCapsules.AnyAsync(c => c.EmployeeProfileID == employeeProfileId, cancellationToken);
        if (exists)
            return;
        _db.TimeCapsules.Add(new TimeCapsule
        {
            EmployeeProfileID = employeeProfileId,
            SchoolID = schoolId,
            IsLocked = true,
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        });
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<TimeCapsuleStatusDto> GetCapsuleStatusAsync(
        int employeeProfileId,
        string? currentUserId,
        string? userType,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        if (!await CanAccessEmployeeAsync(employeeProfileId, currentUserId, userType, isAdmin, cancellationToken))
            throw new UnauthorizedAccessException("Not allowed to view this employee's capsule status.");

        var profile = await _db.EmployeeProfiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.EmployeeProfileID == employeeProfileId, cancellationToken)
            ?? throw new InvalidOperationException("Employee profile not found.");

        await EnsureCapsuleForEmployeeAsync(profile.EmployeeProfileID, profile.SchoolID, cancellationToken);

        var capsule = await _db.TimeCapsules.AsNoTracking()
            .FirstAsync(c => c.EmployeeProfileID == employeeProfileId, cancellationToken);

        var latestResignation = await _db.ResignationRequests.AsNoTracking()
            .Where(r => r.EmployeeProfileID == employeeProfileId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var dto = new TimeCapsuleStatusDto
        {
            EmployeeProfileId = employeeProfileId,
            TimeCapsuleId = capsule.TimeCapsuleID,
            IsUnlocked = !capsule.IsLocked
        };

        if (!capsule.IsLocked)
        {
            dto.Phase = "Unlocked";
            dto.MessageAr = "الكبسولة مفتوحة — استمتع بعرض الرحلة.";
            return dto;
        }

        if (latestResignation == null)
        {
            dto.Phase = "LockedNoResignation";
            dto.MessageAr = "لا يمكن فتح الكبسولة إلا عند تقديم استقالة.";
            return dto;
        }

        dto.ResignationRequestId = latestResignation.ResignationRequestID;
        dto.ResignationStatus = latestResignation.Status;

        switch (latestResignation.Status)
        {
            case ResignationRequestStatus.Pending:
                dto.Phase = "ResignationPending";
                dto.MessageAr = "قيد المراجعة — طلب الاستقالة بانتظار موافقة الإدارة.";
                return dto;
            case ResignationRequestStatus.Rejected:
                dto.Phase = "ResignationRejected";
                dto.MessageAr = "تم رفض طلب الاستقالة الأخير.";
                return dto;
        }

        // Resignation approved — check unlock
        var pendingUnlock = await _db.CapsuleUnlockApprovals.AsNoTracking()
            .Where(a => a.TimeCapsuleID == capsule.TimeCapsuleID && a.Status == CapsuleUnlockApprovalStatus.Pending)
            .OrderByDescending(a => a.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (pendingUnlock != null)
        {
            dto.Phase = "UnlockPending";
            dto.PendingUnlockApprovalId = pendingUnlock.CapsuleUnlockApprovalID;
            dto.MessageAr = "قيد المراجعة — بانتظار الموافقة على فتح كبسولة الزمن.";
            return dto;
        }

        var lastUnlock = await _db.CapsuleUnlockApprovals.AsNoTracking()
            .Where(a => a.TimeCapsuleID == capsule.TimeCapsuleID)
            .OrderByDescending(a => a.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastUnlock?.Status == CapsuleUnlockApprovalStatus.Rejected)
        {
            dto.Phase = "UnlockRejected";
            dto.MessageAr = "تم رفض فتح الكبسولة. يمكن لمسؤول إعادة طلب الفتح بعد مراجعة الملاحظات.";
            return dto;
        }

        dto.Phase = "UnlockPending";
        dto.MessageAr = "قيد المراجعة — بانتظار إنشاء طلب فتح الكبسولة.";
        return dto;
    }

    public async Task<ResignationRequestReadDto> RequestResignationAsync(
        ResignationRequestCreateDto dto,
        string currentUserId,
        string? userType,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var profile = await _db.EmployeeProfiles
            .FirstOrDefaultAsync(p => p.EmployeeProfileID == dto.EmployeeProfileId, cancellationToken)
            ?? throw new InvalidOperationException("Employee profile not found.");

        if (!await CanSubmitResignationAsync(profile, currentUserId, userType, isAdmin, cancellationToken))
            throw new UnauthorizedAccessException("Not allowed to submit resignation for this profile.");

        var pending = await _db.ResignationRequests.AnyAsync(
            r => r.EmployeeProfileID == dto.EmployeeProfileId && r.Status == ResignationRequestStatus.Pending,
            cancellationToken);
        if (pending)
            throw new InvalidOperationException("A pending resignation request already exists.");

        await EnsureCapsuleForEmployeeAsync(profile.EmployeeProfileID, profile.SchoolID, cancellationToken);

        var yearOk = await _db.Years.AnyAsync(y => y.YearID == dto.AcademicYearId && y.SchoolID == profile.SchoolID, cancellationToken);
        if (!yearOk)
            throw new InvalidOperationException("Invalid academic year for this school.");

        var entity = new ResignationRequest
        {
            EmployeeProfileID = dto.EmployeeProfileId,
            SchoolID = profile.SchoolID,
            AcademicYearID = dto.AcademicYearId,
            RequestDateUtc = DateTime.UtcNow,
            Reason = dto.Reason,
            Status = ResignationRequestStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.ResignationRequests.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return MapResignation(entity);
    }

    public async Task<ResignationRequestReadDto> ApproveResignationAsync(
        int resignationRequestId,
        string approverUserId,
        string? userType,
        bool isAdmin,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        if (!await IsHrApproverAsync(approverUserId, userType, isAdmin, cancellationToken))
            throw new UnauthorizedAccessException("Only administrators or school managers can approve resignations.");

        var entity = await _db.ResignationRequests
            .Include(r => r.EmployeeProfile)
            .FirstOrDefaultAsync(r => r.ResignationRequestID == resignationRequestId, cancellationToken)
            ?? throw new InvalidOperationException("Resignation request not found.");

        if (!await IsManagerOfSchoolAsync(approverUserId, userType, isAdmin, entity.SchoolID, cancellationToken))
            throw new UnauthorizedAccessException("Manager is not scoped to this school.");

        if (entity.Status != ResignationRequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be approved.");

        entity.Status = ResignationRequestStatus.Approved;
        entity.ApprovedByUserID = approverUserId;
        entity.ApprovedAtUtc = DateTime.UtcNow;
        entity.Notes = notes;

        var capsule = await _db.TimeCapsules.FirstAsync(c => c.EmployeeProfileID == entity.EmployeeProfileID, cancellationToken);

        var oldPendingUnlocks = await _db.CapsuleUnlockApprovals
            .Where(a => a.TimeCapsuleID == capsule.TimeCapsuleID && a.Status == CapsuleUnlockApprovalStatus.Pending)
            .ToListAsync(cancellationToken);
        foreach (var o in oldPendingUnlocks)
        {
            o.Status = CapsuleUnlockApprovalStatus.Rejected;
            o.ApprovedAtUtc = DateTime.UtcNow;
            o.ApprovedByUserID = approverUserId;
            o.Notes = "Superseded by a newer resignation approval.";
        }

        _db.CapsuleUnlockApprovals.Add(new CapsuleUnlockApproval
        {
            TimeCapsuleID = capsule.TimeCapsuleID,
            ResignationRequestID = entity.ResignationRequestID,
            Status = CapsuleUnlockApprovalStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);
        return MapResignation(entity);
    }

    public async Task<ResignationRequestReadDto> RejectResignationAsync(
        int resignationRequestId,
        string approverUserId,
        string? userType,
        bool isAdmin,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        if (!await IsHrApproverAsync(approverUserId, userType, isAdmin, cancellationToken))
            throw new UnauthorizedAccessException("Only administrators or school managers can reject resignations.");

        var entity = await _db.ResignationRequests
            .FirstOrDefaultAsync(r => r.ResignationRequestID == resignationRequestId, cancellationToken)
            ?? throw new InvalidOperationException("Resignation request not found.");

        if (!await IsManagerOfSchoolAsync(approverUserId, userType, isAdmin, entity.SchoolID, cancellationToken))
            throw new UnauthorizedAccessException("Manager is not scoped to this school.");

        if (entity.Status != ResignationRequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be rejected.");

        entity.Status = ResignationRequestStatus.Rejected;
        entity.ApprovedByUserID = approverUserId;
        entity.ApprovedAtUtc = DateTime.UtcNow;
        entity.Notes = notes;

        await _db.SaveChangesAsync(cancellationToken);
        return MapResignation(entity);
    }

    public async Task ApproveCapsuleUnlockAsync(
        int capsuleId,
        string approverUserId,
        string? userType,
        bool isAdmin,
        string? unlockReason,
        CancellationToken cancellationToken = default)
    {
        if (!await IsHrApproverAsync(approverUserId, userType, isAdmin, cancellationToken))
            throw new UnauthorizedAccessException("Only administrators or school managers can approve capsule unlock.");

        var capsule = await _db.TimeCapsules
            .Include(c => c.EmployeeProfile)
            .FirstOrDefaultAsync(c => c.TimeCapsuleID == capsuleId, cancellationToken)
            ?? throw new InvalidOperationException("Time capsule not found.");

        if (!await IsManagerOfSchoolAsync(approverUserId, userType, isAdmin, capsule.SchoolID, cancellationToken))
            throw new UnauthorizedAccessException("Manager is not scoped to this school.");

        if (!capsule.IsLocked)
            throw new InvalidOperationException("Capsule is already unlocked.");

        var approval = await _db.CapsuleUnlockApprovals
            .Include(a => a.ResignationRequest)
            .Where(a => a.TimeCapsuleID == capsuleId && a.Status == CapsuleUnlockApprovalStatus.Pending)
            .OrderByDescending(a => a.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No pending capsule unlock approval.");

        if (approval.ResignationRequest.Status != ResignationRequestStatus.Approved)
            throw new InvalidOperationException("Resignation must be approved before unlocking the capsule.");

        approval.Status = CapsuleUnlockApprovalStatus.Approved;
        approval.ApprovedByUserID = approverUserId;
        approval.ApprovedAtUtc = DateTime.UtcNow;

        capsule.IsLocked = false;
        capsule.UnlockedAtUtc = DateTime.UtcNow;
        capsule.UnlockedByUserID = approverUserId;
        capsule.UnlockReason = unlockReason;

        await _db.SaveChangesAsync(cancellationToken);

        await GenerateCapsuleDataAsync(capsuleId, cancellationToken);
    }

    public async Task RejectCapsuleUnlockAsync(
        int capsuleId,
        string approverUserId,
        string? userType,
        bool isAdmin,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        if (!await IsHrApproverAsync(approverUserId, userType, isAdmin, cancellationToken))
            throw new UnauthorizedAccessException("Only administrators or school managers can reject capsule unlock.");

        var capsule = await _db.TimeCapsules
            .FirstOrDefaultAsync(c => c.TimeCapsuleID == capsuleId, cancellationToken)
            ?? throw new InvalidOperationException("Time capsule not found.");

        if (!await IsManagerOfSchoolAsync(approverUserId, userType, isAdmin, capsule.SchoolID, cancellationToken))
            throw new UnauthorizedAccessException("Manager is not scoped to this school.");

        var approval = await _db.CapsuleUnlockApprovals
            .Include(a => a.ResignationRequest)
            .Where(a => a.TimeCapsuleID == capsuleId && a.Status == CapsuleUnlockApprovalStatus.Pending)
            .OrderByDescending(a => a.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No pending capsule unlock approval.");

        approval.Status = CapsuleUnlockApprovalStatus.Rejected;
        approval.ApprovedByUserID = approverUserId;
        approval.ApprovedAtUtc = DateTime.UtcNow;
        approval.Notes = notes;

        if (approval.ResignationRequest.Status == ResignationRequestStatus.Approved && capsule.IsLocked)
        {
            var hasPending = await _db.CapsuleUnlockApprovals.AnyAsync(
                a => a.TimeCapsuleID == capsuleId && a.Status == CapsuleUnlockApprovalStatus.Pending,
                cancellationToken);
            if (!hasPending)
            {
                _db.CapsuleUnlockApprovals.Add(new CapsuleUnlockApproval
                {
                    TimeCapsuleID = capsuleId,
                    ResignationRequestID = approval.ResignationRequestID,
                    Status = CapsuleUnlockApprovalStatus.Pending,
                    CreatedAtUtc = DateTime.UtcNow,
                    Notes = "Re-opened after previous unlock rejection."
                });
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task GenerateCapsuleDataAsync(int timeCapsuleId, CancellationToken cancellationToken = default)
    {
        var capsule = await _db.TimeCapsules
            .Include(c => c.EmployeeProfile)
            .ThenInclude(p => p!.JobType)
            .FirstAsync(c => c.TimeCapsuleID == timeCapsuleId, cancellationToken);

        if (capsule.IsLocked)
            throw new InvalidOperationException("Cannot generate capsule data while locked.");

        var employeeId = capsule.EmployeeProfileID;
        var schoolId = capsule.SchoolID;

        var existing = await _db.TimeCapsuleSections.AnyAsync(s => s.TimeCapsuleID == timeCapsuleId, cancellationToken);
        if (existing)
            return;

        var years = await _db.Years.AsNoTracking()
            .Where(y => y.SchoolID == schoolId)
            .OrderBy(y => y.YearDateStart)
            .ToListAsync(cancellationToken);

        var perfRows = await _db.EmployeePerformanceSummaries.AsNoTracking()
            .Where(s => s.EmployeeProfileID == employeeId)
            .ToListAsync(cancellationToken);

        var dailyEvals = await _db.DailyEvaluations.AsNoTracking()
            .Where(e => e.EvaluatedEmployeeProfileID == employeeId
                        && (e.Status == DailyEvaluationStatus.Submitted || e.Status == DailyEvaluationStatus.Locked))
            .ToListAsync(cancellationToken);

        var violations = await _db.Violations.AsNoTracking()
            .Include(v => v.ViolationType)
            .Where(v => v.SubjectEmployeeProfileID == employeeId)
            .OrderBy(v => v.OpenedAtUtc)
            .ToListAsync(cancellationToken);

        var achievements = await _db.AchievementRequests.AsNoTracking()
            .Include(a => a.Achievement)
            .Where(a => a.EmployeeProfileID == employeeId)
            .OrderByDescending(a => a.SubmittedAtUtc)
            .ToListAsync(cancellationToken);

        var activities = await _db.ActivityRequests.AsNoTracking()
            .Where(a => a.EmployeeProfileID == employeeId)
            .OrderByDescending(a => a.SubmittedAtUtc)
            .ToListAsync(cancellationToken);

        var documents = await _db.EmployeeDocuments.AsNoTracking()
            .Where(d => d.EmployeeProfileID == employeeId && d.IsActive)
            .OrderByDescending(d => d.UploadedAtUtc)
            .ToListAsync(cancellationToken);

        var analytics = await _db.TeacherAnalytics.AsNoTracking()
            .Where(a => a.EmployeeProfileID == employeeId)
            .OrderBy(a => a.PeriodStartUtc)
            .ToListAsync(cancellationToken);

        var history = await _db.EmployeeHistories.AsNoTracking()
            .Where(h => h.EmployeeProfileID == employeeId)
            .OrderBy(h => h.StartDate)
            .ToListAsync(cancellationToken);

        var profile = capsule.EmployeeProfile;

        var displayName = string.Join(' ', new[] { profile.FullName.FirstName, profile.FullName.MiddleName, profile.FullName.LastName }
            .Where(s => !string.IsNullOrWhiteSpace(s))).Trim();

        var generalInfo = new
        {
            employeeCode = profile.EmployeeCode,
            displayName,
            fullName = new
            {
                profile.FullName.FirstName,
                profile.FullName.MiddleName,
                profile.FullName.LastName
            },
            jobType = profile.JobType?.NameAr ?? profile.JobType?.Name,
            hireDate = profile.HireDate,
            employmentStatus = profile.EmploymentStatus.ToString(),
            email = profile.Email,
            phone = profile.Phone,
            history = history.Select(h => new
            {
                h.AcademicYearID,
                jobTitle = h.JobTitle,
                department = h.Department,
                startDate = h.StartDate,
                endDate = h.EndDate,
                notes = h.Notes
            })
        };

        var yearIds = years.Select(y => y.YearID)
            .Concat(perfRows.Select(p => p.AcademicYearID))
            .Concat(dailyEvals.Select(d => d.AcademicYearID))
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        var performanceTimeline = yearIds.Select(yid =>
        {
            var y = years.FirstOrDefault(x => x.YearID == yid);
            var perf = perfRows.FirstOrDefault(p => p.AcademicYearID == yid);
            var evals = dailyEvals.Where(d => d.AcademicYearID == yid).ToList();
            return new
            {
                academicYearId = yid,
                yearLabel = y != null ? y.YearDateStart.Year.ToString() : yid.ToString(),
                evaluationScore = perf?.EvaluationScore,
                performanceLevel = perf?.PerformanceLevel,
                achievementPoints = perf?.AchievementPoints ?? 0,
                violationPoints = perf?.ViolationPoints ?? 0,
                dailyEvaluationCount = evals.Count,
                dailyEvaluationAverage = evals.Count == 0 ? (decimal?)null : Math.Round(evals.Average(e => e.TotalScore), 2),
                strengthsSummary = perf?.StrengthsSummary,
                weaknessesSummary = perf?.WeaknessesSummary
            };
        }).ToList();

        var achievementsSnap = achievements.Select(a => new
        {
            a.AcademicYearID,
            title = a.Achievement?.Title ?? a.CustomTitle,
            status = a.Status.ToString(),
            pointsHint = a.Achievement?.DefaultPoints,
            submittedAtUtc = a.SubmittedAtUtc
        });

        var violationsSnap = violations.Select(v => new
        {
            v.ViolationID,
            v.AcademicYearID,
            title = v.Title,
            status = v.Status.ToString(),
            typeName = v.ViolationType?.Name,
            openedAtUtc = v.OpenedAtUtc
        });

        var evalSummary = new
        {
            totalEvaluations = dailyEvals.Count,
            averageScore = dailyEvals.Count == 0 ? (decimal?)null : Math.Round(dailyEvals.Average(e => e.TotalScore), 2),
            byYear = dailyEvals.GroupBy(e => e.AcademicYearID).Select(g => new
            {
                academicYearId = g.Key,
                count = g.Count(),
                average = Math.Round(g.Average(e => e.TotalScore), 2)
            })
        };

        var activitiesSnap = activities.Select(a => new
        {
            a.AcademicYearID,
            a.Title,
            status = a.Status.ToString(),
            submittedAtUtc = a.SubmittedAtUtc
        });

        var reportsSnap = documents.Select(d => new
        {
            d.DocumentType,
            d.Title,
            uploadedAtUtc = d.UploadedAtUtc
        });

        var analyticsSnap = analytics.Select(a => new
        {
            a.AcademicYearID,
            a.PeriodKind,
            a.PeriodStartUtc,
            a.PeriodEndUtc,
            a.CompositeScore,
            a.AverageDailyEvaluationScore,
            a.AchievementPoints,
            a.ViolationPoints,
            a.ActivityCount,
            trend = a.TrendDirection.ToString()
        });

        var totals = new
        {
            yearsCovered = yearIds.Count,
            violationCount = violations.Count,
            achievementRequests = achievements.Count,
            activityRequests = activities.Count,
            approvedAchievements = achievements.Count(x => x.Status == AchievementRequestStatus.Approved)
        };

        // TODO: Replace rule-based narrative with AI-generated storytelling (Azure OpenAI / internal LLM) once approved.
        var narrativeText = BuildRuleBasedNarrative(profile, performanceTimeline.Count, totals);

        void AddSection(TimeCapsuleSectionType type, string title, int order, object payload)
        {
            _db.TimeCapsuleSections.Add(new TimeCapsuleSection
            {
                TimeCapsuleID = timeCapsuleId,
                SectionType = type,
                Title = title,
                DataJson = JsonSerializer.Serialize(payload, JsonSnapshotOpts),
                SortOrder = order,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        AddSection(TimeCapsuleSectionType.GeneralInfo, "معلومات عامة", 1, generalInfo);
        AddSection(TimeCapsuleSectionType.PerformanceTimeline, "مسار الأداء عبر السنوات", 2, new { years = performanceTimeline, analytics = analyticsSnap });
        AddSection(TimeCapsuleSectionType.Achievements, "الإنجازات", 3, achievementsSnap);
        AddSection(TimeCapsuleSectionType.Violations, "المخالفات", 4, violationsSnap);
        AddSection(TimeCapsuleSectionType.Evaluations, "ملخص التقييمات", 5, evalSummary);
        AddSection(TimeCapsuleSectionType.Activities, "الأنشطة", 6, activitiesSnap);
        AddSection(TimeCapsuleSectionType.Reports, "الوثائق والتقارير", 7, reportsSnap);
        AddSection(TimeCapsuleSectionType.FinalSummary, "الختام", 8, new { totals, analyticsRollup = analyticsSnap });

        _db.CapsuleNarratives.Add(new CapsuleNarrative
        {
            TimeCapsuleID = timeCapsuleId,
            NarrativeText = narrativeText,
            GeneratedAtUtc = DateTime.UtcNow,
            GeneratedBy = CapsuleNarrativeGeneratedBy.System
        });

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<TimeCapsuleDetailDto?> GetCapsuleAsync(
        int employeeProfileId,
        string? currentUserId,
        string? userType,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        if (!await CanAccessEmployeeAsync(employeeProfileId, currentUserId, userType, isAdmin, cancellationToken))
            throw new UnauthorizedAccessException("Not allowed to view this capsule.");

        var capsule = await _db.TimeCapsules.AsNoTracking()
            .FirstOrDefaultAsync(c => c.EmployeeProfileID == employeeProfileId, cancellationToken);
        if (capsule == null)
            return null;

        if (capsule.IsLocked)
            return null;

        var sections = await _db.TimeCapsuleSections.AsNoTracking()
            .Where(s => s.TimeCapsuleID == capsule.TimeCapsuleID)
            .OrderBy(s => s.SortOrder)
            .Select(s => new TimeCapsuleSectionReadDto
            {
                TimeCapsuleSectionId = s.TimeCapsuleSectionID,
                SectionType = s.SectionType,
                Title = s.Title,
                DataJson = s.DataJson,
                SortOrder = s.SortOrder
            })
            .ToListAsync(cancellationToken);

        var narrative = await _db.CapsuleNarratives.AsNoTracking()
            .Where(n => n.TimeCapsuleID == capsule.TimeCapsuleID)
            .OrderByDescending(n => n.GeneratedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrEmpty(currentUserId))
            await LogAccessAsync(capsule.TimeCapsuleID, currentUserId, CapsuleAccessActionType.Viewed, null, cancellationToken);

        return new TimeCapsuleDetailDto
        {
            TimeCapsuleId = capsule.TimeCapsuleID,
            EmployeeProfileId = capsule.EmployeeProfileID,
            SchoolId = capsule.SchoolID,
            IsLocked = capsule.IsLocked,
            UnlockedAtUtc = capsule.UnlockedAtUtc,
            UnlockedByUserId = capsule.UnlockedByUserID,
            UnlockReason = capsule.UnlockReason,
            Sections = sections,
            NarrativeText = narrative?.NarrativeText,
            NarrativeGeneratedAtUtc = narrative?.GeneratedAtUtc,
            NarrativeGeneratedBy = narrative?.GeneratedBy
        };
    }

    public async Task LogAccessAsync(
        int timeCapsuleId,
        string userId,
        CapsuleAccessActionType action,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        _db.CapsuleAccessLogs.Add(new CapsuleAccessLog
        {
            TimeCapsuleID = timeCapsuleId,
            AccessedByUserID = userId,
            AccessedAtUtc = DateTime.UtcNow,
            ActionType = action,
            Notes = notes
        });
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CapsuleAccessLogReadDto>> GetAccessLogsAsync(
        int capsuleId,
        string? currentUserId,
        string? userType,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        if (!await IsHrApproverAsync(currentUserId, userType, isAdmin, cancellationToken))
            throw new UnauthorizedAccessException("Only administrators or managers can view access logs.");

        var capsule = await _db.TimeCapsules.AsNoTracking()
            .FirstOrDefaultAsync(c => c.TimeCapsuleID == capsuleId, cancellationToken)
            ?? throw new InvalidOperationException("Time capsule not found.");

        if (!isAdmin && !await IsManagerOfSchoolAsync(currentUserId, userType, isAdmin, capsule.SchoolID, cancellationToken))
            throw new UnauthorizedAccessException("Manager is not scoped to this school.");

        return await _db.CapsuleAccessLogs.AsNoTracking()
            .Where(l => l.TimeCapsuleID == capsuleId)
            .OrderByDescending(l => l.AccessedAtUtc)
            .Select(l => new CapsuleAccessLogReadDto
            {
                CapsuleAccessLogId = l.CapsuleAccessLogID,
                AccessedByUserId = l.AccessedByUserID,
                AccessedAtUtc = l.AccessedAtUtc,
                ActionType = l.ActionType,
                Notes = l.Notes
            })
            .ToListAsync(cancellationToken);
    }

    private static ResignationRequestReadDto MapResignation(ResignationRequest r) => new()
    {
        ResignationRequestId = r.ResignationRequestID,
        EmployeeProfileId = r.EmployeeProfileID,
        SchoolId = r.SchoolID,
        AcademicYearId = r.AcademicYearID,
        RequestDateUtc = r.RequestDateUtc,
        Reason = r.Reason,
        Status = r.Status,
        ApprovedByUserId = r.ApprovedByUserID,
        ApprovedAtUtc = r.ApprovedAtUtc,
        Notes = r.Notes
    };

    private async Task<bool> CanAccessEmployeeAsync(
        int employeeProfileId,
        string? userId,
        string? userType,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        if (isAdmin)
            return true;
        var profile = await _db.EmployeeProfiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.EmployeeProfileID == employeeProfileId, cancellationToken);
        if (profile == null)
            return false;
        if (!string.IsNullOrEmpty(profile.UserId) && profile.UserId == userId)
            return true;
        if (string.Equals(userType, "TEACHER", StringComparison.OrdinalIgnoreCase)
            && profile.TeacherID is int teacherRowId
            && !string.IsNullOrEmpty(userId))
        {
            if (await _db.Teachers.AsNoTracking()
                    .AnyAsync(t => t.TeacherID == teacherRowId && t.UserID == userId, cancellationToken))
                return true;
        }
        if (string.Equals(userType, "MANAGER", StringComparison.OrdinalIgnoreCase))
        {
            var sid = await GetSchoolIdForManagerUserAsync(userId, cancellationToken);
            return sid == profile.SchoolID;
        }
        return false;
    }

    private async Task<bool> CanSubmitResignationAsync(
        EmployeeProfile profile,
        string currentUserId,
        string? userType,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        if (isAdmin)
            return true;
        if (string.Equals(userType, "MANAGER", StringComparison.OrdinalIgnoreCase))
        {
            var sid = await GetSchoolIdForManagerUserAsync(currentUserId, cancellationToken);
            return sid == profile.SchoolID;
        }
        if (!string.IsNullOrEmpty(profile.UserId) && profile.UserId == currentUserId)
            return true;
        if (string.Equals(userType, "TEACHER", StringComparison.OrdinalIgnoreCase)
            && profile.TeacherID is int teacherRowId
            && !string.IsNullOrEmpty(currentUserId))
        {
            return await _db.Teachers.AsNoTracking()
                .AnyAsync(t => t.TeacherID == teacherRowId && t.UserID == currentUserId, cancellationToken);
        }
        return false;
    }

    private static Task<bool> IsHrApproverAsync(string? userId, string? userType, bool isAdmin, CancellationToken _)
        => Task.FromResult(isAdmin || string.Equals(userType, "MANAGER", StringComparison.OrdinalIgnoreCase));

    private async Task<bool> IsManagerOfSchoolAsync(
        string? userId,
        string? userType,
        bool isAdmin,
        int schoolId,
        CancellationToken cancellationToken)
    {
        if (isAdmin)
            return true;
        if (!string.Equals(userType, "MANAGER", StringComparison.OrdinalIgnoreCase))
            return false;
        var sid = await GetSchoolIdForManagerUserAsync(userId, cancellationToken);
        return sid == schoolId;
    }

    private async Task<int?> GetSchoolIdForManagerUserAsync(string? userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;
        return await _db.Managers.AsNoTracking()
            .Where(m => m.UserID == userId)
            .Select(m => (int?)m.SchoolID)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static string BuildRuleBasedNarrative(EmployeeProfile profile, int academicYearsTracked, object totals)
    {
        var startYear = profile.HireDate?.Year;
        var name = string.Join(' ', new[] { profile.FullName.FirstName, profile.FullName.MiddleName, profile.FullName.LastName }
            .Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
        if (string.IsNullOrEmpty(name))
            name = "هذا الموظف";

        _ = totals;

        if (startYear is int y)
        {
            return
                $"بدأ {name} رحلته المهنية في عام {y}، وامتدت خبرته عبر السنوات الدراسية المسجلة في النظام. " +
                $"يُظهر مسار الأداء تطورًا متراكمًا عبر {academicYearsTracked} سنة أكاديمية في اللقطات المحفوظة. " +
                "تجمع كبسولة الزمن بين التقييمات والإنجازات والمخالفات والأنشطة في قصة واحدة تُسدل بها الستار على فصل مشرّف.";
        }

        return
            $"تروي كبسولة الزمن قصة {name} عبر السنوات، مع لقطات من الأداء والتقييم والسلوك المهني. " +
            "هذه الصفحة أرشيف للذكريات والأرقام معًا — فصل يُغلق بكرامة، ويُحفظ للرجوع إليه عند الحاجة.";
    }
}
