using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models;

public class VisitRecommendation
{
    public int VisitRecommendationID { get; set; }

    [Required]
    public int SupervisorVisitID { get; set; }

    [JsonIgnore]
    public SupervisorVisit SupervisorVisit { get; set; } = null!;

    [Required]
    [MaxLength(8000)]
    public string RecommendationText { get; set; } = string.Empty;

    /// <summary>حالة تنفيذ التوصيات.</summary>
    public RecommendationImplementationStatus ImplementationStatus { get; set; } = RecommendationImplementationStatus.Pending;

    public DateOnly? DueDate { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<RecommendationFollowUp> FollowUps { get; set; } = new List<RecommendationFollowUp>();
}
