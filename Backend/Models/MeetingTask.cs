using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Action item captured from a meeting.</summary>
public class MeetingTask
{
    public int MeetingTaskID { get; set; }

    [Required]
    public int MeetingID { get; set; }

    [JsonIgnore]
    public Meeting Meeting { get; set; } = null!;

    [Required]
    [MaxLength(512)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Details { get; set; }

    public int? AssignedToEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile? AssignedToEmployeeProfile { get; set; }

    public DateTime? DueAtUtc { get; set; }

    public MeetingTaskStatus Status { get; set; } = MeetingTaskStatus.Open;

    public int SortOrder { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<MeetingTaskFollowUp> FollowUps { get; set; } = new List<MeetingTaskFollowUp>();
}
