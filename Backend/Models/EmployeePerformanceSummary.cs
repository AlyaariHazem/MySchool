using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models;

public class EmployeePerformanceSummary
{
    public int EmployeePerformanceSummaryID { get; set; }

    [Required]
    public int EmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile EmployeeProfile { get; set; } = null!;

    [Required]
    public int AcademicYearID { get; set; }

    [JsonIgnore]
    public Year AcademicYear { get; set; } = null!;

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    public int? EmployeeJobTypeID { get; set; }

    [JsonIgnore]
    public EmployeeJobType? JobType { get; set; }

    [MaxLength(256)]
    public string? JobTitle { get; set; }

    [Column(TypeName = "decimal(18,2)")]
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

    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
}
