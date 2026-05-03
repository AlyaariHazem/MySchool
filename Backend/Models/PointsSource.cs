using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>
/// Catalog of modules that can emit points (achievement, activity, violation, evaluation, etc.).
/// </summary>
public class PointsSource
{
    public int PointsSourceID { get; set; }

    /// <summary>Stable code used in APIs and rule matching, e.g. ACHIEVEMENT, VIOLATION.</summary>
    [Required]
    [MaxLength(64)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    [JsonIgnore]
    public ICollection<PointsRule> Rules { get; set; } = new List<PointsRule>();

    [JsonIgnore]
    public ICollection<PointsLedger> LedgerLines { get; set; } = new List<PointsLedger>();
}
