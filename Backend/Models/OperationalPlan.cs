using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Operational plan implementing part of an <see cref="AnnualGoal"/>.</summary>
public class OperationalPlan
{
    public int OperationalPlanID { get; set; }

    [Required]
    public int AnnualGoalID { get; set; }

    [JsonIgnore]
    public AnnualGoal AnnualGoal { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Details { get; set; }

    public OperationalPlanStatus Status { get; set; } = OperationalPlanStatus.Draft;

    public int SortOrder { get; set; }

    public DateTime? StartDateUtc { get; set; }

    public DateTime? EndDateUtc { get; set; }

    public int? OwnerEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile? OwnerEmployeeProfile { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<PlanTask> Tasks { get; set; } = new List<PlanTask>();
}
