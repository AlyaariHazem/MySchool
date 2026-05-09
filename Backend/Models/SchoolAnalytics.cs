using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>School-level institutional performance analytics.</summary>
public class SchoolAnalytics
{
    public int SchoolAnalyticsID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

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

    public decimal? OverallScore { get; set; }

    public decimal? AverageTeacherScore { get; set; }

    public int TotalViolations { get; set; }

    public int TotalAchievements { get; set; }

    public int TotalActivities { get; set; }

    public int TotalComplaints { get; set; }

    public decimal? PlanCompletionPercent { get; set; }

    public int EmployeeCount { get; set; }

    public int ActiveTeacherCount { get; set; }

    public AnalyticsRiskLevel RiskLevel { get; set; } = AnalyticsRiskLevel.Low;

    public decimal? TargetAchievementPercent { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public DateTime ComputedAtUtc { get; set; } = DateTime.UtcNow;
}
