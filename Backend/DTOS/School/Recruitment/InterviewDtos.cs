using System.ComponentModel.DataAnnotations;
using Backend.Models;

namespace Backend.DTOS.School.Recruitment;

public class InterviewCreateDto
{
    public DateTime InterviewDate { get; set; }

    [MaxLength(64)]
    public string? InterviewType { get; set; }

    [MaxLength(512)]
    public string? LocationOrMeetingLink { get; set; }

    [MaxLength(256)]
    public string? InterviewerName { get; set; }

    [MaxLength(450)]
    public string? InterviewerUserID { get; set; }

    public int? InterviewerEmployeeProfileID { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }
}

public class InterviewUpdateDto
{
    public DateTime? InterviewDate { get; set; }

    [MaxLength(64)]
    public string? InterviewType { get; set; }

    [MaxLength(512)]
    public string? LocationOrMeetingLink { get; set; }

    [MaxLength(256)]
    public string? InterviewerName { get; set; }

    [MaxLength(450)]
    public string? InterviewerUserID { get; set; }

    public int? InterviewerEmployeeProfileID { get; set; }

    public InterviewStatus? Status { get; set; }

    [MaxLength(4000)]
    public string? Summary { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public decimal? Score { get; set; }
}

public class InterviewReadDto
{
    public int InterviewID { get; set; }
    public int JobApplicationID { get; set; }
    public int SchoolID { get; set; }
    public int AcademicYearID { get; set; }
    public DateTime InterviewDate { get; set; }
    public string? InterviewType { get; set; }
    public string? LocationOrMeetingLink { get; set; }
    public string? InterviewerName { get; set; }
    public string? InterviewerUserID { get; set; }
    public int? InterviewerEmployeeProfileID { get; set; }
    public InterviewStatus Status { get; set; }
    public string? Summary { get; set; }
    public string? Notes { get; set; }
    public decimal? Score { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
