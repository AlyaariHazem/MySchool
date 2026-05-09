using System.Text.Json.Serialization;
using Backend.Models;

namespace Backend.DTOS.School.Analytics;

public sealed class AnalyticsDashboardQueryDto
{
    public int? SchoolID { get; set; }
    public AnalyticsPeriodKind? PeriodKind { get; set; }
    public DashboardAudience? Audience { get; set; }
}

public sealed class AnalyticsDashboardResultDto
{
    public List<AnalyticsKpiCardDto> Cards { get; set; } = [];
    public List<AnalyticsKpiSnapshotRowDto> Snapshots { get; set; } = [];
    public List<AnalyticsTrendRowDto> Trends { get; set; } = [];
    public List<AnalyticsDepartmentRowDto> Departments { get; set; } = [];
    public List<AnalyticsTeacherRowDto> Teachers { get; set; } = [];
    public List<AnalyticsSchoolRowDto> School { get; set; } = [];
}

public sealed class AnalyticsKpiCardDto
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }

    [JsonPropertyName("target")]
    public decimal? TargetValue { get; set; }

    public decimal? Trend { get; set; }
}

public sealed class AnalyticsKpiSnapshotRowDto
{
    public int KpiSnapshotID { get; set; }
    public int KpiDefinitionID { get; set; }
    public string? KpiTitle { get; set; }
    public int SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public int? TermID { get; set; }
    public int? EmployeeProfileID { get; set; }
    public string? DepartmentName { get; set; }
    public AnalyticsPeriodKind PeriodKind { get; set; }
    public DateTime PeriodStartUtc { get; set; }
    public DateTime PeriodEndUtc { get; set; }
    public decimal Value { get; set; }
    public decimal? TargetValue { get; set; }
    public DateTime RecordedAtUtc { get; set; }
}

public sealed class AnalyticsTrendRowDto
{
    public int TrendAnalysisID { get; set; }
    public int SchoolID { get; set; }
    public int KpiDefinitionID { get; set; }
    public string? KpiTitle { get; set; }
    public DashboardAudience DashboardAudience { get; set; }
    public AnalyticsPeriodKind PeriodKind { get; set; }
    public DateTime FromUtc { get; set; }
    public DateTime ToUtc { get; set; }
    public decimal? BaselineValue { get; set; }
    public decimal? CurrentValue { get; set; }
    public decimal? DeltaValue { get; set; }
    public decimal? DeltaPercent { get; set; }
    public bool IsPositiveTrend { get; set; }
    public string? TrendLabel { get; set; }
}

public sealed class AnalyticsDepartmentRowDto
{
    public int DepartmentAnalyticsID { get; set; }
    public int SchoolID { get; set; }
    public string? DepartmentName { get; set; }
    public AnalyticsPeriodKind PeriodKind { get; set; }
    public DateTime PeriodStartUtc { get; set; }
    public DateTime PeriodEndUtc { get; set; }
    public int KpiCount { get; set; }
    public decimal? AverageScore { get; set; }
    public decimal? TargetAchievementPercent { get; set; }
    public DateTime ComputedAtUtc { get; set; }
}

public sealed class AnalyticsTeacherRowDto
{
    public int TeacherAnalyticsID { get; set; }
    public int SchoolID { get; set; }
    public int EmployeeProfileID { get; set; }
    public string? EmployeeName { get; set; }
    public AnalyticsPeriodKind PeriodKind { get; set; }
    public DateTime PeriodStartUtc { get; set; }
    public DateTime PeriodEndUtc { get; set; }
    public int KpiCount { get; set; }
    public decimal? CompositeScore { get; set; }
    public decimal? TargetAchievementPercent { get; set; }
    public DateTime ComputedAtUtc { get; set; }
}

public sealed class AnalyticsSchoolRowDto
{
    public int SchoolAnalyticsID { get; set; }
    public int SchoolID { get; set; }
    public AnalyticsPeriodKind PeriodKind { get; set; }
    public DateTime PeriodStartUtc { get; set; }
    public DateTime PeriodEndUtc { get; set; }
    public int KpiCount { get; set; }
    public decimal? OverallScore { get; set; }
    public decimal? TargetAchievementPercent { get; set; }
    public DateTime ComputedAtUtc { get; set; }
}
