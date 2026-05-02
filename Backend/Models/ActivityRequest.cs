using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Staff activity request (events, programs, participation).</summary>
public class ActivityRequest
{
    public int ActivityRequestID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [Required]
    public int AcademicYearID { get; set; }

    [JsonIgnore]
    public Year AcademicYear { get; set; } = null!;

    [Required]
    public int EmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile EmployeeProfile { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Details { get; set; }

    public ActivityRequestStatus Status { get; set; } = ActivityRequestStatus.Draft;

    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAtUtc { get; set; }

    public ICollection<ActivityApproval> Approvals { get; set; } = new List<ActivityApproval>();

    public ICollection<ActivityExecution> Executions { get; set; } = new List<ActivityExecution>();

    public ICollection<ActivityEvaluation> Evaluations { get; set; } = new List<ActivityEvaluation>();

    public ICollection<ActivityPoints> Points { get; set; } = new List<ActivityPoints>();
}
