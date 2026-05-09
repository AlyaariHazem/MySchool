using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

public class TimeCapsuleSection
{
    public int TimeCapsuleSectionID { get; set; }

    [Required]
    public int TimeCapsuleID { get; set; }

    [JsonIgnore]
    public TimeCapsule TimeCapsule { get; set; } = null!;

    public TimeCapsuleSectionType SectionType { get; set; }

    [Required]
    [MaxLength(512)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Immutable JSON snapshot (camelCase) produced at unlock.</summary>
    public string DataJson { get; set; } = "{}";

    public int SortOrder { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
