using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>
/// Per-school catalog row for one <see cref="ViolationKind"/> (seeded four rows per school).
/// </summary>
public class ViolationType
{
    public int ViolationTypeID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    public ViolationKind Kind { get; set; }

    /// <summary>Display name (e.g. Arabic); seeded from migration, can be edited per school later.</summary>
    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Cases currently at this escalation type.</summary>
    public ICollection<Violation> Violations { get; set; } = new List<Violation>();

    public ICollection<ViolationEscalationHistory> EscalationHistoriesFrom { get; set; } = new List<ViolationEscalationHistory>();

    public ICollection<ViolationEscalationHistory> EscalationHistoriesTo { get; set; } = new List<ViolationEscalationHistory>();
}
