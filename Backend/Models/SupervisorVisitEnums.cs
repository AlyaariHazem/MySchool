namespace Backend.Models;

/// <summary>Workflow state for a supervisor classroom visit record.</summary>
public enum SupervisorVisitStatus
{
    Draft = 0,
    Submitted = 1,
    Archived = 2,
}

/// <summary>Implementation status for a visit recommendation (حالة تنفيذ التوصيات).</summary>
public enum RecommendationImplementationStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Deferred = 3,
    NotApplicable = 4,
}
