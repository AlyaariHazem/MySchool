using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

public class CapsuleNarrative
{
    public int CapsuleNarrativeID { get; set; }

    [Required]
    public int TimeCapsuleID { get; set; }

    [JsonIgnore]
    public TimeCapsule TimeCapsule { get; set; } = null!;

    [MaxLength(8000)]
    public string NarrativeText { get; set; } = string.Empty;

    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;

    public CapsuleNarrativeGeneratedBy GeneratedBy { get; set; } = CapsuleNarrativeGeneratedBy.System;
}
