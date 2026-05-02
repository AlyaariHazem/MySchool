using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Link between a meeting and an invited employee profile.</summary>
public class MeetingAttendee
{
    public int MeetingAttendeeID { get; set; }

    [Required]
    public int MeetingID { get; set; }

    [JsonIgnore]
    public Meeting Meeting { get; set; } = null!;

    [Required]
    public int EmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile EmployeeProfile { get; set; } = null!;

    public MeetingAttendeeRole Role { get; set; } = MeetingAttendeeRole.Required;

    public MeetingAttendeeResponse Response { get; set; } = MeetingAttendeeResponse.Pending;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
