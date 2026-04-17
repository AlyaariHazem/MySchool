using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>
/// End-of-day lock for a school (and academic year) on a calendar date.
/// When <see cref="DailyEvaluationTemplateID"/> is null, all daily evaluations for that date are considered locked.
/// When set, only evaluations using that template are locked (optional finer scope).
/// </summary>
public class EvaluationLock
{
    public int EvaluationLockID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [Required]
    public int AcademicYearID { get; set; }

    [JsonIgnore]
    public Year AcademicYear { get; set; } = null!;

    [Required]
    public DateOnly LockDate { get; set; }

    public int? DailyEvaluationTemplateID { get; set; }

    [JsonIgnore]
    public DailyEvaluationTemplate? Template { get; set; }

    public EvaluationLockStatus Status { get; set; } = EvaluationLockStatus.Open;

    public DateTime? LockedAtUtc { get; set; }

    [MaxLength(450)]
    public string? LockedByUserId { get; set; }

    public DateTime? ReopenedAtUtc { get; set; }

    [MaxLength(450)]
    public string? ReopenedByUserId { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<EvaluationOverrideLog> OverrideLogs { get; set; } = new List<EvaluationOverrideLog>();
}
