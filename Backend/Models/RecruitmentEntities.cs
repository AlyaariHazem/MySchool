using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

public class JobPosting
{
    public int JobPostingID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    public int? AcademicYearID { get; set; }

    [Required]
    public int EmployeeJobTypeID { get; set; }

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? Department { get; set; }

    [MaxLength(8000)]
    public string? Description { get; set; }

    [MaxLength(8000)]
    public string? Requirements { get; set; }

    [MaxLength(8000)]
    public string? Responsibilities { get; set; }

    [MaxLength(64)]
    public string? EmploymentType { get; set; }

    public int NumberOfOpenings { get; set; } = 1;

    public DateTime PostingDate { get; set; }

    public DateTime? ClosingDate { get; set; }

    public JobPostingStatus Status { get; set; } = JobPostingStatus.Draft;

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public School School { get; set; } = null!;

    [JsonIgnore]
    public Year? AcademicYear { get; set; }

    [JsonIgnore]
    public EmployeeJobType JobType { get; set; } = null!;

    [JsonIgnore]
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}

public class JobApplication
{
    public int JobApplicationID { get; set; }

    [Required]
    public int JobPostingID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [Required]
    public int AcademicYearID { get; set; }

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

    public JobApplicationStatus Status { get; set; } = JobApplicationStatus.Submitted;

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public int? ConvertedEmployeeProfileID { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public JobPosting JobPosting { get; set; } = null!;

    [JsonIgnore]
    public School School { get; set; } = null!;

    [JsonIgnore]
    public Year AcademicYear { get; set; } = null!;

    [JsonIgnore]
    public EmployeeProfile? ConvertedEmployeeProfile { get; set; }

    [JsonIgnore]
    public ICollection<Interview> Interviews { get; set; } = new List<Interview>();

    [JsonIgnore]
    public ICollection<CandidateEvaluation> Evaluations { get; set; } = new List<CandidateEvaluation>();

    [JsonIgnore]
    public HiringDecision? HiringDecision { get; set; }
}

public class Interview
{
    public int InterviewID { get; set; }

    [Required]
    public int JobApplicationID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [Required]
    public int AcademicYearID { get; set; }

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

    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

    [MaxLength(4000)]
    public string? Summary { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public decimal? Score { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public JobApplication JobApplication { get; set; } = null!;

    [JsonIgnore]
    public School School { get; set; } = null!;

    [JsonIgnore]
    public Year AcademicYear { get; set; } = null!;

    [JsonIgnore]
    public EmployeeProfile? InterviewerEmployeeProfile { get; set; }

    [JsonIgnore]
    public ICollection<CandidateEvaluation> Evaluations { get; set; } = new List<CandidateEvaluation>();
}

public class CandidateEvaluation
{
    public int CandidateEvaluationID { get; set; }

    [Required]
    public int JobApplicationID { get; set; }

    public int? InterviewID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [Required]
    public int AcademicYearID { get; set; }

    [MaxLength(450)]
    public string? EvaluatorUserID { get; set; }

    public int? EvaluatorEmployeeProfileID { get; set; }

    public decimal? TechnicalScore { get; set; }

    public decimal? CommunicationScore { get; set; }

    public decimal? ClassManagementScore { get; set; }

    public decimal? CultureFitScore { get; set; }

    public decimal? OverallScore { get; set; }

    [MaxLength(4000)]
    public string? Strengths { get; set; }

    [MaxLength(4000)]
    public string? Weaknesses { get; set; }

    public EvaluationRecommendation Recommendation { get; set; } = EvaluationRecommendation.Consider;

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public JobApplication JobApplication { get; set; } = null!;

    [JsonIgnore]
    public Interview? Interview { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [JsonIgnore]
    public Year AcademicYear { get; set; } = null!;

    [JsonIgnore]
    public EmployeeProfile? EvaluatorEmployeeProfile { get; set; }
}

public class HiringDecision
{
    public int HiringDecisionID { get; set; }

    [Required]
    public int JobApplicationID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [Required]
    public int AcademicYearID { get; set; }

    public HiringDecisionStatus DecisionStatus { get; set; } = HiringDecisionStatus.Pending;

    public DateTime DecisionDate { get; set; } = DateTime.UtcNow;

    [MaxLength(450)]
    public string? DecidedByUserID { get; set; }

    public int? DecidedByEmployeeProfileID { get; set; }

    [Required]
    public int OfferJobTypeID { get; set; }

    public DateTime? ProposedHireDate { get; set; }

    [MaxLength(2000)]
    public string? ProposedSalaryNotes { get; set; }

    [MaxLength(4000)]
    public string? Reason { get; set; }

    [MaxLength(4000)]
    public string? InternalNotes { get; set; }

    public int? ConvertedEmployeeProfileID { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public JobApplication JobApplication { get; set; } = null!;

    [JsonIgnore]
    public School School { get; set; } = null!;

    [JsonIgnore]
    public Year AcademicYear { get; set; } = null!;

    [JsonIgnore]
    public EmployeeJobType OfferJobType { get; set; } = null!;

    [JsonIgnore]
    public EmployeeProfile? DecidedByEmployeeProfile { get; set; }

    [JsonIgnore]
    public EmployeeProfile? ConvertedEmployeeProfile { get; set; }
}
