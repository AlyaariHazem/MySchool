namespace Backend.DTOS.School.Employee;

/// <summary>
/// Copy selected staff into a target school year as Active without duplicating teacher/manager rows.
/// </summary>
public class EmployeeYearRolloverRequestDTO
{
    public int SourceYearId { get; set; }
    public int TargetYearId { get; set; }
    public List<int>? TeacherIds { get; set; }
    public List<int>? ManagerIds { get; set; }
    public List<int>? SchoolStaffIds { get; set; }
}
