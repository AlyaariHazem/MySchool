using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>
/// One logical posting (one user/API action). Contains one or more <see cref="PointsLedger"/> lines.
/// </summary>
public class PointsTransaction
{
    public int PointsTransactionID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [Required]
    public int AcademicYearID { get; set; }

    [JsonIgnore]
    public Year AcademicYear { get; set; } = null!;

    public DateTime PostedAtUtc { get; set; } = DateTime.UtcNow;

    public int? PostedByEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile? PostedByEmployeeProfile { get; set; }

    /// <summary>Optional deduplication key (e.g. AchievementRequest:42:POSTED).</summary>
    [MaxLength(128)]
    public string? IdempotencyKey { get; set; }

    /// <summary>Domain entity type: AchievementRequest, ActivityRequest, Violation, …</summary>
    [MaxLength(64)]
    public string? CorrelationEntityType { get; set; }

    public int? CorrelationEntityID { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [JsonIgnore]
    public ICollection<PointsLedger> LedgerLines { get; set; } = new List<PointsLedger>();
}
