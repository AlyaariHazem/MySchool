using Backend.Models;

namespace Backend.DTOS.School.Employees;

public class EmployeeProfileListFilterDto
{
    public int? SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public int? EmployeeJobTypeID { get; set; }
    public bool? IsActive { get; set; }
    public EmploymentStatus? EmploymentStatus { get; set; }
}
