using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>One approval step on an achievement request.</summary>
public class AchievementApproval
{
    public int AchievementApprovalID { get; set; }

    [Required]
    public int AchievementRequestID { get; set; }

    [JsonIgnore]
    public AchievementRequest AchievementRequest { get; set; } = null!;

    [Required]
    public int ApproverEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile ApproverEmployeeProfile { get; set; } = null!;

    public AchievementApprovalDecision Decision { get; set; } = AchievementApprovalDecision.Pending;

    [MaxLength(2000)]
    public string? Comment { get; set; }

    public int SortOrder { get; set; }

    public DateTime? DecidedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
