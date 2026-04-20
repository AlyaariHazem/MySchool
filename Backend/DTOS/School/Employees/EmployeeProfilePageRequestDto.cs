using Backend.Common;

namespace Backend.DTOS.School.Employees;

/// <summary>Same <see cref="Filter"/> rules as <c>POST /employees/list</c>. Used by <c>POST /employees/page</c> (id+name) and <c>POST /employees/list/page</c> (full rows).</summary>
public class EmployeeProfilePageRequestDto : PageRequestDto
{
    public EmployeeProfileListFilterDto? Filter { get; set; }
}
