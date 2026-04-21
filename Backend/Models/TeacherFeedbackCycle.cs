using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>
/// A time-bounded feedback window for one teacher (students and/or guardians submit structured answers).
/// </summary>
public class TeacherFeedbackCycle
{
    public int TeacherFeedbackCycleID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [Required]
    public int AcademicYearID { get; set; }

    [JsonIgnore]
    public Year AcademicYear { get; set; } = null!;

    /// <summary>Teacher being evaluated.</summary>
    [Required]
    public int TeacherID { get; set; }

    [JsonIgnore]
    public Teacher Teacher { get; set; } = null!;

    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime OpensAtUtc { get; set; }

    public DateTime ClosesAtUtc { get; set; }

    public TeacherFeedbackCycleStatus Status { get; set; } = TeacherFeedbackCycleStatus.Draft;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<FeedbackQuestion> Questions { get; set; } = new List<FeedbackQuestion>();

    public ICollection<StudentFeedback> StudentFeedbacks { get; set; } = new List<StudentFeedback>();

    public ICollection<ParentFeedback> ParentFeedbacks { get; set; } = new List<ParentFeedback>();

    public ICollection<FeedbackSummary> Summaries { get; set; } = new List<FeedbackSummary>();
}
