namespace Backend.Models.Master;

/// <summary>
/// Per-tenant authorization role. Distinct from global <see cref="Microsoft.AspNetCore.Identity.IdentityRole"/> (SuperAdmin, etc.).
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
