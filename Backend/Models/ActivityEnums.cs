namespace Backend.Models;

/// <summary>Lifecycle of a staff activity request.</summary>
public enum ActivityRequestStatus
{
    Draft = 0,
    Submitted = 1,
    InReview = 2,
    Approved = 3,
    Rejected = 4,
    InProgress = 5,
    Completed = 6,
    Cancelled = 7
}

/// <summary>Approver decision for one step on an activity request.</summary>
public enum ActivityApprovalDecision
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Skipped = 3
}

/// <summary>Execution / implementation status for an activity request.</summary>
public enum ActivityExecutionStatus
{
    Pending = 0,
    InProgress = 1,
    WaitingExternal = 2,
    Completed = 3,
    Blocked = 4,
    Cancelled = 5
}
