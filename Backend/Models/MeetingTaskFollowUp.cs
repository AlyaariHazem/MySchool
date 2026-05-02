using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Progress note on a meeting action item.</summary>
public class MeetingTaskFollowUp
{
    public int MeetingTaskFollowUpID { get; set; }

    [Required]
    public int MeetingTaskID { get; set; }

    [JsonIgnore]
    public MeetingTask MeetingTask { get; set; } = null!;

    public int? AuthorEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile? AuthorEmployeeProfile { get; set; }

    [Required]
    [MaxLength(4000)]
    public string Note { get; set; } = string.Empty;

    public int? ProgressPercent { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
