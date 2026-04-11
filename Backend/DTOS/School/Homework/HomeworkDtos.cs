namespace Backend.DTOS.School.Homework;

public class HomeworkTaskLinkDto
{
    public int HomeworkTaskLinkID { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Label { get; set; }
    public int SortOrder { get; set; }
}

public class HomeworkTaskLinkInputDto
{
    public string Url { get; set; } = string.Empty;
    public string? Label { get; set; }
    public int SortOrder { get; set; }
}

public class CreateHomeworkTaskDto
{
    /// <summary>Required when an admin/manager creates a task on behalf of a teacher.</summary>
    public int? TeacherID { get; set; }

    /// <summary>Ignored on create/update; the API sets this from the active academic year in the database.</summary>
    public int YearID { get; set; }
    public int TermID { get; set; }
    public int ClassID { get; set; }
    public int DivisionID { get; set; }
    public int SubjectID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDateUtc { get; set; }
    public bool SubmissionRequired { get; set; }
    public IReadOnlyList<HomeworkTaskLinkInputDto>? Links { get; set; }
}

public class UpdateHomeworkTaskDto: CreateHomeworkTaskDto
{
    public int HomeworkTaskID { get; set; }
}

public class HomeworkTaskListDto
{
    public int HomeworkTaskID { get; set; }
    public int TeacherID { get; set; }
    public string? TeacherName { get; set; }
    public int YearID { get; set; }
    public int TermID { get; set; }
    public int ClassID { get; set; }
    public string? ClassName { get; set; }
    public int DivisionID { get; set; }
    public string? DivisionName { get; set; }
    public int SubjectID { get; set; }
    public string? SubjectName { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime DueDateUtc { get; set; }
    public bool SubmissionRequired { get; set; }
    public int SubmissionCount { get; set; }
    public int PendingCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class HomeworkTaskDetailDto : HomeworkTaskListDto
{
    public string? Description { get; set; }
    public IReadOnlyList<HomeworkTaskLinkDto> Links { get; set; } = Array.Empty<HomeworkTaskLinkDto>();
}

public class HomeworkFilterQuery
{
    public int? YearID { get; set; }
    public int? TermID { get; set; }
    public int? ClassID { get; set; }
    public int? DivisionID { get; set; }
    public int? SubjectID { get; set; }
    public int? TeacherID { get; set; }
}

public class HomeworkSubmissionFileDto
{
    public int HomeworkSubmissionFileID { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string? FileName { get; set; }
}

public class HomeworkSubmissionRowDto
{
    public int HomeworkSubmissionID { get; set; }
    public int StudentID { get; set; }
    public string? StudentName { get; set; }
    public byte Status { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }
    public string? AnswerText { get; set; }
    public IReadOnlyList<HomeworkSubmissionFileDto> Files { get; set; } = Array.Empty<HomeworkSubmissionFileDto>();
    public string? TeacherFeedback { get; set; }
    public decimal? Score { get; set; }
    public bool FeedbackPublished { get; set; }
}

public class ReviewHomeworkSubmissionDto
{
    public byte Status { get; set; }
    public string? TeacherFeedback { get; set; }
    public decimal? Score { get; set; }
    public bool FeedbackPublished { get; set; }
}

public class StudentHomeworkListItemDto
{
    public int HomeworkTaskID { get; set; }
    public int HomeworkSubmissionID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? SubjectName { get; set; }
    public string? ClassName { get; set; }
    public string? DivisionName { get; set; }
    public DateTime DueDateUtc { get; set; }
    public bool SubmissionRequired { get; set; }
    public byte Status { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }
    public string? TeacherFeedback { get; set; }
    public decimal? Score { get; set; }
    public bool FeedbackPublished { get; set; }
}

/// <summary>Homework row for a guardian viewing all linked students' tasks.</summary>
public class GuardianStudentHomeworkRowDto : StudentHomeworkListItemDto
{
    public int StudentID { get; set; }
    public string? StudentName { get; set; }
}

public class StudentHomeworkDetailDto : StudentHomeworkListItemDto
{
    public string? Description { get; set; }
    public IReadOnlyList<HomeworkTaskLinkDto> TaskLinks { get; set; } = Array.Empty<HomeworkTaskLinkDto>();
    public string? AnswerText { get; set; }
    public IReadOnlyList<HomeworkSubmissionFileDto> Files { get; set; } = Array.Empty<HomeworkSubmissionFileDto>();
}

public class StudentSubmitHomeworkDto
{
    public string? AnswerText { get; set; }
    public IReadOnlyList<HomeworkSubmissionFileInputDto>? Files { get; set; }
}

public class HomeworkSubmissionFileInputDto
{
    public string FileUrl { get; set; } = string.Empty;
    public string? FileName { get; set; }
}

public class HomeworkActivitySummaryDto
{
    public int TaskCount { get; set; }
    public int MissingSubmissionCount { get; set; }
    public int GradedCount { get; set; }
    public IReadOnlyList<HomeworkTeacherActivityDto> Teachers { get; set; } = Array.Empty<HomeworkTeacherActivityDto>();
}

public class HomeworkTeacherActivityDto
{
    public int TeacherID { get; set; }
    public string? TeacherName { get; set; }
    public int TasksCreated { get; set; }
}
