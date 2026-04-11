using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>One official scheduled exam for a class division and subject.</summary>
public class ScheduledExam
{
    public int ScheduledExamID { get; set; }

    public int? ExamSessionID { get; set; }
    public int ExamTypeID { get; set; }
    public int YearID { get; set; }
    public int TermID { get; set; }
    public int ClassID { get; set; }
    public int DivisionID { get; set; }
    public int SubjectID { get; set; }
    public int TeacherID { get; set; }

    public DateTime ExamDate { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string? Room { get; set; }

    public decimal TotalMarks { get; set; }
    public decimal PassingMarks { get; set; }

    /// <summary>When true, students/guardians may see date/time/room on calendars.</summary>
    public bool SchedulePublished { get; set; }

    /// <summary>When true, students/guardians may see marks and remarks.</summary>
    public bool ResultsPublished { get; set; }

    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    [JsonIgnore]
    public ExamSession? ExamSession { get; set; }

    [JsonIgnore]
    public ExamType ExamType { get; set; } = null!;

    [JsonIgnore]
    public Year Year { get; set; } = null!;

    [JsonIgnore]
    public Term Term { get; set; } = null!;

    [JsonIgnore]
    public Class Class { get; set; } = null!;

    [JsonIgnore]
    public Division Division { get; set; } = null!;

    [JsonIgnore]
    public Subject Subject { get; set; } = null!;

    [JsonIgnore]
    public Teacher Teacher { get; set; } = null!;

    public ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
}
