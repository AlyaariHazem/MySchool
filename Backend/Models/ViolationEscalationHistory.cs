using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Audit row when a case moves to another <see cref="ViolationKind"/> / <see cref="ViolationType"/>.</summary>
public class ViolationEscalationHistory
{
    public int ViolationEscalationHistoryID { get; set; }

    [Required]
    public int ViolationID { get; set; }

    [JsonIgnore]
    public Violation Violation { get; set; } = null!;

    public int? PreviousViolationTypeID { get; set; }

    [JsonIgnore]
    public ViolationType? PreviousViolationType { get; set; }

    [Required]
    public int NewViolationTypeID { get; set; }

    [JsonIgnore]
    public ViolationType NewViolationType { get; set; } = null!;

    [MaxLength(2000)]
    public string? Reason { get; set; }

    [Required]
    public int ChangedByEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile ChangedByEmployeeProfile { get; set; } = null!;

    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
}
