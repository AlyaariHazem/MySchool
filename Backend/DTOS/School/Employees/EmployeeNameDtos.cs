using System.ComponentModel.DataAnnotations;

namespace Backend.DTOS.School.Employees;

public class EmployeeNameDto
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    public string? MiddleName { get; set; }

    [Required]
    public string LastName { get; set; } = string.Empty;
}

public class EmployeeNameAlisDto
{
    public string? FirstNameEng { get; set; }
    public string? MiddleNameEng { get; set; }
    public string? LastNameEng { get; set; }
}
