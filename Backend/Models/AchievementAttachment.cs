using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>File attached to an achievement request.</summary>
public class AchievementAttachment
{
    public int AchievementAttachmentID { get; set; }

    [Required]
    public int AchievementRequestID { get; set; }

    [JsonIgnore]
    public AchievementRequest AchievementRequest { get; set; } = null!;

    [Required]
    [MaxLength(512)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? ContentType { get; set; }

    [Required]
    [MaxLength(1024)]
    public string StoragePath { get; set; } = string.Empty;

    public long? FileSizeBytes { get; set; }

    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

    public int? UploadedByEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile? UploadedByEmployeeProfile { get; set; }
}
