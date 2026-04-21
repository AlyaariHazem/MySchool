namespace Backend.Models;

/// <summary>Lifecycle state of an achievement request.</summary>
public enum AchievementRequestStatus
{
    Draft = 0,
    Submitted = 1,
    InReview = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5
}

/// <summary>Approver decision for one step in the approval workflow.</summary>
public enum AchievementApprovalDecision
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Skipped = 3
}
