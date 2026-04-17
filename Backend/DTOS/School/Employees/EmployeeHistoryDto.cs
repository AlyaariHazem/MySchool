using System.ComponentModel.DataAnnotations;

namespace Backend.DTOS.School.Employees;

public class EmployeeHistoryDto
{
    public int? EmployeeHistoryID { get; set; }

    [Required]
    public int AcademicYearID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    public int? EmployeeJobTypeID { get; set; }

    [MaxLength(256)]
    public string? JobTitle { get; set; }

    [MaxLength(256)]
    public string? Department { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [MaxLength(64)]
    public string? Status { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
