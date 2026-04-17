using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Audit trail for post-lock edits or day unlock operations.</summary>
public class EvaluationOverrideLog
{
    public int EvaluationOverrideLogID { get; set; }

    public int? DailyEvaluationID { get; set; }

    [JsonIgnore]
    public DailyEvaluation? DailyEvaluation { get; set; }

    public int? EvaluationLockID { get; set; }

    [JsonIgnore]
    public EvaluationLock? EvaluationLock { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [Required]
    public int AcademicYearID { get; set; }

    [JsonIgnore]
    public Year AcademicYear { get; set; } = null!;

    /// <summary>Stored as int.</summary>
    public EvaluationOverrideActionType OverrideActionType { get; set; }

    [MaxLength(2000)]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(8000)]
    public string? PreviousValuesJson { get; set; }

    [MaxLength(8000)]
    public string? NewValuesJson { get; set; }

    [Required]
    [MaxLength(450)]
    public string PerformedByUserId { get; set; } = string.Empty;

    public DateTime PerformedAtUtc { get; set; } = DateTime.UtcNow;

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
