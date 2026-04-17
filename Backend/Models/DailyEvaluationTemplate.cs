using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>
/// Configurable template for daily employee evaluations (per school / academic year; optional job type scope).
/// </summary>
public class DailyEvaluationTemplate
{
    public int DailyEvaluationTemplateID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [Required]
    public int AcademicYearID { get; set; }

    [JsonIgnore]
    public Year AcademicYear { get; set; } = null!;

    /// <summary>When set, template applies only to this job type; when null, any job type may use it if business rules allow.</summary>
    public int? EmployeeJobTypeID { get; set; }

    [JsonIgnore]
    public EmployeeJobType? JobType { get; set; }

    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    public EvaluationTemplateStatus Status { get; set; } = EvaluationTemplateStatus.Draft;

    public DateOnly? EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<DailyEvaluationCriteria> Criteria { get; set; } = new List<DailyEvaluationCriteria>();

    public ICollection<DailyEvaluation> DailyEvaluations { get; set; } = new List<DailyEvaluation>();
}
