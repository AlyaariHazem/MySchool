namespace Backend.Models;

/// <summary>Stored as int on <see cref="DailyEvaluationTemplate.Status"/>.</summary>
public enum EvaluationTemplateStatus
{
    Draft = 1,
    Active = 2,
    Inactive = 3,
    Archived = 4
}

/// <summary>Stored as int on <see cref="DailyEvaluation.Status"/>.</summary>
public enum DailyEvaluationStatus
{
    Draft = 1,
    Submitted = 2,
    Locked = 3
}

/// <summary>Stored as int on <see cref="EvaluationLock.Status"/>.</summary>
public enum EvaluationLockStatus
{
    Open = 1,
    Locked = 2,
    Reopened = 3
}

/// <summary>Stored as int on <see cref="EvaluationOverrideLog.OverrideActionType"/>.</summary>
public enum EvaluationOverrideActionType
{
    EditAfterLock = 1,
    ReopenEvaluation = 2,
    UnlockDay = 3,
    ForceUpdate = 4,
    DeleteAfterLock = 5
}
