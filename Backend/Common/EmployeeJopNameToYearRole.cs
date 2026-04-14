using System;
using Backend.Models;

namespace Backend.Common;

/// <summary>Maps <see cref="DTOS.School.Employee.EmployeeDTO.JopName"/> to <see cref="EmployeeYearAssignment.EmployeeRole"/> values.</summary>
public static class EmployeeJopNameToYearRole
{
    public static string ToAssignmentRole(string? jopName)
    {
        if (string.IsNullOrWhiteSpace(jopName))
            return EmployeeYearAssignmentRoles.Teacher;

        if (string.Equals(jopName, SchoolUserRoleKeys.Teacher, StringComparison.OrdinalIgnoreCase))
            return EmployeeYearAssignmentRoles.Teacher;

        if (string.Equals(jopName, SchoolUserRoleKeys.Manager, StringComparison.OrdinalIgnoreCase))
            return EmployeeYearAssignmentRoles.Manager;

        if (string.Equals(jopName, SchoolUserRoleKeys.Student, StringComparison.OrdinalIgnoreCase))
            return EmployeeYearAssignmentRoles.Student;

        if (string.Equals(jopName, SchoolUserRoleKeys.Guardian, StringComparison.OrdinalIgnoreCase))
            return EmployeeYearAssignmentRoles.Guardian;

        foreach (var r in SchoolUserRoleKeys.ManagerTableRoles)
        {
            if (string.Equals(jopName, r, StringComparison.OrdinalIgnoreCase))
                return EmployeeYearAssignmentRoles.SchoolStaff;
        }

        return EmployeeYearAssignmentRoles.Teacher;
    }
}
