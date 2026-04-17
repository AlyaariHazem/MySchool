using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

public class EmployeeLeave
{
    public int EmployeeLeaveID { get; set; }

    [Required]
    public int EmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile EmployeeProfile { get; set; } = null!;

    [Required]
    public int AcademicYearID { get; set; }

    [JsonIgnore]
    public Year AcademicYear { get; set; } = null!;

    public LeaveType LeaveType { get; set; } = LeaveType.Other;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public decimal TotalDays { get; set; }

    [MaxLength(2000)]
    public string? Reason { get; set; }

    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

    [MaxLength(450)]
    public string? ApprovedByUserId { get; set; }

    public DateTime? ApprovedAtUtc { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
