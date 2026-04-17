using Backend.Models;

namespace Backend.DTOS.School.Employees;

public class EmployeeProfileReadDto
{
    public int EmployeeProfileID { get; set; }
    public string? UserId { get; set; }
    public int SchoolID { get; set; }
    public int CurrentAcademicYearID { get; set; }
    public int EmployeeJobTypeID { get; set; }
    public string JobTypeCode { get; set; } = string.Empty;
    public string JobTypeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public EmployeeNameDto FullName { get; set; } = null!;
    public EmployeeNameAlisDto? FullNameAlis { get; set; }
    public string? NationalId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime? HireDate { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public int? TeacherID { get; set; }
    public int? ManagerID { get; set; }
    public int? SchoolStaffID { get; set; }
}
