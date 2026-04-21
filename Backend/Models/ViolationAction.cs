using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Recorded administrative action (meeting, note, issued letter, etc.).</summary>
public class ViolationAction
{
    public int ViolationActionID { get; set; }

    [Required]
    public int ViolationID { get; set; }

    [JsonIgnore]
    public Violation Violation { get; set; } = null!;

    public ViolationActionCategory Category { get; set; } = ViolationActionCategory.GeneralNote;

    [Required]
    [MaxLength(512)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Notes { get; set; }

    [Required]
    public int PerformedByEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile PerformedByEmployeeProfile { get; set; } = null!;

    public DateTime PerformedAtUtc { get; set; } = DateTime.UtcNow;
}
