using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Computed trend analysis result for a KPI and dashboard audience.</summary>
public class TrendAnalysis
{
    public int TrendAnalysisID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [Required]
    public int KpiDefinitionID { get; set; }

    [JsonIgnore]
    public KpiDefinition KpiDefinition { get; set; } = null!;

    public int? AcademicYearID { get; set; }

    [JsonIgnore]
    public Year? AcademicYear { get; set; }

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

    [MaxLength(64)]
    public string? TrendLabel { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public DateTime ComputedAtUtc { get; set; } = DateTime.UtcNow;
}
