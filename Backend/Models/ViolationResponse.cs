using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Written response or clarification reply on a violation case.</summary>
public class ViolationResponse
{
    public int ViolationResponseID { get; set; }

    [Required]
    public int ViolationID { get; set; }

    [JsonIgnore]
    public Violation Violation { get; set; } = null!;

    public int? AuthorEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile? AuthorEmployeeProfile { get; set; }

    [Required]
    [MaxLength(4000)]
    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
