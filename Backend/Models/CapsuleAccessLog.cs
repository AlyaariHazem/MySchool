using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

public class CapsuleAccessLog
{
    public int CapsuleAccessLogID { get; set; }

    [Required]
    public int TimeCapsuleID { get; set; }

    [JsonIgnore]
    public TimeCapsule TimeCapsule { get; set; } = null!;

    [Required]
    [MaxLength(450)]
    public string AccessedByUserID { get; set; } = string.Empty;

    public DateTime AccessedAtUtc { get; set; } = DateTime.UtcNow;

    public CapsuleAccessActionType ActionType { get; set; } = CapsuleAccessActionType.Viewed;

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
