using System.ComponentModel.DataAnnotations;

namespace Backend.DTOS.School.SupervisorVisit;

public class SupervisorVisitFilterDto
{
    public int? SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public int? VisitedTeacherID { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}

public class SupervisorVisitListItemDto
{
    public int SupervisorVisitID { get; set; }
    public int SchoolID { get; set; }
    public int AcademicYearID { get; set; }
    public int VisitedTeacherID { get; set; }
    public string VisitedTeacherName { get; set; } = string.Empty;
    public int? ClassID { get; set; }
    public string? ClassName { get; set; }
    public int? SubjectID { get; set; }
    public string? SubjectName { get; set; }
    public int SupervisorEmployeeProfileID { get; set; }
    public string SupervisorName { get; set; } = string.Empty;
    public DateOnly VisitDate { get; set; }
    public int Status { get; set; }
    public decimal OverallScoreOutOf100 { get; set; }
}

public class RecommendationFollowUpReadDto
{
    public int RecommendationFollowUpID { get; set; }
    public int VisitRecommendationID { get; set; }
    public string FollowUpNote { get; set; } = string.Empty;
    public DateOnly FollowUpDate { get; set; }
    public int? FollowUpByEmployeeProfileID { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class VisitObservationReadDto
{
    public int VisitObservationID { get; set; }
    public int SupervisorVisitID { get; set; }
    public string? Category { get; set; }
    public string ObservationText { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class VisitRecommendationReadDto
{
    public int VisitRecommendationID { get; set; }
    public int SupervisorVisitID { get; set; }
    public string RecommendationText { get; set; } = string.Empty;
    public int ImplementationStatus { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public List<RecommendationFollowUpReadDto> FollowUps { get; set; } = new();
}

public class SupervisorVisitDetailDto : SupervisorVisitListItemDto
{
    public string? SummaryNotes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public List<VisitObservationReadDto> Observations { get; set; } = new();
    public List<VisitRecommendationReadDto> Recommendations { get; set; } = new();
}

public class RecommendationFollowUpWriteDto
{
    [Required]
    [MaxLength(8000)]
    public string FollowUpNote { get; set; } = string.Empty;

    public DateOnly FollowUpDate { get; set; }
    public int? FollowUpByEmployeeProfileID { get; set; }
}

public class VisitObservationWriteDto
{
    [MaxLength(200)]
    public string? Category { get; set; }

    [Required]
    [MaxLength(8000)]
    public string ObservationText { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}

public class VisitRecommendationWriteDto
{
    [Required]
    [MaxLength(8000)]
    public string RecommendationText { get; set; } = string.Empty;

    public int ImplementationStatus { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public int SortOrder { get; set; }
    public List<RecommendationFollowUpWriteDto> FollowUps { get; set; } = new();
}

public class SupervisorVisitWriteDto
{
    [Required]
    public int SchoolID { get; set; }

    [Required]
    public int AcademicYearID { get; set; }

    [Required]
    public int VisitedTeacherID { get; set; }

    public int? ClassID { get; set; }
    public int? SubjectID { get; set; }

    [Required]
    public int SupervisorEmployeeProfileID { get; set; }

    public DateOnly VisitDate { get; set; }
    public int Status { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal OverallScoreOutOf100 { get; set; }

    [MaxLength(4000)]
    public string? SummaryNotes { get; set; }

    public List<VisitObservationWriteDto> Observations { get; set; } = new();
    public List<VisitRecommendationWriteDto> Recommendations { get; set; } = new();
}
