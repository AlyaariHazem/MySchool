using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Immutable audit trail for a complaint or a suggestion (exactly one parent set).</summary>
public class ConcernActionLog
{
    public int ConcernActionLogID { get; set; }

    public int? ComplaintID { get; set; }

    [JsonIgnore]
    public Complaint? Complaint { get; set; }

    public int? SuggestionID { get; set; }

    [JsonIgnore]
    public Suggestion? Suggestion { get; set; }

    public int? ActorEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile? ActorEmployeeProfile { get; set; }

    public ConcernActionKind ActionKind { get; set; } = ConcernActionKind.Created;

    public ConcernStatus? OldStatus { get; set; }

    public ConcernStatus? NewStatus { get; set; }

    [MaxLength(4000)]
    public string? Comment { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
