using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>معيار تقييم مرتبط بجائزة محددة.</summary>
public class AwardCriteria
{
    public int AwardCriteriaID { get; set; }

    [Required]
    public int AwardID { get; set; }

    [JsonIgnore]
    public Award Award { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    public int SortOrder { get; set; }

    /// <summary>وزن اختياري في التقييم (مثلاً من 100).</summary>
    public int? Weight { get; set; }
}
