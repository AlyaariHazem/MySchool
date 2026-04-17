namespace Backend.Models;

/// <summary>Stored as int on <see cref="EmployeeProfile.EmploymentStatus"/>.</summary>
public enum EmploymentStatus
{
    Active = 1,
    OnLeave = 2,
    Suspended = 3,
    Terminated = 4
}

/// <summary>Stored as int on <see cref="EmployeeLeave.LeaveType"/>.</summary>
public enum LeaveType
{
    Annual = 1,
    Sick = 2,
    Unpaid = 3,
    Emergency = 4,
    Other = 99
}

/// <summary>Stored as int on <see cref="EmployeeLeave.ApprovalStatus"/>.</summary>
public enum ApprovalStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4
}
