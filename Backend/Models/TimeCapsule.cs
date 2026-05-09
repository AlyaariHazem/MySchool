using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Historical archive for an employee; locked until resignation and unlock approvals complete.</summary>
public class TimeCapsule
{
    public int TimeCapsuleID { get; set; }

    [Required]
    public int EmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile EmployeeProfile { get; set; } = null!;

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    public bool IsLocked { get; set; } = true;

    public DateTime? UnlockedAtUtc { get; set; }

    [MaxLength(450)]
    public string? UnlockedByUserID { get; set; }

    [MaxLength(2000)]
    public string? UnlockReason { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public ICollection<TimeCapsuleSection> Sections { get; set; } = new List<TimeCapsuleSection>();

    public ICollection<CapsuleUnlockApproval> UnlockApprovals { get; set; } = new List<CapsuleUnlockApproval>();

    public ICollection<CapsuleNarrative> Narratives { get; set; } = new List<CapsuleNarrative>();

    public ICollection<CapsuleAccessLog> AccessLogs { get; set; } = new List<CapsuleAccessLog>();
}
