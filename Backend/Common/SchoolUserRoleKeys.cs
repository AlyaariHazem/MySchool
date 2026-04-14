namespace Backend.Common;

/// <summary>API values for <see cref="DTOS.School.Employee.EmployeeDTO.JopName"/> (unified school users).</summary>
public static class SchoolUserRoleKeys
{
    public const string Teacher = "Teacher";
    public const string Manager = "Manager";
    public const string SystemAdmin = "SystemAdmin";
    public const string EducationalSupervisor = "EducationalSupervisor";
    public const string AdministrativeSupervisor = "AdministrativeSupervisor";
    public const string AdministrativeEmployee = "AdministrativeEmployee";
    public const string Student = "Student";
    public const string Guardian = "Guardian";

    /// <summary>All keys used in <see cref="Master.RolePermission"/>.</summary>
    public static readonly string[] AllRoles =
    {
        Teacher, Manager, SystemAdmin, EducationalSupervisor, AdministrativeSupervisor, AdministrativeEmployee, Student, Guardian
    };

    /// <summary>Roles stored in <see cref="Models.SchoolStaff.StaffRole"/> (non-teacher staff).</summary>
    public static readonly string[] ManagerTableRoles =
    {
        SystemAdmin,
        EducationalSupervisor,
        AdministrativeSupervisor,
        AdministrativeEmployee
    };

    public static bool IsManagerTableRole(string? jopName) =>
        jopName != null && (jopName == Manager || ManagerTableRoles.Contains(jopName));
}
