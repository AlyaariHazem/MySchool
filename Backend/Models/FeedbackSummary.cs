using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Precomputed aggregates for a cycle (students, parents, or combined).</summary>
public class FeedbackSummary
{
    public int FeedbackSummaryID { get; set; }

    [Required]
    public int TeacherFeedbackCycleID { get; set; }

    [JsonIgnore]
    public TeacherFeedbackCycle TeacherFeedbackCycle { get; set; } = null!;

    public FeedbackSummaryAudience Audience { get; set; }

    /// <summary>Number of submitted feedback forms in this audience slice.</summary>
    public int SubmittedCount { get; set; }

    /// <summary>Mean of numeric (1–5) answers across all numeric questions and submissions, when applicable.</summary>
    [Column(TypeName = "decimal(6,3)")]
    public decimal? AverageNumericScore { get; set; }

    /// <summary>Optional JSON with per-question averages / counts.</summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? AggregateJson { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public DateTime ComputedAtUtc { get; set; } = DateTime.UtcNow;
}
