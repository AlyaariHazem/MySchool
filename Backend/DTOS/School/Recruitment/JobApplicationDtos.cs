using System.ComponentModel.DataAnnotations;
using Backend.Models;

namespace Backend.DTOS.School.Recruitment;

public class JobApplicationCreateDto
{
    [Required]
    public int JobPostingID { get; set; }

    /// <summary>When the posting has no academic year, this must be set and belong to the same school.</summary>
    public int? AcademicYearID { get; set; }

    [Required]
    [MaxLength(128)]
    public string ApplicantFirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string ApplicantLastName { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? ApplicantArabicName { get; set; }

    [MaxLength(256)]
    public string? ApplicantEnglishName { get; set; }

    [MaxLength(64)]
    public string? NationalID { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(32)]
    public string? Gender { get; set; }

    [MaxLength(64)]
    public string? Phone { get; set; }

    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(512)]
    public string? Address { get; set; }

    [MaxLength(256)]
    public string? HighestQualification { get; set; }

    [MaxLength(256)]
    public string? Specialization { get; set; }

    public int? YearsOfExperience { get; set; }

    [MaxLength(256)]
    public string? CurrentEmployer { get; set; }

    [MaxLength(1024)]
    public string? ResumeFileUrl { get; set; }

    [MaxLength(8000)]
    public string? CoverLetter { get; set; }

    [MaxLength(128)]
    public string? Source { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }
}

public class JobApplicationUpdateDto
{
    [MaxLength(128)]
    public string? ApplicantFirstName { get; set; }

    [MaxLength(128)]
    public string? ApplicantLastName { get; set; }

    [MaxLength(256)]
    public string? ApplicantArabicName { get; set; }

    [MaxLength(256)]
    public string? ApplicantEnglishName { get; set; }

    [MaxLength(64)]
    public string? NationalID { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(32)]
    public string? Gender { get; set; }

    [MaxLength(64)]
    public string? Phone { get; set; }

    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(512)]
    public string? Address { get; set; }

    [MaxLength(256)]
    public string? HighestQualification { get; set; }

    [MaxLength(256)]
    public string? Specialization { get; set; }

    public int? YearsOfExperience { get; set; }

    [MaxLength(256)]
    public string? CurrentEmployer { get; set; }

    [MaxLength(1024)]
    public string? ResumeFileUrl { get; set; }

    [MaxLength(8000)]
    public string? CoverLetter { get; set; }

    [MaxLength(128)]
    public string? Source { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public bool? IsActive { get; set; }
}

public class JobApplicationReadDto
{
    public int JobApplicationID { get; set; }
    public int JobPostingID { get; set; }
    public string? JobPostingTitle { get; set; }
    public int SchoolID { get; set; }
    public int AcademicYearID { get; set; }
    public string ApplicantFirstName { get; set; } = string.Empty;
    public string ApplicantLastName { get; set; } = string.Empty;
    public string? ApplicantArabicName { get; set; }
    public string? ApplicantEnglishName { get; set; }
    public string? NationalID { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? HighestQualification { get; set; }
    public string? Specialization { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? CurrentEmployer { get; set; }
    public string? ResumeFileUrl { get; set; }
    public string? CoverLetter { get; set; }
    public string? Source { get; set; }
    public JobApplicationStatus Status { get; set; }
    public DateTime AppliedAt { get; set; }
    public string? Notes { get; set; }
    public int? ConvertedEmployeeProfileID { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public class JobApplicationListDto
{
    public int JobApplicationID { get; set; }
    public int JobPostingID { get; set; }
    public string? JobPostingTitle { get; set; }
    public int SchoolID { get; set; }
    public string ApplicantFirstName { get; set; } = string.Empty;
    public string ApplicantLastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public JobApplicationStatus Status { get; set; }
    public DateTime AppliedAt { get; set; }
    public int? ConvertedEmployeeProfileID { get; set; }
}

public class JobApplicationFullDto
{
    public JobApplicationReadDto Application { get; set; } = null!;
    public JobPostingReadDto? Posting { get; set; }
    public IReadOnlyList<InterviewReadDto> Interviews { get; set; } = Array.Empty<InterviewReadDto>();
    public IReadOnlyList<CandidateEvaluationReadDto> Evaluations { get; set; } = Array.Empty<CandidateEvaluationReadDto>();
    public HiringDecisionReadDto? Decision { get; set; }
}

public class JobApplicationStatusMoveDto
{
    public JobApplicationStatus NewStatus { get; set; }
}
