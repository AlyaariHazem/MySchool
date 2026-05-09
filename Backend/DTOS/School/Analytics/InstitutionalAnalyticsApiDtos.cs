using Backend.Models;

namespace Backend.DTOS.School.Analytics;

public sealed class AnalyticsGenerateRequestDto
{
    public int SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public AnalyticsPeriodKind PeriodKind { get; set; } = AnalyticsPeriodKind.Monthly;
    public bool ReplaceExistingForPeriod { get; set; } = true;
}

public sealed class AnalyticsGenerationResultDto
{
    public int SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public AnalyticsPeriodKind PeriodKind { get; set; }
    public DateTime PeriodStartUtc { get; set; }
    public DateTime PeriodEndUtc { get; set; }
    public int KpiDefinitionsEnsured { get; set; }
    public int KpiSnapshotsWritten { get; set; }
    public int SchoolAnalyticsWritten { get; set; }
    public int DepartmentAnalyticsWritten { get; set; }
    public int TeacherAnalyticsWritten { get; set; }
    public int TrendAnalysisWritten { get; set; }
    public IReadOnlyList<string> Messages { get; set; } = Array.Empty<string>();
}

public sealed class AnalyticsListQueryDto
{
    public int? SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public AnalyticsPeriodKind? PeriodKind { get; set; }
    public int Take { get; set; } = 200;
}

public sealed class KpiDefinitionListDto
{
    public int KpiDefinitionID { get; set; }
    public int SchoolID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? EnglishName { get; set; }
    public string? ArabicName { get; set; }
    public string? Description { get; set; }
    public int Category { get; set; }
    public int? TargetAudience { get; set; }
    public int CalculationType { get; set; }
    public string? Unit { get; set; }
    public bool HigherIsBetter { get; set; }
    public decimal? DefaultTargetValue { get; set; }
    public bool IsSystemKpi { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}

public sealed class YearComparisonQueryDto
{
    public int SchoolID { get; set; }
    public int CurrentYearID { get; set; }
    public int PreviousYearID { get; set; }
    public AnalyticsPeriodKind PeriodKind { get; set; } = AnalyticsPeriodKind.Yearly;
}

public sealed class YearComparisonResultDto
{
    public int SchoolID { get; set; }
    public int CurrentYearID { get; set; }
    public int PreviousYearID { get; set; }
    public AnalyticsPeriodKind PeriodKind { get; set; }
    public AnalyticsSchoolRowDto? Current { get; set; }
    public AnalyticsSchoolRowDto? Previous { get; set; }
    public decimal? OverallScoreDelta { get; set; }
    public int? ViolationsDelta { get; set; }
    public int? AchievementsDelta { get; set; }
    public int? ActivitiesDelta { get; set; }
    public int? ComplaintsDelta { get; set; }
}
