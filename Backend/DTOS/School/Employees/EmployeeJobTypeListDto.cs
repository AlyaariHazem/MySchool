namespace Backend.DTOS.School.Employees;

/// <summary>Lookup row for HR job type dropdowns (read-only).</summary>
public class EmployeeJobTypeListDto
{
    public int EmployeeJobTypeID { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? NameAr { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }
}
