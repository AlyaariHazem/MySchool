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
