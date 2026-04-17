using Backend.Models;

namespace Backend.DTOS.School.Recruitment;

public class JobApplicationFilterDto
{
    public int? SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public int? JobPostingID { get; set; }
    public JobApplicationStatus? Status { get; set; }
    public string? Email { get; set; }
    public string? NationalID { get; set; }
    public bool? IsActive { get; set; }
}
