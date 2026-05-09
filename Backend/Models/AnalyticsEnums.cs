namespace Backend.Models;

/// <summary>Analytics period granularity used for KPI snapshots and trend analysis.</summary>
public enum AnalyticsPeriodKind
{
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Termly = 4,
    Yearly = 5
}

/// <summary>Target dashboard audience for analytics rendering.</summary>
public enum DashboardAudience
{
    TopManagement = 1,
    EducationalSupervisor = 2,
    AdministrativeSupervisor = 3,
    EmployeeSelf = 4,
    School = 5,
    YearComparison = 6
}

public enum KpiCategory
{
    Institutional = 1,
    Academic = 2,
    Discipline = 3,
    Engagement = 4,
    Workforce = 5,
    Other = 99
}

public enum KpiCalculationType
{
    Aggregate = 1,
    Average = 2,
    Ratio = 3,
    Count = 4,
    Latest = 5,
    Scripted = 6
}

/// <summary>Generic entity reference for trend rows (school, department, teacher, …).</summary>
public enum AnalyticsEntityType
{
    None = 0,
    School = 1,
    Department = 2,
    Teacher = 3,
    Kpi = 4,
    Metric = 5
}

public enum AnalyticsTrendDirection
{
    Unknown = 0,
    Improving = 1,
    Stable = 2,
    Declining = 3
}

public enum AnalyticsRiskLevel
{
    Low = 1,
    Medium = 2,
    High = 3
}

public enum KpiSnapshotStatus
{
    Ok = 1,
    Warning = 2,
    Critical = 3,
    PendingSource = 4
}
