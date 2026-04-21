using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

public class VisitObservation
{
    public int VisitObservationID { get; set; }

    [Required]
    public int SupervisorVisitID { get; set; }

    [JsonIgnore]
    public SupervisorVisit SupervisorVisit { get; set; } = null!;

    [MaxLength(200)]
    public string? Category { get; set; }

    [Required]
    [MaxLength(8000)]
    public string ObservationText { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
