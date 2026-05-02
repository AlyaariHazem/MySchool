using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Department / unit goal (organizational performance), optionally aligned to strategic or annual goals.</summary>
public class DepartmentGoal
{
    public int DepartmentGoalID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    public int? AcademicYearID { get; set; }

    [JsonIgnore]
    public Year? AcademicYear { get; set; }

    public int? StrategicGoalID { get; set; }

    [JsonIgnore]
    public StrategicGoal? StrategicGoal { get; set; }

    public int? AnnualGoalID { get; set; }

    [JsonIgnore]
    public AnnualGoal? AnnualGoal { get; set; }

    /// <summary>Department or unit label (no separate Department master table in this schema).</summary>
    [Required]
    [MaxLength(256)]
    public string DepartmentName { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Details { get; set; }

    public DepartmentGoalStatus Status { get; set; } = DepartmentGoalStatus.Draft;

    public int SortOrder { get; set; }

    public int? OwnerEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile? OwnerEmployeeProfile { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
