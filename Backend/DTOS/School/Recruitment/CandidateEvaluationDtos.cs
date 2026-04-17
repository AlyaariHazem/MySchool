using System.ComponentModel.DataAnnotations;
using Backend.Models;

namespace Backend.DTOS.School.Recruitment;

public class CandidateEvaluationCreateDto
{
    public int? InterviewID { get; set; }

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

    public DateTime? EvaluatedAt { get; set; }
}

public class CandidateEvaluationUpdateDto
{
    public int? InterviewID { get; set; }

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

    public EvaluationRecommendation? Recommendation { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public DateTime? EvaluatedAt { get; set; }
}

public class CandidateEvaluationReadDto
{
    public int CandidateEvaluationID { get; set; }
    public int JobApplicationID { get; set; }
    public int? InterviewID { get; set; }
    public int SchoolID { get; set; }
    public int AcademicYearID { get; set; }
    public string? EvaluatorUserID { get; set; }
    public int? EvaluatorEmployeeProfileID { get; set; }
    public decimal? TechnicalScore { get; set; }
    public decimal? CommunicationScore { get; set; }
    public decimal? ClassManagementScore { get; set; }
    public decimal? CultureFitScore { get; set; }
    public decimal? OverallScore { get; set; }
    public string? Strengths { get; set; }
    public string? Weaknesses { get; set; }
    public EvaluationRecommendation Recommendation { get; set; }
    public string? Notes { get; set; }
    public DateTime EvaluatedAt { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
