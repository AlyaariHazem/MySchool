using System.ComponentModel.DataAnnotations;
using Backend.Models;

namespace Backend.DTOS.School.Employees;

public class EmployeeProfileUpdateDto
{
    /// <summary>Omit or use 0 to keep the employee's existing school.</summary>
    public int SchoolID { get; set; }

    /// <summary>Omit or use 0 to keep the employee's existing academic year.</summary>
    public int CurrentAcademicYearID { get; set; }

    [Required]
    public int EmployeeJobTypeID { get; set; }

    /// <summary>Omit or whitespace to keep the existing employee code.</summary>
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

    public EmploymentStatus EmploymentStatus { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(450)]
    public string? UserId { get; set; }

    public int? TeacherID { get; set; }
    public int? ManagerID { get; set; }
    public int? SchoolStaffID { get; set; }
}
