using Backend.Data;
using Backend.DTOS.School.Analytics;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly TenantDbContext _db;
    private readonly IUnitOfWork _unitOfWork;

    public AnalyticsService(TenantDbContext db, IUnitOfWork unitOfWork)
    {
        _db = db;
        _unitOfWork = unitOfWork;
    }

    public Task<AnalyticsDashboardResultDto> GetExecutiveDashboardAsync(
        AnalyticsDashboardQueryDto query,
        CancellationToken cancellationToken = default)
    {
        query ??= new AnalyticsDashboardQueryDto();
        query.Audience = DashboardAudience.TopManagement;
        return _unitOfWork.Analytics.GetDashboardAsync(query, cancellationToken);
    }

    public Task<AnalyticsDashboardResultDto> GetEducationalSupervisorDashboardAsync(
        AnalyticsDashboardQueryDto query,
        CancellationToken cancellationToken = default)
    {
        query ??= new AnalyticsDashboardQueryDto();
        query.Audience = DashboardAudience.EducationalSupervisor;
        return _unitOfWork.Analytics.GetDashboardAsync(query, cancellationToken);
    }

    public Task<AnalyticsDashboardResultDto> GetAdministrativeSupervisorDashboardAsync(
        AnalyticsDashboardQueryDto query,
        CancellationToken cancellationToken = default)
    {
        query ??= new AnalyticsDashboardQueryDto();
        query.Audience = DashboardAudience.AdministrativeSupervisor;
        return _unitOfWork.Analytics.GetDashboardAsync(query, cancellationToken);
    }

    public async Task<AnalyticsDashboardResultDto> GetEmployeeDashboardAsync(
        int employeeProfileId,
        AnalyticsDashboardQueryDto query,
        CancellationToken cancellationToken = default)
    {
        query ??= new AnalyticsDashboardQueryDto();
        query.Audience = DashboardAudience.EmployeeSelf;
        var dash = await _unitOfWork.Analytics.GetDashboardAsync(query, cancellationToken);
        dash.Teachers = dash.Teachers.Where(t => t.EmployeeProfileID == employeeProfileId).ToList();
        dash.Snapshots = dash.Snapshots.Where(s => s.EmployeeProfileID == null || s.EmployeeProfileID == employeeProfileId).ToList();
        return dash;
    }

    public Task<AnalyticsDashboardResultDto> GetSchoolDashboardAsync(
        AnalyticsDashboardQueryDto query,
        CancellationToken cancellationToken = default)
    {
        query ??= new AnalyticsDashboardQueryDto();
        query.Audience = DashboardAudience.School;
        return _unitOfWork.Analytics.GetDashboardAsync(query, cancellationToken);
    }

    public async Task<IReadOnlyList<KpiDefinitionListDto>> GetKpiDefinitionsAsync(
        AnalyticsListQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var q = _db.KpiDefinitions.AsNoTracking().AsQueryable();
        if (query.SchoolID is int sid && sid > 0)
            q = q.Where(x => x.SchoolID == sid);
        q = q.Where(x => x.IsActive).OrderBy(x => x.SortOrder).ThenBy(x => x.Code);
        return await q
            .Take(Math.Clamp(query.Take, 1, 500))
            .Select(x => new KpiDefinitionListDto
            {
                KpiDefinitionID = x.KpiDefinitionID,
                SchoolID = x.SchoolID,
                Code = x.Code,
                Title = x.Title,
                EnglishName = x.EnglishName,
                ArabicName = x.ArabicName,
                Description = x.Description,
                Category = (int)x.Category,
                TargetAudience = x.TargetAudience == null ? null : (int)x.TargetAudience.Value,
                CalculationType = (int)x.CalculationType,
                Unit = x.Unit,
                HigherIsBetter = x.HigherIsBetter,
                DefaultTargetValue = x.TargetValue,
                IsSystemKpi = x.IsSystemKpi,
                IsActive = x.IsActive,
                SortOrder = x.SortOrder
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AnalyticsDashboardResultDto> GetKpiSnapshotsDashboardAsync(
        AnalyticsListQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var q = new AnalyticsDashboardQueryDto
        {
            SchoolID = query.SchoolID,
            PeriodKind = query.PeriodKind,
            Audience = DashboardAudience.TopManagement
        };
        return await _unitOfWork.Analytics.GetDashboardAsync(q, cancellationToken);
    }

    public async Task<YearComparisonResultDto> GetYearOverYearComparisonAsync(
        YearComparisonQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var result = new YearComparisonResultDto
        {
            SchoolID = query.SchoolID,
            CurrentYearID = query.CurrentYearID,
            PreviousYearID = query.PreviousYearID,
            PeriodKind = query.PeriodKind,
        };

        async Task<AnalyticsSchoolRowDto?> PickSchoolRow(int yearId)
        {
            var row = await _db.SchoolAnalytics.AsNoTracking()
                .Where(x => x.SchoolID == query.SchoolID
                    && x.AcademicYearID == yearId
                    && x.PeriodKind == query.PeriodKind)
                .OrderByDescending(x => x.ComputedAtUtc)
                .Select(x => new AnalyticsSchoolRowDto
                {
                    SchoolAnalyticsID = x.SchoolAnalyticsID,
                    SchoolID = x.SchoolID,
                    PeriodKind = x.PeriodKind,
                    PeriodStartUtc = x.PeriodStartUtc,
                    PeriodEndUtc = x.PeriodEndUtc,
                    KpiCount = x.KpiCount,
                    OverallScore = x.OverallScore,
                    AverageTeacherScore = x.AverageTeacherScore,
                    TotalViolations = x.TotalViolations,
                    TotalAchievements = x.TotalAchievements,
                    TotalActivities = x.TotalActivities,
                    TotalComplaints = x.TotalComplaints,
                    EmployeeCount = x.EmployeeCount,
                    ActiveTeacherCount = x.ActiveTeacherCount,
                    RiskLevel = x.RiskLevel,
                    TargetAchievementPercent = x.TargetAchievementPercent,
                    ComputedAtUtc = x.ComputedAtUtc
                })
                .FirstOrDefaultAsync(cancellationToken);
            return row;
        }

        result.Current = await PickSchoolRow(query.CurrentYearID);
        result.Previous = await PickSchoolRow(query.PreviousYearID);
        if (result.Current != null && result.Previous != null)
        {
            result.OverallScoreDelta = (result.Current.OverallScore ?? 0) - (result.Previous.OverallScore ?? 0);
            result.ViolationsDelta = result.Current.TotalViolations - result.Previous.TotalViolations;
            result.AchievementsDelta = result.Current.TotalAchievements - result.Previous.TotalAchievements;
            result.ActivitiesDelta = result.Current.TotalActivities - result.Previous.TotalActivities;
            result.ComplaintsDelta = result.Current.TotalComplaints - result.Previous.TotalComplaints;
        }

        return result;
    }

    public async Task<AnalyticsGenerationResultDto> GenerateSnapshotsAsync(
        AnalyticsGenerateRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<string>();
        var sid = request.SchoolID;
        Year? year = null;
        if (request.AcademicYearID is int yid && yid > 0)
        {
            year = await _db.Years.AsNoTracking().FirstOrDefaultAsync(y => y.YearID == yid && y.SchoolID == sid, cancellationToken);
            if (year == null)
                messages.Add("Academic year not found for school; using calendar-based period only.");
        }

        var asOf = DateTime.UtcNow;
        var (startUtc, endUtc) = ResolvePeriodUtc(request.PeriodKind, year, asOf);
        var periodStart = startUtc;
        var periodEnd = endUtc;
        var prevStart = periodStart.AddMonths(-1);
        var prevEnd = periodStart;

        if (request.ReplaceExistingForPeriod)
        {
            await _db.SchoolAnalytics.Where(x =>
                    x.SchoolID == sid && x.PeriodKind == request.PeriodKind
                    && x.PeriodStartUtc == periodStart && x.PeriodEndUtc == periodEnd)
                .ExecuteDeleteAsync(cancellationToken);
            await _db.DepartmentAnalytics.Where(x =>
                    x.SchoolID == sid && x.PeriodKind == request.PeriodKind
                    && x.PeriodStartUtc == periodStart && x.PeriodEndUtc == periodEnd)
                .ExecuteDeleteAsync(cancellationToken);
            await _db.TeacherAnalytics.Where(x =>
                    x.SchoolID == sid && x.PeriodKind == request.PeriodKind
                    && x.PeriodStartUtc == periodStart && x.PeriodEndUtc == periodEnd)
                .ExecuteDeleteAsync(cancellationToken);
            await _db.TrendAnalyses.Where(x =>
                    x.SchoolID == sid && x.PeriodKind == request.PeriodKind
                    && x.FromUtc == periodStart && x.ToUtc == periodEnd)
                .ExecuteDeleteAsync(cancellationToken);
            await _db.KpiSnapshots.Where(x =>
                    x.SchoolID == sid && x.PeriodKind == request.PeriodKind
                    && x.PeriodStartUtc == periodStart && x.PeriodEndUtc == periodEnd
                    && x.EmployeeProfileID == null && x.DepartmentName == null)
                .ExecuteDeleteAsync(cancellationToken);
        }

        var ensured = await EnsureCoreKpiDefinitionsAsync(sid, cancellationToken);
        var defsByCode = await _db.KpiDefinitions.AsNoTracking()
            .Where(d => d.SchoolID == sid && d.IsActive)
            .ToDictionaryAsync(d => d.Code, d => d, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var evalStart = DateOnly.FromDateTime(periodStart);
        var evalEnd = DateOnly.FromDateTime(periodEnd);

        var avgTeacherEval = await _db.DailyEvaluations.AsNoTracking()
            .Where(e => e.SchoolID == sid
                && (request.AcademicYearID == null || e.AcademicYearID == request.AcademicYearID)
                && (e.Status == DailyEvaluationStatus.Submitted || e.Status == DailyEvaluationStatus.Locked)
                && e.EvaluationDate >= evalStart && e.EvaluationDate < evalEnd)
            .Select(e => (decimal?)e.TotalScore)
            .AverageAsync(cancellationToken) ?? 0m;

        var totalViolations = await _db.Violations.AsNoTracking()
            .CountAsync(v => v.SchoolID == sid && v.OpenedAtUtc >= periodStart && v.OpenedAtUtc < periodEnd, cancellationToken);

        var totalAchievements = await _db.AchievementRequests.AsNoTracking()
            .CountAsync(a => a.SchoolID == sid
                && a.Status == AchievementRequestStatus.Approved
                && (a.ResolvedAtUtc ?? a.SubmittedAtUtc) >= periodStart
                && (a.ResolvedAtUtc ?? a.SubmittedAtUtc) < periodEnd, cancellationToken);

        var totalActivities = await _db.ActivityRequests.AsNoTracking()
            .CountAsync(a => a.SchoolID == sid
                && a.Status >= ActivityRequestStatus.Submitted
                && a.SubmittedAtUtc >= periodStart && a.SubmittedAtUtc < periodEnd, cancellationToken);

        var totalComplaints = await _db.Complaints.AsNoTracking()
            .CountAsync(c => c.SchoolID == sid
                && c.SubmittedAtUtc >= periodStart && c.SubmittedAtUtc < periodEnd, cancellationToken);

        var employeeCount = await _db.EmployeeProfiles.AsNoTracking()
            .CountAsync(e => e.SchoolID == sid && e.IsActive, cancellationToken);

        var activeTeacherCount = await _db.EmployeeProfiles.AsNoTracking()
            .CountAsync(e => e.SchoolID == sid && e.IsActive && e.TeacherID != null, cancellationToken);

        var complaintRate = employeeCount > 0 ? 100m * totalComplaints / employeeCount : 0m;

        var risk = AnalyticsRiskLevel.Low;
        if (totalViolations > 20 || complaintRate > 15m) risk = AnalyticsRiskLevel.High;
        else if (totalViolations > 8 || complaintRate > 7m) risk = AnalyticsRiskLevel.Medium;

        var schoolRow = new SchoolAnalytics
        {
            SchoolID = sid,
            AcademicYearID = request.AcademicYearID,
            PeriodKind = request.PeriodKind,
            PeriodStartUtc = periodStart,
            PeriodEndUtc = periodEnd,
            KpiCount = defsByCode.Count,
            OverallScore = avgTeacherEval,
            AverageTeacherScore = avgTeacherEval,
            TotalViolations = totalViolations,
            TotalAchievements = totalAchievements,
            TotalActivities = totalActivities,
            TotalComplaints = totalComplaints,
            PlanCompletionPercent = null,
            EmployeeCount = employeeCount,
            ActiveTeacherCount = activeTeacherCount,
            RiskLevel = risk,
            Notes = "PlanCompletionPercent: pending organizational plan completion metrics.",
            ComputedAtUtc = DateTime.UtcNow
        };
        _db.SchoolAnalytics.Add(schoolRow);

        var snapCount = 0;
        snapCount += await UpsertKpiSnapshotAsync(defsByCode, "AVG_TEACHER_EVALUATION", sid, request.AcademicYearID, request.PeriodKind, periodStart, periodEnd, avgTeacherEval, cancellationToken);
        snapCount += await UpsertKpiSnapshotAsync(defsByCode, "TOTAL_VIOLATIONS", sid, request.AcademicYearID, request.PeriodKind, periodStart, periodEnd, totalViolations, cancellationToken);
        snapCount += await UpsertKpiSnapshotAsync(defsByCode, "TOTAL_ACHIEVEMENTS", sid, request.AcademicYearID, request.PeriodKind, periodStart, periodEnd, totalAchievements, cancellationToken);
        snapCount += await UpsertKpiSnapshotAsync(defsByCode, "TOTAL_ACTIVITIES", sid, request.AcademicYearID, request.PeriodKind, periodStart, periodEnd, totalActivities, cancellationToken);
        snapCount += await UpsertKpiSnapshotAsync(defsByCode, "COMPLAINT_RATE", sid, request.AcademicYearID, request.PeriodKind, periodStart, periodEnd, complaintRate, cancellationToken);
        snapCount += await UpsertKpiSnapshotAsync(defsByCode, "PLAN_COMPLETION", sid, request.AcademicYearID, request.PeriodKind, periodStart, periodEnd, 0, cancellationToken, KpiSnapshotStatus.PendingSource, "Pending plan completion data source.");
        snapCount += await UpsertKpiSnapshotAsync(defsByCode, "EMPLOYEE_COUNT", sid, request.AcademicYearID, request.PeriodKind, periodStart, periodEnd, employeeCount, cancellationToken);
        snapCount += await UpsertKpiSnapshotAsync(defsByCode, "ACTIVE_TEACHER_COUNT", sid, request.AcademicYearID, request.PeriodKind, periodStart, periodEnd, activeTeacherCount, cancellationToken);
        snapCount += await UpsertKpiSnapshotAsync(defsByCode, "PERFORMANCE_RISK_LEVEL", sid, request.AcademicYearID, request.PeriodKind, periodStart, periodEnd, (int)risk, cancellationToken);

        var deptCount = 0;
        var jobTypes = await _db.EmployeeJobTypes.AsNoTracking().Where(j => j.IsActive).ToListAsync(cancellationToken);
        foreach (var jt in jobTypes)
        {
            var empIds = await _db.EmployeeProfiles.AsNoTracking()
                .Where(e => e.SchoolID == sid && e.EmployeeJobTypeID == jt.EmployeeJobTypeID && e.IsActive)
                .Select(e => e.EmployeeProfileID)
                .ToListAsync(cancellationToken);
            if (empIds.Count == 0) continue;

            var deptAvg = await _db.DailyEvaluations.AsNoTracking()
                .Where(e => empIds.Contains(e.EvaluatedEmployeeProfileID)
                    && (e.Status == DailyEvaluationStatus.Submitted || e.Status == DailyEvaluationStatus.Locked)
                    && e.EvaluationDate >= evalStart && e.EvaluationDate < evalEnd)
                .Select(e => (decimal?)e.TotalScore)
                .AverageAsync(cancellationToken);

            var vCount = await _db.Violations.AsNoTracking()
                .CountAsync(v => v.SchoolID == sid && empIds.Contains(v.SubjectEmployeeProfileID)
                    && v.OpenedAtUtc >= periodStart && v.OpenedAtUtc < periodEnd, cancellationToken);

            var achCount = await _db.AchievementRequests.AsNoTracking()
                .CountAsync(a => a.SchoolID == sid && empIds.Contains(a.EmployeeProfileID)
                    && a.Status == AchievementRequestStatus.Approved
                    && (a.ResolvedAtUtc ?? a.SubmittedAtUtc) >= periodStart
                    && (a.ResolvedAtUtc ?? a.SubmittedAtUtc) < periodEnd, cancellationToken);

            var actCount = await _db.ActivityRequests.AsNoTracking()
                .CountAsync(a => a.SchoolID == sid && empIds.Contains(a.EmployeeProfileID)
                    && a.Status >= ActivityRequestStatus.Submitted
                    && a.SubmittedAtUtc >= periodStart && a.SubmittedAtUtc < periodEnd, cancellationToken);

            var compCount = await _db.Complaints.AsNoTracking()
                .CountAsync(c => c.SchoolID == sid && empIds.Contains(c.SubmitterEmployeeProfileID)
                    && c.SubmittedAtUtc >= periodStart && c.SubmittedAtUtc < periodEnd, cancellationToken);

            _db.DepartmentAnalytics.Add(new DepartmentAnalytics
            {
                SchoolID = sid,
                EmployeeJobTypeID = jt.EmployeeJobTypeID,
                DepartmentName = jt.Name,
                AcademicYearID = request.AcademicYearID,
                PeriodKind = request.PeriodKind,
                PeriodStartUtc = periodStart,
                PeriodEndUtc = periodEnd,
                KpiCount = defsByCode.Count,
                AverageScore = deptAvg,
                ViolationCount = vCount,
                AchievementCount = achCount,
                ActivityCount = actCount,
                ComplaintCount = compCount,
                EmployeeCount = empIds.Count,
                PerformanceLevel = ScoreToLevel(deptAvg),
                Notes = "PlanCompletionPercent: pending.",
                ComputedAtUtc = DateTime.UtcNow
            });
            deptCount++;
        }

        var teacherCount = 0;
        var teachers = await _db.EmployeeProfiles.AsNoTracking()
            .Where(e => e.SchoolID == sid && e.IsActive && e.TeacherID != null)
            .Select(e => new { e.EmployeeProfileID, e.TeacherID })
            .ToListAsync(cancellationToken);

        foreach (var t in teachers)
        {
            var tAvg = await _db.DailyEvaluations.AsNoTracking()
                .Where(e => e.EvaluatedEmployeeProfileID == t.EmployeeProfileID
                    && (e.Status == DailyEvaluationStatus.Submitted || e.Status == DailyEvaluationStatus.Locked)
                    && e.EvaluationDate >= evalStart && e.EvaluationDate < evalEnd)
                .Select(e => (decimal?)e.TotalScore)
                .AverageAsync(cancellationToken);

            var visitAvg = await _db.SupervisorVisits.AsNoTracking()
                .Where(v => v.SchoolID == sid && v.VisitedTeacherID == t.TeacherID
                    && v.Status == SupervisorVisitStatus.Submitted
                    && v.VisitDate >= evalStart && v.VisitDate < evalEnd)
                .Select(v => (decimal?)v.OverallScoreOutOf100)
                .AverageAsync(cancellationToken);

            var perf = await _db.EmployeePerformanceSummaries.AsNoTracking()
                .Where(p => p.EmployeeProfileID == t.EmployeeProfileID
                    && (request.AcademicYearID == null || p.AcademicYearID == request.AcademicYearID))
                .OrderByDescending(p => p.GeneratedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            var trend = AnalyticsTrendDirection.Unknown;
            if (tAvg is decimal a && a >= 75) trend = AnalyticsTrendDirection.Improving;
            else if (tAvg is decimal b && b < 55) trend = AnalyticsTrendDirection.Declining;
            else if (tAvg != null) trend = AnalyticsTrendDirection.Stable;

            _db.TeacherAnalytics.Add(new TeacherAnalytics
            {
                SchoolID = sid,
                EmployeeProfileID = t.EmployeeProfileID,
                AcademicYearID = request.AcademicYearID,
                PeriodKind = request.PeriodKind,
                PeriodStartUtc = periodStart,
                PeriodEndUtc = periodEnd,
                KpiCount = defsByCode.Count,
                CompositeScore = tAvg,
                AverageDailyEvaluationScore = tAvg,
                SupervisorVisitAverage = visitAvg,
                AchievementPoints = perf?.AchievementPoints ?? 0,
                ViolationPoints = perf?.ViolationPoints ?? 0,
                ActivityCount = perf?.ActivityCount ?? 0,
                ComplaintCount = await _db.Complaints.AsNoTracking()
                    .CountAsync(c => c.SchoolID == sid && c.SubmitterEmployeeProfileID == t.EmployeeProfileID
                        && c.SubmittedAtUtc >= periodStart && c.SubmittedAtUtc < periodEnd, cancellationToken),
                TrendDirection = trend,
                PerformanceLevel = perf?.PerformanceLevel ?? ScoreToLevel(tAvg),
                StrengthsSummary = perf?.StrengthsSummary,
                WeaknessesSummary = perf?.WeaknessesSummary,
                Recommendations = perf?.Recommendations,
                ComputedAtUtc = DateTime.UtcNow
            });
            teacherCount++;
        }

        var trendWritten = 0;
        if (defsByCode.TryGetValue("AVG_TEACHER_EVALUATION", out var kpiEval))
        {
            var prevAvg = await _db.DailyEvaluations.AsNoTracking()
                .Where(e => e.SchoolID == sid
                    && (e.Status == DailyEvaluationStatus.Submitted || e.Status == DailyEvaluationStatus.Locked)
                    && e.EvaluationDate >= DateOnly.FromDateTime(prevStart)
                    && e.EvaluationDate < DateOnly.FromDateTime(prevEnd))
                .Select(e => (decimal?)e.TotalScore)
                .AverageAsync(cancellationToken);

            var delta = avgTeacherEval - (prevAvg ?? avgTeacherEval);
            decimal? pct = prevAvg is decimal p and > 0 ? (delta / p) * 100m : null;
            _db.TrendAnalyses.Add(new TrendAnalysis
            {
                SchoolID = sid,
                KpiDefinitionID = kpiEval.KpiDefinitionID,
                AcademicYearID = request.AcademicYearID,
                EntityType = AnalyticsEntityType.School,
                MetricCode = kpiEval.Code,
                DashboardAudience = DashboardAudience.TopManagement,
                PeriodKind = request.PeriodKind,
                FromUtc = periodStart,
                ToUtc = periodEnd,
                BaselineValue = prevAvg,
                CurrentValue = avgTeacherEval,
                DeltaValue = delta,
                DeltaPercent = pct,
                IsPositiveTrend = delta >= 0,
                TrendDirection = delta > 0.5m ? AnalyticsTrendDirection.Improving : delta < -0.5m ? AnalyticsTrendDirection.Declining : AnalyticsTrendDirection.Stable,
                Interpretation = "Compared to immediately preceding period window.",
                ComputedAtUtc = DateTime.UtcNow
            });
            trendWritten++;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new AnalyticsGenerationResultDto
        {
            SchoolID = sid,
            AcademicYearID = request.AcademicYearID,
            PeriodKind = request.PeriodKind,
            PeriodStartUtc = periodStart,
            PeriodEndUtc = periodEnd,
            KpiDefinitionsEnsured = ensured,
            KpiSnapshotsWritten = snapCount,
            SchoolAnalyticsWritten = 1,
            DepartmentAnalyticsWritten = deptCount,
            TeacherAnalyticsWritten = teacherCount,
            TrendAnalysisWritten = trendWritten,
            Messages = messages
        };
    }

    private static string? ScoreToLevel(decimal? score)
    {
        if (score == null) return null;
        return score switch
        {
            >= 85m => "Excellent",
            >= 70m => "Good",
            >= 55m => "Fair",
            _ => "NeedsSupport"
        };
    }

    private static (DateTime startUtc, DateTime endUtc) ResolvePeriodUtc(
        AnalyticsPeriodKind kind,
        Year? year,
        DateTime asOfUtc)
    {
        var d = asOfUtc.Kind == DateTimeKind.Utc ? asOfUtc : asOfUtc.ToUniversalTime();
        if (year != null && kind == AnalyticsPeriodKind.Yearly)
        {
            var end = year.YearDateEnd ?? year.YearDateStart.AddYears(1);
            return (year.YearDateStart, end);
        }

        return kind switch
        {
            AnalyticsPeriodKind.Daily => (d.Date, d.Date.AddDays(1)),
            AnalyticsPeriodKind.Weekly =>
                (d.Date.AddDays(-(int)d.DayOfWeek), d.Date.AddDays(-(int)d.DayOfWeek).AddDays(7)),
            AnalyticsPeriodKind.Monthly =>
                (new DateTime(d.Year, d.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(d.Year, d.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
            AnalyticsPeriodKind.Termly =>
                (new DateTime(d.Year, ((d.Month - 1) / 4) * 4 + 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(d.Year, ((d.Month - 1) / 4) * 4 + 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(4)),
            AnalyticsPeriodKind.Yearly =>
                (new DateTime(d.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(d.Year + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            _ => (new DateTime(d.Year, d.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(d.Year, d.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1))
        };
    }

    private async Task<int> EnsureCoreKpiDefinitionsAsync(int schoolId, CancellationToken cancellationToken)
    {
        var templates = new (string Code, string En, string Ar, KpiCategory Cat, KpiCalculationType Calc, decimal? Target, DashboardAudience? Aud, int Order)[]
        {
            ("AVG_TEACHER_EVALUATION", "Average teacher evaluation score", "متوسط تقييم المعلمين", KpiCategory.Academic, KpiCalculationType.Average, 75m, DashboardAudience.EducationalSupervisor, 10),
            ("TOTAL_VIOLATIONS", "Total violations", "إجمالي المخالفات", KpiCategory.Discipline, KpiCalculationType.Count, 0m, DashboardAudience.AdministrativeSupervisor, 20),
            ("TOTAL_ACHIEVEMENTS", "Total approved achievements", "إجمالي الإنجازات المعتمدة", KpiCategory.Engagement, KpiCalculationType.Count, null, DashboardAudience.TopManagement, 30),
            ("TOTAL_ACTIVITIES", "Total staff activities", "إجمالي الأنشطة", KpiCategory.Engagement, KpiCalculationType.Count, null, DashboardAudience.TopManagement, 40),
            ("COMPLAINT_RATE", "Complaints per 100 employees", "معدل الشكاوى لكل 100 موظف", KpiCategory.Discipline, KpiCalculationType.Ratio, null, DashboardAudience.AdministrativeSupervisor, 50),
            ("PLAN_COMPLETION", "Strategic plan completion %", "نسبة إنجاز الخطة", KpiCategory.Institutional, KpiCalculationType.Ratio, null, DashboardAudience.TopManagement, 60),
            ("EMPLOYEE_COUNT", "Active employees", "عدد الموظفين النشطين", KpiCategory.Workforce, KpiCalculationType.Count, null, DashboardAudience.TopManagement, 70),
            ("ACTIVE_TEACHER_COUNT", "Active teachers", "المعلمون النشطون", KpiCategory.Workforce, KpiCalculationType.Count, null, DashboardAudience.EducationalSupervisor, 80),
            ("PERFORMANCE_RISK_LEVEL", "Performance risk level (1=low..3=high)", "مستوى مخاطر الأداء", KpiCategory.Institutional, KpiCalculationType.Latest, null, DashboardAudience.TopManagement, 90),
        };

        var added = 0;
        foreach (var t in templates)
        {
            var exists = await _db.KpiDefinitions.AnyAsync(
                d => d.SchoolID == schoolId && d.Code == t.Code, cancellationToken);
            if (exists) continue;
            _db.KpiDefinitions.Add(new KpiDefinition
            {
                SchoolID = schoolId,
                Code = t.Code,
                Title = t.En,
                EnglishName = t.En,
                ArabicName = t.Ar,
                Category = t.Cat,
                CalculationType = t.Calc,
                TargetAudience = t.Aud,
                TargetValue = t.Target,
                HigherIsBetter = t.Code != "TOTAL_VIOLATIONS" && t.Code != "COMPLAINT_RATE" && t.Code != "PERFORMANCE_RISK_LEVEL",
                IsSystemKpi = true,
                IsActive = true,
                SortOrder = t.Order,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            });
            added++;
        }

        if (added > 0)
            await _db.SaveChangesAsync(cancellationToken);
        return added;
    }

    private async Task<int> UpsertKpiSnapshotAsync(
        Dictionary<string, KpiDefinition> defsByCode,
        string code,
        int schoolId,
        int? academicYearId,
        AnalyticsPeriodKind periodKind,
        DateTime periodStart,
        DateTime periodEnd,
        decimal value,
        CancellationToken cancellationToken,
        KpiSnapshotStatus status = KpiSnapshotStatus.Ok,
        string? notes = null)
    {
        if (!defsByCode.TryGetValue(code, out var def)) return 0;
        var prev = await _db.KpiSnapshots.AsNoTracking()
            .Where(s => s.KpiDefinitionID == def.KpiDefinitionID && s.SchoolID == schoolId
                && s.EmployeeProfileID == null && s.DepartmentName == null
                && s.PeriodEndUtc <= periodStart)
            .OrderByDescending(s => s.PeriodEndUtc)
            .Select(s => (decimal?)s.Value)
            .FirstOrDefaultAsync(cancellationToken);
        decimal? changePct = null;
        if (prev is decimal p && p != 0)
            changePct = (value - p) / p * 100m;

        _db.KpiSnapshots.Add(new KpiSnapshot
        {
            KpiDefinitionID = def.KpiDefinitionID,
            SchoolID = schoolId,
            AcademicYearID = academicYearId,
            PeriodKind = periodKind,
            PeriodStartUtc = periodStart,
            PeriodEndUtc = periodEnd,
            SnapshotDateUtc = DateTime.UtcNow,
            Value = value,
            TargetValue = def.TargetValue,
            PreviousValue = prev,
            ChangePercent = changePct,
            Status = status,
            Notes = notes,
            RecordedAtUtc = DateTime.UtcNow
        });
        return 1;
    }
}
