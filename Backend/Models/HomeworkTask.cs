using System;
using System.Collections.Generic;

namespace Backend.Models;

public class HomeworkTask
{
    public int HomeworkTaskID { get; set; }
    public int TeacherID { get; set; }
    public Teacher Teacher { get; set; } = null!;

    public int YearID { get; set; }
    public Year Year { get; set; } = null!;

    public int TermID { get; set; }
    public Term Term { get; set; } = null!;

    public int ClassID { get; set; }
    public Class Class { get; set; } = null!;

    public int DivisionID { get; set; }
    public Division Division { get; set; } = null!;

    public int SubjectID { get; set; }
    public Subject Subject { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Due instant in UTC (date comparisons typically use .Date).</summary>
    public DateTime DueDateUtc { get; set; }

    public bool SubmissionRequired { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<HomeworkTaskLink> Links { get; set; } = new List<HomeworkTaskLink>();
    public ICollection<HomeworkSubmission> Submissions { get; set; } = new List<HomeworkSubmission>();
}
