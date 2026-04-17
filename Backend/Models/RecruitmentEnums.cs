namespace Backend.Models;

/// <summary>Stored as int on <see cref="JobPosting.Status"/>.</summary>
public enum JobPostingStatus
{
    Draft = 1,
    Open = 2,
    Closed = 3,
    Archived = 4
}

/// <summary>Stored as int on <see cref="JobApplication.Status"/>.</summary>
public enum JobApplicationStatus
{
    Submitted = 1,
    UnderReview = 2,
    InterviewScheduled = 3,
    Evaluated = 4,
    Accepted = 5,
    Rejected = 6,
    ConvertedToEmployee = 7,
    Withdrawn = 8
}

/// <summary>Stored as int on <see cref="Interview.Status"/>.</summary>
public enum InterviewStatus
{
    Scheduled = 1,
    Completed = 2,
    Cancelled = 3,
    NoShow = 4
}

/// <summary>Stored as int on <see cref="CandidateEvaluation.Recommendation"/>.</summary>
public enum EvaluationRecommendation
{
    StrongReject = 1,
    Reject = 2,
    Consider = 3,
    Recommend = 4,
    StrongRecommend = 5
}

/// <summary>Stored as int on <see cref="HiringDecision.DecisionStatus"/>.</summary>
public enum HiringDecisionStatus
{
    Pending = 1,
    Accepted = 2,
    Rejected = 3,
    Cancelled = 4
}
