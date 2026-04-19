namespace Backend.DTOS.School.Employees;

/// <summary>Paged employee list: <see cref="PageIndex"/> is zero-based; same filter rules as <c>POST /employees/list</c>.</summary>
public class EmployeeProfilePageRequestDto
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public EmployeeProfileListFilterDto? Filter { get; set; }
}
