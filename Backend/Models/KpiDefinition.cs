using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>KPI definition at school, department, or employee scope.</summary>
public class KpiDefinition
{
    public int KpiDefinitionID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [Required]
    [MaxLength(64)]
    public string Code { get; set; } = string.Empty;

    /// <summary>Legacy display title; prefer <see cref="EnglishName"/> when set.</summary>
    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? EnglishName { get; set; }

    [MaxLength(256)]
    public string? ArabicName { get; set; }

    [MaxLength(4000)]
    public string? Description { get; set; }

    public KpiCategory Category { get; set; } = KpiCategory.Institutional;

    public DashboardAudience? TargetAudience { get; set; }

    public KpiCalculationType CalculationType { get; set; } = KpiCalculationType.Aggregate;

    [MaxLength(128)]
    public string? Unit { get; set; }

    public bool HigherIsBetter { get; set; } = true;

    /// <summary>Default / target value for this KPI at definition level.</summary>
    public decimal? TargetValue { get; set; }

    public bool IsSystemKpi { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public ICollection<KpiSnapshot> Snapshots { get; set; } = new List<KpiSnapshot>();
}
