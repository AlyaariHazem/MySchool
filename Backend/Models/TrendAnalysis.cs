using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Computed trend analysis result for a KPI, metric, or entity.</summary>
public class TrendAnalysis
{
    public int TrendAnalysisID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    public int? KpiDefinitionID { get; set; }

    [JsonIgnore]
    public KpiDefinition? KpiDefinition { get; set; }

    public int? AcademicYearID { get; set; }

    [JsonIgnore]
    public Year? AcademicYear { get; set; }

    public AnalyticsEntityType EntityType { get; set; } = AnalyticsEntityType.None;

    public int? EntityID { get; set; }

    [MaxLength(128)]
    public string? MetricCode { get; set; }

    [MaxLength(256)]
    public string? DepartmentName { get; set; }

    public int? EmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile? EmployeeProfile { get; set; }

    public DashboardAudience DashboardAudience { get; set; } = DashboardAudience.School;

    public AnalyticsPeriodKind PeriodKind { get; set; } = AnalyticsPeriodKind.Monthly;

    public DateTime FromUtc { get; set; }

    public DateTime ToUtc { get; set; }

    public decimal? BaselineValue { get; set; }

    public decimal? CurrentValue { get; set; }

    public decimal? DeltaValue { get; set; }

    public decimal? DeltaPercent { get; set; }

    public bool IsPositiveTrend { get; set; }

    public AnalyticsTrendDirection TrendDirection { get; set; } = AnalyticsTrendDirection.Unknown;

    [MaxLength(64)]
    public string? TrendLabel { get; set; }

    [MaxLength(4000)]
    public string? Interpretation { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public DateTime ComputedAtUtc { get; set; } = DateTime.UtcNow;
}
