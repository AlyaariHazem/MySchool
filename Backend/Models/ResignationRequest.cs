using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

public class ResignationRequest
{
    public int ResignationRequestID { get; set; }

    [Required]
    public int EmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile EmployeeProfile { get; set; } = null!;

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [Required]
    public int AcademicYearID { get; set; }

    [JsonIgnore]
    public Year AcademicYear { get; set; } = null!;

    public DateTime RequestDateUtc { get; set; } = DateTime.UtcNow;

    [MaxLength(4000)]
    public string? Reason { get; set; }

    public ResignationRequestStatus Status { get; set; } = ResignationRequestStatus.Pending;

    [MaxLength(450)]
    public string? ApprovedByUserID { get; set; }

    public DateTime? ApprovedAtUtc { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<CapsuleUnlockApproval> CapsuleUnlockApprovals { get; set; } = new List<CapsuleUnlockApproval>();
}
