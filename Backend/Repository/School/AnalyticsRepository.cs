using Backend.Data;
using Backend.DTOS.School.Analytics;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly TenantDbContext _db;

    public AnalyticsRepository(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<AnalyticsDashboardResultDto> GetDashboardAsync(
        AnalyticsDashboardQueryDto query,
        CancellationToken cancellationToken = default)
    {
        query ??= new AnalyticsDashboardQueryDto();

        var snapshotsQ = _db.KpiSnapshots.AsNoTracking().AsQueryable();
        var trendsQ = _db.TrendAnalyses.AsNoTracking().AsQueryable();
        var deptQ = _db.DepartmentAnalytics.AsNoTracking().AsQueryable();
        var teacherQ = _db.TeacherAnalytics.AsNoTracking().AsQueryable();
        var schoolQ = _db.SchoolAnalytics.AsNoTracking().AsQueryable();

        if (query.SchoolID is int schoolId && schoolId > 0)
        {
            snapshotsQ = snapshotsQ.Where(x => x.SchoolID == schoolId);
            trendsQ = trendsQ.Where(x => x.SchoolID == schoolId);
            deptQ = deptQ.Where(x => x.SchoolID == schoolId);
            teacherQ = teacherQ.Where(x => x.SchoolID == schoolId);
            schoolQ = schoolQ.Where(x => x.SchoolID == schoolId);
        }

        if (query.PeriodKind is AnalyticsPeriodKind periodKind)
        {
            snapshotsQ = snapshotsQ.Where(x => x.PeriodKind == periodKind);
            trendsQ = trendsQ.Where(x => x.PeriodKind == periodKind);
            deptQ = deptQ.Where(x => x.PeriodKind == periodKind);
            teacherQ = teacherQ.Where(x => x.PeriodKind == periodKind);
            schoolQ = schoolQ.Where(x => x.PeriodKind == periodKind);
        }

        var audience = query.Audience ?? DashboardAudience.TopManagement;
        trendsQ = trendsQ.Where(x => x.DashboardAudience == audience);

        var snapshots = await snapshotsQ
            .OrderByDescending(x => x.RecordedAtUtc)
            .Take(200)
            .Select(x => new AnalyticsKpiSnapshotRowDto
            {
                KpiSnapshotID = x.KpiSnapshotID,
                KpiDefinitionID = x.KpiDefinitionID,
                KpiTitle = x.KpiDefinition.Title,
                SchoolID = x.SchoolID,
                AcademicYearID = x.AcademicYearID,
                TermID = x.TermID,
                EmployeeProfileID = x.EmployeeProfileID,
                DepartmentName = x.DepartmentName,
                PeriodKind = x.PeriodKind,
                PeriodStartUtc = x.PeriodStartUtc,
                PeriodEndUtc = x.PeriodEndUtc,
                Value = x.Value,
                TargetValue = x.TargetValue,
                RecordedAtUtc = x.RecordedAtUtc
            })
            .ToListAsync(cancellationToken);

        var trends = await trendsQ
            .OrderByDescending(x => x.ComputedAtUtc)
            .Take(200)
            .Select(x => new AnalyticsTrendRowDto
            {
                TrendAnalysisID = x.TrendAnalysisID,
                SchoolID = x.SchoolID,
                KpiDefinitionID = x.KpiDefinitionID,
                KpiTitle = x.KpiDefinition.Title,
                DashboardAudience = x.DashboardAudience,
                PeriodKind = x.PeriodKind,
                FromUtc = x.FromUtc,
                ToUtc = x.ToUtc,
                BaselineValue = x.BaselineValue,
                CurrentValue = x.CurrentValue,
                DeltaValue = x.DeltaValue,
                DeltaPercent = x.DeltaPercent,
                IsPositiveTrend = x.IsPositiveTrend,
                TrendLabel = x.TrendLabel
            })
            .ToListAsync(cancellationToken);

        var departments = await deptQ
            .OrderByDescending(x => x.ComputedAtUtc)
            .Take(100)
            .Select(x => new AnalyticsDepartmentRowDto
            {
                DepartmentAnalyticsID = x.DepartmentAnalyticsID,
                SchoolID = x.SchoolID,
                DepartmentName = x.DepartmentName,
                PeriodKind = x.PeriodKind,
                PeriodStartUtc = x.PeriodStartUtc,
                PeriodEndUtc = x.PeriodEndUtc,
                KpiCount = x.KpiCount,
                AverageScore = x.AverageScore,
                TargetAchievementPercent = x.TargetAchievementPercent,
                ComputedAtUtc = x.ComputedAtUtc
            })
            .ToListAsync(cancellationToken);

        var teachers = await teacherQ
            .OrderByDescending(x => x.ComputedAtUtc)
            .Take(100)
            .Select(x => new AnalyticsTeacherRowDto
            {
                TeacherAnalyticsID = x.TeacherAnalyticsID,
                SchoolID = x.SchoolID,
                EmployeeProfileID = x.EmployeeProfileID,
                EmployeeName = x.EmployeeProfile.FullName.FirstName + " " + x.EmployeeProfile.FullName.LastName,
                PeriodKind = x.PeriodKind,
                PeriodStartUtc = x.PeriodStartUtc,
                PeriodEndUtc = x.PeriodEndUtc,
                KpiCount = x.KpiCount,
                CompositeScore = x.CompositeScore,
                TargetAchievementPercent = x.TargetAchievementPercent,
                ComputedAtUtc = x.ComputedAtUtc
            })
            .ToListAsync(cancellationToken);

        var school = await schoolQ
            .OrderByDescending(x => x.ComputedAtUtc)
            .Take(100)
            .Select(x => new AnalyticsSchoolRowDto
            {
                SchoolAnalyticsID = x.SchoolAnalyticsID,
                SchoolID = x.SchoolID,
                PeriodKind = x.PeriodKind,
                PeriodStartUtc = x.PeriodStartUtc,
                PeriodEndUtc = x.PeriodEndUtc,
                KpiCount = x.KpiCount,
                OverallScore = x.OverallScore,
                TargetAchievementPercent = x.TargetAchievementPercent,
                ComputedAtUtc = x.ComputedAtUtc
            })
            .ToListAsync(cancellationToken);

        var cards = snapshots
            .GroupBy(x => new { x.KpiDefinitionID, x.KpiTitle })
            .Select(g =>
            {
                var latest = g.OrderByDescending(x => x.RecordedAtUtc).First();
                var previous = g.OrderByDescending(x => x.RecordedAtUtc).Skip(1).FirstOrDefault();
                decimal? trend = previous is null ? null : latest.Value - previous.Value;
                return new AnalyticsKpiCardDto
                {
                    Code = $"KPI-{g.Key.KpiDefinitionID}",
                    Label = g.Key.KpiTitle ?? $"KPI {g.Key.KpiDefinitionID}",
                    Value = latest.Value,
                    TargetValue = latest.TargetValue,
                    Trend = trend
                };
            })
            .Take(8)
            .ToList();

        return new AnalyticsDashboardResultDto
        {
            Cards = cards,
            Snapshots = snapshots,
            Trends = trends,
            Departments = departments,
            Teachers = teachers,
            School = school
        };
    }
}
