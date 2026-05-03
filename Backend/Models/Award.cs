using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>تعريف جائزة على مستوى المدرسة (مثل نجم الأسبوع/الشهر/الفصل/العام).</summary>
public class Award
{
    public int AwardID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [Required]
    [MaxLength(64)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    public AwardCycleKind CycleKind { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public ICollection<AwardCriteria> Criteria { get; set; } = new List<AwardCriteria>();

    [JsonIgnore]
    public ICollection<AwardCycle> Cycles { get; set; } = new List<AwardCycle>();
}
