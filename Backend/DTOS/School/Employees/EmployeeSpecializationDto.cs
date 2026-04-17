using System.ComponentModel.DataAnnotations;

namespace Backend.DTOS.School.Employees;

public class EmployeeSpecializationDto
{
    public int? EmployeeSpecializationID { get; set; }

    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? Category { get; set; }

    [MaxLength(64)]
    public string? Level { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
