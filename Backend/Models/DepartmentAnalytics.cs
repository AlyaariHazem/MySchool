using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Aggregated department/unit performance analytics for a given time period.</summary>
public class DepartmentAnalytics
{
    public int DepartmentAnalyticsID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    public int? EmployeeJobTypeID { get; set; }

    [JsonIgnore]
    public EmployeeJobType? EmployeeJobType { get; set; }

    [Required]
    [MaxLength(256)]
    public string DepartmentName { get; set; } = string.Empty;

    public int? AcademicYearID { get; set; }

    [JsonIgnore]
    public Year? AcademicYear { get; set; }

    public int? TermID { get; set; }

    [JsonIgnore]
    public Term? Term { get; set; }

    public AnalyticsPeriodKind PeriodKind { get; set; } = AnalyticsPeriodKind.Monthly;

    public DateTime PeriodStartUtc { get; set; }

    public DateTime PeriodEndUtc { get; set; }

    public int KpiCount { get; set; }

    /// <summary>Average daily evaluation total score for the department in the period.</summary>
    public decimal? AverageScore { get; set; }

    public decimal? TargetAchievementPercent { get; set; }

    public int ViolationCount { get; set; }

    public int AchievementCount { get; set; }

    public int ActivityCount { get; set; }

    public int ComplaintCount { get; set; }

    /// <summary>Organizational plan completion — pending until plan engine exposes completion metrics.</summary>
    public decimal? PlanCompletionPercent { get; set; }

    public int EmployeeCount { get; set; }

    [MaxLength(64)]
    public string? PerformanceLevel { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public DateTime ComputedAtUtc { get; set; } = DateTime.UtcNow;
}
