using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Staff (or school) improvement suggestion.</summary>
public class Suggestion
{
    public int SuggestionID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [Required]
    public int AcademicYearID { get; set; }

    [JsonIgnore]
    public Year AcademicYear { get; set; } = null!;

    [Required]
    public int ConcernCategoryID { get; set; }

    [JsonIgnore]
    public ConcernCategory ConcernCategory { get; set; } = null!;

    [Required]
    public int SubmitterEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile SubmitterEmployeeProfile { get; set; } = null!;

    public int? AssignedToEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile? AssignedToEmployeeProfile { get; set; }

    [Required]
    [MaxLength(512)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Details { get; set; }

    public ConcernStatus Status { get; set; } = ConcernStatus.Draft;

    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ClosedAtUtc { get; set; }

    public ICollection<ConcernActionLog> ActionLogs { get; set; } = new List<ConcernActionLog>();
}
