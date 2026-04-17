using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>
/// A single criterion belonging to a <see cref="DailyEvaluationTemplate"/>.
/// </summary>
public class DailyEvaluationCriteria
{
    public int DailyEvaluationCriteriaID { get; set; }

    [Required]
    public int DailyEvaluationTemplateID { get; set; }

    [JsonIgnore]
    public DailyEvaluationTemplate Template { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>Relative weight for analytics / weighted rollups (optional).</summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal Weight { get; set; } = 1m;

    [Column(TypeName = "decimal(18,2)")]
    public decimal MaxScore { get; set; } = 10m;

    [Column(TypeName = "decimal(18,2)")]
    public decimal MinScore { get; set; } = 0m;

    public bool IsMandatory { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<DailyEvaluationItem> Items { get; set; } = new List<DailyEvaluationItem>();
}
