using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>
/// Links a teacher or manager row to a school year with a status (active vs archived for that year).
/// Person rows stay in <see cref="Teacher"/> / <see cref="Manager"/> — no duplicate people.
/// </summary>
public class EmployeeYearAssignment
{
    public int AssignmentID { get; set; }

    [Required]
    public int YearID { get; set; }

    /// <summary>Teacher or Manager (same strings as <c>JopName</c> on employee DTOs).</summary>
    [Required]
    [MaxLength(32)]
    public string EmployeeRole { get; set; } = "Teacher";

    /// <summary><see cref="Teacher.TeacherID"/> or <see cref="Manager.ManagerID"/> depending on <see cref="EmployeeRole"/>.</summary>
    public int EmployeeEntityID { get; set; }

    /// <summary>Active = counted as staff for this year; Archived = left / not continued for this year.</summary>
    [Required]
    [MaxLength(32)]
    public string AssignmentStatus { get; set; } = EmployeeYearAssignmentStatuses.Active;

    public DateTime? ExitDate { get; set; }

    [MaxLength(512)]
    public string? ExitReason { get; set; }

    [MaxLength(1024)]
    public string? Notes { get; set; }

    [JsonIgnore]
    public Year Year { get; set; } = null!;
}

/// <summary>Stored in <see cref="EmployeeYearAssignment.AssignmentStatus"/>.</summary>
public static class EmployeeYearAssignmentStatuses
{
    public const string Active = "Active";
    public const string Archived = "Archived";
}

/// <summary>Stored in <see cref="EmployeeYearAssignment.EmployeeRole"/>.</summary>
public static class EmployeeYearAssignmentRoles
{
    public const string Teacher = "Teacher";
    public const string Manager = "Manager";
    /// <summary>All <see cref="SchoolStaff"/> rows; <see cref="EmployeeYearAssignment.EmployeeEntityID"/> is <see cref="SchoolStaff.SchoolStaffID"/>.</summary>
    public const string SchoolStaff = "SchoolStaff";
    public const string Student = "Student";
    public const string Guardian = "Guardian";
}
