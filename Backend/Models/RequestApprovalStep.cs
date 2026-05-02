using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>One step in the employee request approval chain.</summary>
public class RequestApprovalStep
{
    public int RequestApprovalStepID { get; set; }

    [Required]
    public int EmployeeRequestID { get; set; }

    [JsonIgnore]
    public EmployeeRequest EmployeeRequest { get; set; } = null!;

    [Required]
    public int ApproverEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile ApproverEmployeeProfile { get; set; } = null!;

    public int StepOrder { get; set; }

    public RequestApprovalDecision Decision { get; set; } = RequestApprovalDecision.Pending;

    [MaxLength(2000)]
    public string? Comment { get; set; }

    public DateTime? DecidedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
