using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Progress journal entry for a <see cref="PlanTask"/>.</summary>
public class PlanProgressUpdate
{
    public int PlanProgressUpdateID { get; set; }

    [Required]
    public int PlanTaskID { get; set; }

    [JsonIgnore]
    public PlanTask PlanTask { get; set; } = null!;

    [MaxLength(4000)]
    public string? Note { get; set; }

    public int? ProgressPercent { get; set; }

    public int? AuthorEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile? AuthorEmployeeProfile { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
