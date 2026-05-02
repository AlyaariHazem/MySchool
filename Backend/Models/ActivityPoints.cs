using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Points awarded (or adjusted) for an activity request.</summary>
public class ActivityPoints
{
    public int ActivityPointsID { get; set; }

    [Required]
    public int ActivityRequestID { get; set; }

    [JsonIgnore]
    public ActivityRequest ActivityRequest { get; set; } = null!;

    [Required]
    public int Points { get; set; }

    [MaxLength(1000)]
    public string? Reason { get; set; }

    [Required]
    public int AwardedByEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile AwardedByEmployeeProfile { get; set; } = null!;

    public DateTime AwardedAtUtc { get; set; } = DateTime.UtcNow;
}
