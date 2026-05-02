using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Main employee request record (tools, advance, support).</summary>
public class EmployeeRequest
{
    public int EmployeeRequestID { get; set; }

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
    public int RequestTypeID { get; set; }

    [JsonIgnore]
    public RequestType RequestType { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Details { get; set; }

    public decimal? RequestedAmount { get; set; }

    public EmployeeRequestStatus Status { get; set; } = EmployeeRequestStatus.Draft;

    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAtUtc { get; set; }

    public ICollection<RequestApprovalStep> ApprovalSteps { get; set; } = new List<RequestApprovalStep>();

    public ICollection<RequestExecution> Executions { get; set; } = new List<RequestExecution>();

    public ICollection<RequestDailySummary> DailySummaries { get; set; } = new List<RequestDailySummary>();
}
