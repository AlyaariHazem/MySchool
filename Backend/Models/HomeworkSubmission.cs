using System;
using System.Collections.Generic;

namespace Backend.Models;

public class HomeworkSubmission
{
    public int HomeworkSubmissionID { get; set; }

    public int HomeworkTaskID { get; set; }
    public HomeworkTask HomeworkTask { get; set; } = null!;

    public int StudentID { get; set; }
    public Student Student { get; set; } = null!;

    public HomeworkSubmissionStatus Status { get; set; }

    public DateTime? SubmittedAtUtc { get; set; }

    public string? AnswerText { get; set; }

    public string? TeacherFeedback { get; set; }
    public decimal? Score { get; set; }

    /// <summary>When true, guardian can read teacher feedback (and score if graded).</summary>
    public bool FeedbackPublished { get; set; }

    public DateTime? ReviewedAtUtc { get; set; }

    public ICollection<HomeworkSubmissionFile> Files { get; set; } = new List<HomeworkSubmissionFile>();
}
