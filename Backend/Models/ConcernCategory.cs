using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Per-school taxonomy for complaints and/or suggestions.</summary>
public class ConcernCategory
{
    public int ConcernCategoryID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [Required]
    [MaxLength(64)]
    public string Code { get; set; } = string.Empty;

    public ConcernCategoryKind CategoryKind { get; set; } = ConcernCategoryKind.Both;

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? NameAr { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();

    public ICollection<Suggestion> Suggestions { get; set; } = new List<Suggestion>();
}
