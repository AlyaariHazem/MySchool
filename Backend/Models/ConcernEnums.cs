namespace Backend.Models;

/// <summary>Lifecycle of a complaint or suggestion.</summary>
public enum ConcernStatus
{
    Draft = 0,
    Submitted = 1,
    UnderReview = 2,
    InProgress = 3,
    Resolved = 4,
    Rejected = 5,
    Closed = 6,
    Cancelled = 7
}

/// <summary>Whether a category applies to complaints, suggestions, or both.</summary>
public enum ConcernCategoryKind
{
    Complaint = 0,
    Suggestion = 1,
    Both = 2
}

/// <summary>Audit entry for staff actions on a complaint or suggestion.</summary>
public enum ConcernActionKind
{
    Created = 0,
    StatusChanged = 1,
    NoteAdded = 2,
    Assigned = 3,
    Resolved = 4,
    Closed = 5,
    Rejected = 6
}
