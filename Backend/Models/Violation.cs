using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Staff violation case, tied to the current escalation <see cref="ViolationType"/>.</summary>
public class Violation
{
    public int ViolationID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    public int? AcademicYearID { get; set; }

    [JsonIgnore]
    public Year? AcademicYear { get; set; }

    /// <summary>Current step on the ladder (clarification → … → final warning).</summary>
    [Required]
    public int ViolationTypeID { get; set; }

    [JsonIgnore]
    public ViolationType ViolationType { get; set; } = null!;

    [Required]
    public int SubjectEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile SubjectEmployeeProfile { get; set; } = null!;

    public int? OpenedByEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile? OpenedByEmployeeProfile { get; set; }

    [Required]
    [MaxLength(512)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Details { get; set; }

    public ViolationStatus Status { get; set; } = ViolationStatus.Draft;

    public DateTime OpenedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ClosedAtUtc { get; set; }

    public ICollection<ViolationResponse> Responses { get; set; } = new List<ViolationResponse>();

    public ICollection<ViolationAction> Actions { get; set; } = new List<ViolationAction>();

    public ICollection<ViolationEscalationHistory> EscalationHistory { get; set; } = new List<ViolationEscalationHistory>();
}
