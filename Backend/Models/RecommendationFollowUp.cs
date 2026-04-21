using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

public class RecommendationFollowUp
{
    public int RecommendationFollowUpID { get; set; }

    [Required]
    public int VisitRecommendationID { get; set; }

    [JsonIgnore]
    public VisitRecommendation VisitRecommendation { get; set; } = null!;

    [Required]
    [MaxLength(8000)]
    public string FollowUpNote { get; set; } = string.Empty;

    [Required]
    public DateOnly FollowUpDate { get; set; }

    /// <summary>Optional author of the follow-up entry.</summary>
    public int? FollowUpByEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile? FollowUpByEmployeeProfile { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
