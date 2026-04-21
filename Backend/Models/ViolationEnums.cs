namespace Backend.Models;

/// <summary>
/// Escalation ladder for staff violations (Arabic labels in UI: استيضاح، تنبيه خطي، لفت نظر، تنبيه نهائي).
/// Stored as int on <see cref="ViolationType"/> and escalation history.
/// </summary>
public enum ViolationKind
{
    /// <summary>استيضاح — clarification step.</summary>
    Clarification = 0,

    /// <summary>تنبيه خطي — written warning.</summary>
    WrittenWarning = 1,

    /// <summary>لفت نظر — attention / verbal notice.</summary>
    AttentionNotice = 2,

    /// <summary>تنبيه نهائي — final warning.</summary>
    FinalWarning = 3
}

/// <summary>Lifecycle of a violation case.</summary>
public enum ViolationStatus
{
    Draft = 0,
    Open = 1,
    InProgress = 2,
    Resolved = 3,
    Closed = 4,
    Cancelled = 5
}

/// <summary>Category for a recorded <see cref="ViolationAction"/>.</summary>
public enum ViolationActionCategory
{
    GeneralNote = 0,
    MeetingHeld = 1,
    FormalDocumentation = 2,
    Other = 9
}
