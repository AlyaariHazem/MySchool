namespace Backend.Models;

/// <summary>Available employee request categories.</summary>
public enum EmployeeRequestCategory
{
    Tools = 0,
    Advance = 1,
    Support = 2
}

/// <summary>Lifecycle status of the employee request.</summary>
public enum EmployeeRequestStatus
{
    Draft = 0,
    Submitted = 1,
    InApproval = 2,
    Approved = 3,
    Rejected = 4,
    InExecution = 5,
    Completed = 6,
    Cancelled = 7
}

/// <summary>Decision for one approval step.</summary>
public enum RequestApprovalDecision
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Skipped = 3
}

/// <summary>Current execution tracking status.</summary>
public enum RequestExecutionStatus
{
    Pending = 0,
    InProgress = 1,
    WaitingExternal = 2,
    Completed = 3,
    Blocked = 4,
    Cancelled = 5
}
