namespace Backend.Models;

/// <summary>Lifecycle of a teacher feedback cycle (تقييم أداء المعلم من الطلاب/أولياء الأمور).</summary>
public enum TeacherFeedbackCycleStatus
{
    Draft = 0,
    Active = 1,
    Closed = 2,
}

public enum FeedbackQuestionType
{
    Rating1To5 = 1,
    Text = 2,
    YesNo = 3,
}

/// <summary>Who should answer this question.</summary>
public enum FeedbackQuestionAudience
{
    StudentsOnly = 1,
    ParentsOnly = 2,
    Both = 3,
}

public enum FeedbackSubmissionStatus
{
    Draft = 0,
    Submitted = 1,
}

/// <summary>Which population a <see cref="FeedbackSummary"/> aggregates.</summary>
public enum FeedbackSummaryAudience
{
    Students = 1,
    Parents = 2,
    Combined = 3,
}
