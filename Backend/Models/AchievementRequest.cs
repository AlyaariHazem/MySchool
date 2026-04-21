using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Employee achievement request (catalog item or free-form title).</summary>
public class AchievementRequest
{
    public int AchievementRequestID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [Required]
    public int AcademicYearID { get; set; }

    [JsonIgnore]
    public Year AcademicYear { get; set; } = null!;

    [Required]
    public int EmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile EmployeeProfile { get; set; } = null!;

    /// <summary>When null, the request is free-form and <see cref="CustomTitle"/> is used.</summary>
    public int? AchievementID { get; set; }

    [JsonIgnore]
    public Achievement? Achievement { get; set; }

    [MaxLength(256)]
    public string? CustomTitle { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public AchievementRequestStatus Status { get; set; } = AchievementRequestStatus.Draft;

    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAtUtc { get; set; }

    public ICollection<AchievementApproval> Approvals { get; set; } = new List<AchievementApproval>();

    public ICollection<AchievementAttachment> Attachments { get; set; } = new List<AchievementAttachment>();
}
