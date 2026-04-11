using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Per-student mark row for a scheduled exam.</summary>
public class ExamResult
{
    public int ExamResultID { get; set; }
    public int ScheduledExamID { get; set; }
    public int StudentID { get; set; }

    public decimal? Score { get; set; }
    public bool IsAbsent { get; set; }
    public string? Remarks { get; set; }

    [JsonIgnore]
    public ScheduledExam ScheduledExam { get; set; } = null!;

    [JsonIgnore]
    public Student Student { get; set; } = null!;
}
