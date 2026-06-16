namespace MySchool.Contracts.Authorization;

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

    public static readonly string[] AllRoles =
    {
        Teacher, Manager, SystemAdmin, EducationalSupervisor, AdministrativeSupervisor, AdministrativeEmployee, Student, Guardian
    };

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
