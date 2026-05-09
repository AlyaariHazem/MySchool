namespace Backend.Models;

/// <summary>Sections stored as JSON snapshots when a time capsule is unlocked.</summary>
public enum TimeCapsuleSectionType
{
    GeneralInfo = 1,
    PerformanceTimeline = 2,
    Achievements = 3,
    Violations = 4,
    Evaluations = 5,
    Activities = 6,
    Reports = 7,
    FinalSummary = 8
}

public enum ResignationRequestStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public enum CapsuleUnlockApprovalStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public enum CapsuleNarrativeGeneratedBy
{
    System = 1,
    AI = 2,
    Admin = 3
}

public enum CapsuleAccessActionType
{
    Viewed = 1,
    Exported = 2
}
