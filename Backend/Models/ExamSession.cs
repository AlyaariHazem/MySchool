using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Groups exams within a year/term (e.g. midterm week).</summary>
public class ExamSession
{
    public int ExamSessionID { get; set; }
    public int YearID { get; set; }
    public int TermID { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    [JsonIgnore]
    public Year Year { get; set; } = null!;

    [JsonIgnore]
    public Term Term { get; set; } = null!;

    public ICollection<ScheduledExam> ScheduledExams { get; set; } = new List<ScheduledExam>();
}
