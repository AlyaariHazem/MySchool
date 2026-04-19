namespace Backend.DTOS.School.Employees;

/// <summary>Minimal row for dropdowns: id + Arabic/structured name.</summary>
public class EmployeeProfileOptionDto
{
    public int Id { get; set; }
    public EmployeeNameDto FullName { get; set; } = null!;
}
