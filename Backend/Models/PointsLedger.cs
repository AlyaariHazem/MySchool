using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>
/// Immutable ledger line: points movement for one employee from one business event.
/// </summary>
public class PointsLedger
{
    public int PointsLedgerID { get; set; }

    [Required]
    public int PointsTransactionID { get; set; }

    [JsonIgnore]
    public PointsTransaction PointsTransaction { get; set; } = null!;

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

    [Required]
    public int PointsSourceID { get; set; }

    [JsonIgnore]
    public PointsSource PointsSource { get; set; } = null!;

    public int? PointsRuleID { get; set; }

    [JsonIgnore]
    public PointsRule? PointsRule { get; set; }

    public int DeltaPoints { get; set; }

    [MaxLength(1000)]
    public string? Memo { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
