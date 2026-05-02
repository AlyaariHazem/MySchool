using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Tracks implementation/follow-up updates for a request.</summary>
public class RequestExecution
{
    public int RequestExecutionID { get; set; }

    [Required]
    public int EmployeeRequestID { get; set; }

    [JsonIgnore]
    public EmployeeRequest EmployeeRequest { get; set; } = null!;

    [Required]
    public RequestExecutionStatus Status { get; set; } = RequestExecutionStatus.Pending;

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
