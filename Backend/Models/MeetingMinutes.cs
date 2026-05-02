using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Official minutes for a single meeting (0..1 per meeting).</summary>
public class MeetingMinutes
{
    public int MeetingMinutesID { get; set; }

    [Required]
    public int MeetingID { get; set; }

    [JsonIgnore]
    public Meeting Meeting { get; set; } = null!;

    [Required]
    public string Body { get; set; } = string.Empty;

    [Required]
    public int RecordedByEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile RecordedByEmployeeProfile { get; set; } = null!;

    public DateTime RecordedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public int? ApprovedByEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile? ApprovedByEmployeeProfile { get; set; }

    public DateTime? ApprovedAtUtc { get; set; }
}
