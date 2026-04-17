using System.ComponentModel.DataAnnotations;

namespace Backend.DTOS.School.Employees;

public class EmployeeQualificationDto
{
    public int? EmployeeQualificationID { get; set; }

    [Required]
    [MaxLength(256)]
    public string DegreeName { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? Major { get; set; }

    [MaxLength(256)]
    public string? Institution { get; set; }

    public int? GraduationYear { get; set; }

    [MaxLength(64)]
    public string? GradeOrScore { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
