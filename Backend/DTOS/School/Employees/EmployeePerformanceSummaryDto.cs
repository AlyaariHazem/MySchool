using System.ComponentModel.DataAnnotations;

namespace Backend.DTOS.School.Employees;

public class EmployeePerformanceSummaryDto
{
    public int? EmployeePerformanceSummaryID { get; set; }

    [Required]
    public int AcademicYearID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    public int? EmployeeJobTypeID { get; set; }

    [MaxLength(256)]
    public string? JobTitle { get; set; }

    public decimal? EvaluationScore { get; set; }

    public int AchievementPoints { get; set; }

    public int ViolationPoints { get; set; }

    public int RequestCount { get; set; }

    public int ActivityCount { get; set; }

    [MaxLength(64)]
    public string? PerformanceLevel { get; set; }

    [MaxLength(4000)]
    public string? StrengthsSummary { get; set; }

    [MaxLength(4000)]
    public string? WeaknessesSummary { get; set; }

    [MaxLength(4000)]
    public string? Recommendations { get; set; }

    [MaxLength(4000)]
    public string? FinalNotes { get; set; }

    public DateTime? GeneratedAtUtc { get; set; }
}
