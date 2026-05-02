namespace Backend.Models;

/// <summary>Lifecycle of a scheduled meeting.</summary>
public enum MeetingStatus
{
    Draft = 0,
    Scheduled = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}

/// <summary>Attendance expectation for an invitee.</summary>
public enum MeetingAttendeeRole
{
    Required = 0,
    Optional = 1
}

/// <summary>Invitee response to a meeting.</summary>
public enum MeetingAttendeeResponse
{
    Pending = 0,
    Accepted = 1,
    Declined = 2,
    Tentative = 3
}

/// <summary>Action item status from a meeting.</summary>
public enum MeetingTaskStatus
{
    Open = 0,
    InProgress = 1,
    Done = 2,
    Cancelled = 3
}
