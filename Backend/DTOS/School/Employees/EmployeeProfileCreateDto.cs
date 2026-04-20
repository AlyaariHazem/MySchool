using System.ComponentModel.DataAnnotations;
using Backend.Models;

namespace Backend.DTOS.School.Employees;

public class EmployeeProfileCreateDto
{
    [MaxLength(450)]
    public string? UserId { get; set; }

    /// <summary>Optional when the caller is a school manager — resolved from the manager's school and active year.</summary>
    public int SchoolID { get; set; }

    /// <summary>Optional when the caller is a school manager — resolved from the manager's school and active year.</summary>
    public int CurrentAcademicYearID { get; set; }

    [Required]
    public int EmployeeJobTypeID { get; set; }

    /// <summary>When null or whitespace, the server assigns the next numeric code for the school.</summary>
    [MaxLength(64)]
    public string? EmployeeCode { get; set; }

    [Required]
    public EmployeeNameDto FullName { get; set; } = null!;

    public EmployeeNameAlisDto? FullNameAlis { get; set; }

    [MaxLength(64)]
    public string? NationalId { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(32)]
    public string? Gender { get; set; }

    [MaxLength(64)]
    public string? Phone { get; set; }

    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(512)]
    public string? Address { get; set; }

    public DateTime? HireDate { get; set; }

    public EmploymentStatus EmploymentStatus { get; set; } = EmploymentStatus.Active;

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public int? TeacherID { get; set; }
    public int? ManagerID { get; set; }
    public int? SchoolStaffID { get; set; }
}
