using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Evaluation / rating recorded for an activity request.</summary>
public class ActivityEvaluation
{
    public int ActivityEvaluationID { get; set; }

    [Required]
    public int ActivityRequestID { get; set; }

    [JsonIgnore]
    public ActivityRequest ActivityRequest { get; set; } = null!;

    [Required]
    public int EvaluatorEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile EvaluatorEmployeeProfile { get; set; } = null!;

    /// <summary>Score on a 1–5 scale.</summary>
    public int Score { get; set; }

    [MaxLength(4000)]
    public string? Feedback { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
