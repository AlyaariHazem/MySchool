using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>
/// Configurable mapping from a source (+ optional key) to a points delta for a school/year.
/// </summary>
public class PointsRule
{
    public int PointsRuleID { get; set; }

    /// <summary>When null, rule applies to every school in the tenant (unless a school-specific rule wins).</summary>
    public int? SchoolID { get; set; }

    [JsonIgnore]
    public School? School { get; set; }

    /// <summary>When null, rule applies for any academic year.</summary>
    public int? AcademicYearID { get; set; }

    [JsonIgnore]
    public Year? AcademicYear { get; set; }

    [Required]
    public int PointsSourceID { get; set; }

    [JsonIgnore]
    public PointsSource PointsSource { get; set; } = null!;

    /// <summary>
    /// Match key: "*" for default, or structured e.g. "ViolationType:12", "AchievementCode:BONUS".
    /// </summary>
    [Required]
    [MaxLength(128)]
    public string RuleKey { get; set; } = "*";

    /// <summary>Positive reward or negative penalty.</summary>
    public int DeltaPoints { get; set; }

    /// <summary>Higher wins when multiple rules match.</summary>
    public int Priority { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? EffectiveFromUtc { get; set; }

    public DateTime? EffectiveToUtc { get; set; }

    [JsonIgnore]
    public ICollection<PointsLedger> LedgerLines { get; set; } = new List<PointsLedger>();
}
