namespace Backend.DTOS.School.Employees;

/// <summary>Zero-based <see cref="PageIndex"/>; same <see cref="Filter"/> rules as <c>POST /employees/list</c>. Used by <c>POST /employees/page</c> (id+name) and <c>POST /employees/list/page</c> (full rows).</summary>
public class EmployeeProfilePageRequestDto
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public EmployeeProfileListFilterDto? Filter { get; set; }
}
