using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Annual performance goal for a school year; optionally cascades from a <see cref="StrategicGoal"/>.</summary>
public class AnnualGoal
{
    public int AnnualGoalID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [Required]
    public int AcademicYearID { get; set; }

    [JsonIgnore]
    public Year AcademicYear { get; set; } = null!;

    public int? StrategicGoalID { get; set; }

    [JsonIgnore]
    public StrategicGoal? StrategicGoal { get; set; }

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Details { get; set; }

    public AnnualGoalStatus Status { get; set; } = AnnualGoalStatus.Draft;

    public int SortOrder { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<OperationalPlan> OperationalPlans { get; set; } = new List<OperationalPlan>();

    public ICollection<DepartmentGoal> DepartmentGoals { get; set; } = new List<DepartmentGoal>();
}
