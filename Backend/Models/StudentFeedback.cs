using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>One student's submission for a feedback cycle (answers stored as JSON keyed by question id).</summary>
public class StudentFeedback
{
    public int StudentFeedbackID { get; set; }

    [Required]
    public int TeacherFeedbackCycleID { get; set; }

    [JsonIgnore]
    public TeacherFeedbackCycle TeacherFeedbackCycle { get; set; } = null!;

    [Required]
    public int StudentID { get; set; }

    [JsonIgnore]
    public Student Student { get; set; } = null!;

    public FeedbackSubmissionStatus Status { get; set; } = FeedbackSubmissionStatus.Draft;

    public DateTime? SubmittedAtUtc { get; set; }

    /// <summary>JSON array of responses, e.g. <c>[{"questionId":1,"rating":4,"text":null,"yesNo":null}]</c>.</summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? ResponsesJson { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
