using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>ترشيح طالب لدورة جائزة.</summary>
public class AwardNomination
{
    public int AwardNominationID { get; set; }

    [Required]
    public int AwardCycleID { get; set; }

    [JsonIgnore]
    public AwardCycle AwardCycle { get; set; } = null!;

    [Required]
    public int StudentID { get; set; }

    [JsonIgnore]
    public Student Student { get; set; } = null!;

    public int? NominatedByEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile? NominatedByEmployeeProfile { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public AwardNominationStatus Status { get; set; } = AwardNominationStatus.Pending;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
