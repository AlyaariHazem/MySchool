namespace MySchool.Contracts.Auth;

/// <summary>
/// Per-tenant authorization role. Distinct from global Identity roles (ADMIN, MANAGER, etc.).
/// </summary>
public enum TenantRole
{
    SchoolAdmin = 0,
    Teacher = 1,
    Parent = 2,
    Student = 3,
    Accountant = 4,
    Staff = 5
}
