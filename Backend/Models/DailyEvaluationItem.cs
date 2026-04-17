using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Score / comment for one criterion within a <see cref="DailyEvaluation"/>.</summary>
public class DailyEvaluationItem
{
    public int DailyEvaluationItemID { get; set; }

    [Required]
    public int DailyEvaluationID { get; set; }

    [JsonIgnore]
    public DailyEvaluation DailyEvaluation { get; set; } = null!;

    [Required]
    public int DailyEvaluationCriteriaID { get; set; }

    [JsonIgnore]
    public DailyEvaluationCriteria Criteria { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Score { get; set; }

    [MaxLength(4000)]
    public string? Comment { get; set; }

    public bool IsMandatorySatisfied { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
