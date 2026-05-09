using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

public class CapsuleUnlockApproval
{
    public int CapsuleUnlockApprovalID { get; set; }

    [Required]
    public int TimeCapsuleID { get; set; }

    [JsonIgnore]
    public TimeCapsule TimeCapsule { get; set; } = null!;

    [Required]
    public int ResignationRequestID { get; set; }

    [JsonIgnore]
    public ResignationRequest ResignationRequest { get; set; } = null!;

    public CapsuleUnlockApprovalStatus Status { get; set; } = CapsuleUnlockApprovalStatus.Pending;

    [MaxLength(450)]
    public string? ApprovedByUserID { get; set; }

    public DateTime? ApprovedAtUtc { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
