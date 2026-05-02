using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Long-horizon strategic goal for a school (distinct from academic <see cref="CoursePlan"/>).</summary>
public class StrategicGoal
{
    public int StrategicGoalID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [MaxLength(64)]
    public string? ReferenceCode { get; set; }

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Details { get; set; }

    public StrategicGoalStatus Status { get; set; } = StrategicGoalStatus.Draft;

    public int SortOrder { get; set; }

    public DateTime? EffectiveFromUtc { get; set; }

    public DateTime? EffectiveToUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<AnnualGoal> AnnualGoals { get; set; } = new List<AnnualGoal>();

    public ICollection<DepartmentGoal> DepartmentGoals { get; set; } = new List<DepartmentGoal>();
}
