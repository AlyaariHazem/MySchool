using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>
/// Materialized running total per employee / school / academic year (updated on each posting).
/// </summary>
public class PointsBalanceSnapshot
{
    public int PointsBalanceSnapshotID { get; set; }

    [Required]
    public int EmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile EmployeeProfile { get; set; } = null!;

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [Required]
    public int AcademicYearID { get; set; }

    [JsonIgnore]
    public Year AcademicYear { get; set; } = null!;

    public int TotalPoints { get; set; }

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public int? LastPointsLedgerID { get; set; }

    [JsonIgnore]
    public PointsLedger? LastPointsLedger { get; set; }
}
