using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Guardian feedback about a teacher, scoped to one child (student).</summary>
public class ParentFeedback
{
    public int ParentFeedbackID { get; set; }

    [Required]
    public int TeacherFeedbackCycleID { get; set; }

    [JsonIgnore]
    public TeacherFeedbackCycle TeacherFeedbackCycle { get; set; } = null!;

    [Required]
    public int GuardianID { get; set; }

    [JsonIgnore]
    public Guardian Guardian { get; set; } = null!;

    /// <summary>Which student this feedback refers to (must belong to the guardian).</summary>
    [Required]
    public int StudentID { get; set; }

    [JsonIgnore]
    public Student Student { get; set; } = null!;

    public FeedbackSubmissionStatus Status { get; set; } = FeedbackSubmissionStatus.Draft;

    public DateTime? SubmittedAtUtc { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? ResponsesJson { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
