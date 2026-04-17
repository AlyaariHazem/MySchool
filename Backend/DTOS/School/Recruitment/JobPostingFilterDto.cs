using Backend.Models;

namespace Backend.DTOS.School.Recruitment;

public class JobPostingFilterDto
{
    public int? SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public int? EmployeeJobTypeID { get; set; }
    public JobPostingStatus? Status { get; set; }
    public bool? IsActive { get; set; }
}
