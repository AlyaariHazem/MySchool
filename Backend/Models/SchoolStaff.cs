using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>
/// Non-teacher school employees (system admin, supervisors, administrative staff).
/// <see cref="Managers"/> is limited to one row per <see cref="SchoolID"/>; this table allows many staff per school.
/// </summary>
public class SchoolStaff
{
    public int SchoolStaffID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [Required]
    public string UserID { get; set; } = string.Empty;

    /// <summary>One of <see cref="Common.SchoolUserRoleKeys"/> manager-table equivalents.</summary>
    [Required]
    [MaxLength(64)]
    public string StaffRole { get; set; } = string.Empty;

    [Required]
    public Name FullName { get; set; } = null!;

    public DateTime? DOB { get; set; }
    public string? ImageURL { get; set; }
}
