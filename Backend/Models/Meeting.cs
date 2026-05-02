using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>School staff meeting (scheduled or ad-hoc).</summary>
public class Meeting
{
    public int MeetingID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [Required]
    public int AcademicYearID { get; set; }

    [JsonIgnore]
    public Year AcademicYear { get; set; } = null!;

    [Required]
    public int OrganizerEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile OrganizerEmployeeProfile { get; set; } = null!;

    [Required]
    [MaxLength(512)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    [MaxLength(256)]
    public string? Location { get; set; }

    public DateTime StartAtUtc { get; set; }

    public DateTime? EndAtUtc { get; set; }

    public MeetingStatus Status { get; set; } = MeetingStatus.Draft;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<MeetingAttendee> Attendees { get; set; } = new List<MeetingAttendee>();

    public MeetingMinutes? Minutes { get; set; }

    public ICollection<MeetingTask> Tasks { get; set; } = new List<MeetingTask>();
}
