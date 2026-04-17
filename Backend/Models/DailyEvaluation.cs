using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>
/// One daily evaluation instance for an employee on a calendar date using a specific template.
/// Uniqueness: one row per (evaluated employee, evaluation date, template).
/// </summary>
public class DailyEvaluation
{
    public int DailyEvaluationID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [Required]
    public int AcademicYearID { get; set; }

    [JsonIgnore]
    public Year AcademicYear { get; set; } = null!;

    [Required]
    public int EvaluatedEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile EvaluatedEmployeeProfile { get; set; } = null!;

    /// <summary>Identity user id (admin DB) of the evaluator when present.</summary>
    [MaxLength(450)]
    public string? EvaluatorUserId { get; set; }

    public int? EvaluatorEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile? EvaluatorEmployeeProfile { get; set; }

    [Required]
    public int DailyEvaluationTemplateID { get; set; }

    [JsonIgnore]
    public DailyEvaluationTemplate Template { get; set; } = null!;

    [Required]
    public DateOnly EvaluationDate { get; set; }

    public DailyEvaluationStatus Status { get; set; } = DailyEvaluationStatus.Draft;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalScore { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public DateTime? SubmittedAtUtc { get; set; }

    public DateTime? LockedAtUtc { get; set; }

    public bool IsLocked { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<DailyEvaluationItem> Items { get; set; } = new List<DailyEvaluationItem>();

    public ICollection<EvaluationOverrideLog> OverrideLogs { get; set; } = new List<EvaluationOverrideLog>();
}
