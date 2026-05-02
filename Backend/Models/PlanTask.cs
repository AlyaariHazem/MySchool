using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Action item under an <see cref="OperationalPlan"/>.</summary>
public class PlanTask
{
    public int PlanTaskID { get; set; }

    [Required]
    public int OperationalPlanID { get; set; }

    [JsonIgnore]
    public OperationalPlan OperationalPlan { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Details { get; set; }

    public PlanTaskStatus Status { get; set; } = PlanTaskStatus.NotStarted;

    public int SortOrder { get; set; }

    /// <summary>Rolled-up completion 0–100 (denormalized for convenience).</summary>
    public int ProgressPercent { get; set; }

    public DateTime? DueAtUtc { get; set; }

    public int? AssignedToEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile? AssignedToEmployeeProfile { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<PlanProgressUpdate> ProgressUpdates { get; set; } = new List<PlanProgressUpdate>();
}
