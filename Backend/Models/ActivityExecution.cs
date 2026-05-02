using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Implementation / follow-up update for an activity request.</summary>
public class ActivityExecution
{
    public int ActivityExecutionID { get; set; }

    [Required]
    public int ActivityRequestID { get; set; }

    [JsonIgnore]
    public ActivityRequest ActivityRequest { get; set; } = null!;

    [Required]
    public ActivityExecutionStatus Status { get; set; } = ActivityExecutionStatus.Pending;

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public int ProgressPercent { get; set; }

    public DateTime? DueAtUtc { get; set; }

    public DateTime? ExecutedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public int? ResponsibleEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile? ResponsibleEmployeeProfile { get; set; }
}
