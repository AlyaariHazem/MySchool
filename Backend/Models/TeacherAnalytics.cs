using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Aggregated teacher performance analytics for a given time period.</summary>
public class TeacherAnalytics
{
    public int TeacherAnalyticsID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [Required]
    public int EmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile EmployeeProfile { get; set; } = null!;

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

    public decimal? CompositeScore { get; set; }

    public decimal? AverageDailyEvaluationScore { get; set; }

    public decimal? SupervisorVisitAverage { get; set; }

    public int AchievementPoints { get; set; }

    public int ViolationPoints { get; set; }

    public int ActivityCount { get; set; }

    public int ComplaintCount { get; set; }

    /// <summary>Reserved for attendance/discipline composite when a source exists.</summary>
    public decimal? AttendanceOrDisciplineScore { get; set; }

    public AnalyticsTrendDirection TrendDirection { get; set; } = AnalyticsTrendDirection.Unknown;

    [MaxLength(64)]
    public string? PerformanceLevel { get; set; }

    [MaxLength(4000)]
    public string? StrengthsSummary { get; set; }

    [MaxLength(4000)]
    public string? WeaknessesSummary { get; set; }

    [MaxLength(4000)]
    public string? Recommendations { get; set; }

    public decimal? TargetAchievementPercent { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public DateTime ComputedAtUtc { get; set; } = DateTime.UtcNow;
}
