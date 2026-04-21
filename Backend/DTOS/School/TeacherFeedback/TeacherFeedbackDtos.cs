namespace Backend.DTOS.School.TeacherFeedback;

public class TeacherFeedbackCycleFilterDto
{
    public int? SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public int? TeacherID { get; set; }
    public int? Status { get; set; }
}

public class FeedbackQuestionDto
{
    public int FeedbackQuestionID { get; set; }
    public int TeacherFeedbackCycleID { get; set; }
    public int SortOrder { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int QuestionType { get; set; }
    public int Audience { get; set; }
    public bool IsRequired { get; set; }
}

public class FeedbackQuestionWriteDto
{
    public int? FeedbackQuestionID { get; set; }
    public int SortOrder { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int QuestionType { get; set; }
    public int Audience { get; set; }
    public bool IsRequired { get; set; } = true;
}

public class TeacherFeedbackCycleListItemDto
{
    public int TeacherFeedbackCycleID { get; set; }
    public int SchoolID { get; set; }
    public int AcademicYearID { get; set; }
    public int TeacherID { get; set; }
    public string? TeacherName { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime OpensAtUtc { get; set; }
    public DateTime ClosesAtUtc { get; set; }
    public int Status { get; set; }
    public int QuestionCount { get; set; }
    public int StudentSubmittedCount { get; set; }
    public int ParentSubmittedCount { get; set; }
}

public class TeacherFeedbackCycleDetailDto : TeacherFeedbackCycleListItemDto
{
    public string? Description { get; set; }
    public List<FeedbackQuestionDto> Questions { get; set; } = new();
    public List<FeedbackSummaryDto> Summaries { get; set; } = new();
}

public class TeacherFeedbackCycleWriteDto
{
    public int SchoolID { get; set; }
    public int AcademicYearID { get; set; }
    public int TeacherID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime OpensAtUtc { get; set; }
    public DateTime ClosesAtUtc { get; set; }
    public int Status { get; set; }
    public List<FeedbackQuestionWriteDto>? Questions { get; set; }
}

public class FeedbackSummaryDto
{
    public int FeedbackSummaryID { get; set; }
    public int TeacherFeedbackCycleID { get; set; }
    public int Audience { get; set; }
    public int SubmittedCount { get; set; }
    public decimal? AverageNumericScore { get; set; }
    public string? AggregateJson { get; set; }
    public string? Notes { get; set; }
    public DateTime ComputedAtUtc { get; set; }
}

/// <summary>Single answer cell for student/parent submission.</summary>
public class FeedbackResponseItemDto
{
    public int QuestionId { get; set; }
    public int? Rating { get; set; }
    public string? Text { get; set; }
    public bool? YesNo { get; set; }
}

public class StudentFeedbackSubmitDto
{
    public int TeacherFeedbackCycleID { get; set; }
    public bool Submit { get; set; }
    public List<FeedbackResponseItemDto> Responses { get; set; } = new();
}

public class ParentFeedbackSubmitDto
{
    public int TeacherFeedbackCycleID { get; set; }
    public int StudentID { get; set; }
    public bool Submit { get; set; }
    public List<FeedbackResponseItemDto> Responses { get; set; } = new();
}

public class TeacherFeedbackOpenCycleDto
{
    public int TeacherFeedbackCycleID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? TeacherName { get; set; }
    public DateTime ClosesAtUtc { get; set; }
}

/// <summary>Cycle + questions for a student or parent filling the form.</summary>
public class TeacherFeedbackParticipantFormDto
{
    public int TeacherFeedbackCycleID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? TeacherName { get; set; }
    public DateTime ClosesAtUtc { get; set; }
    public List<FeedbackQuestionDto> Questions { get; set; } = new();
    public List<FeedbackResponseItemDto>? ExistingResponses { get; set; }
    public int SubmissionStatus { get; set; }
}
