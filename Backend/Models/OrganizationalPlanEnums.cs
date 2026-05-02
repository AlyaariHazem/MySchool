namespace Backend.Models;

/// <summary>Lifecycle of a school strategic (multi-year) performance goal.</summary>
public enum StrategicGoalStatus
{
    Draft = 0,
    Active = 1,
    Achieved = 2,
    Superseded = 3,
    Archived = 4,
    Cancelled = 5
}

/// <summary>Annual performance goal aligned to an academic year (optionally linked to a strategic goal).</summary>
public enum AnnualGoalStatus
{
    Draft = 0,
    Active = 1,
    Completed = 2,
    Cancelled = 3
}

/// <summary>Operational plan under an annual goal (execution horizon).</summary>
public enum OperationalPlanStatus
{
    Draft = 0,
    Active = 1,
    OnHold = 2,
    Completed = 3,
    Cancelled = 4
}

/// <summary>Task within an operational plan.</summary>
public enum PlanTaskStatus
{
    NotStarted = 0,
    InProgress = 1,
    Blocked = 2,
    Completed = 3,
    Cancelled = 4
}

/// <summary>Department-level goal (organizational performance), optionally tied to strategic/annual context.</summary>
public enum DepartmentGoalStatus
{
    Draft = 0,
    Active = 1,
    Achieved = 2,
    Cancelled = 3
}
