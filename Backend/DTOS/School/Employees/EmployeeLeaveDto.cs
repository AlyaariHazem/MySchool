using System.ComponentModel.DataAnnotations;
using Backend.Models;

namespace Backend.DTOS.School.Employees;

public class EmployeeLeaveDto
{
    public int? EmployeeLeaveID { get; set; }

    [Required]
    public int AcademicYearID { get; set; }

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
