namespace Backend.Models;

/// <summary>School-defined exam categories (midterm, final, quiz, …).</summary>
public class ExamType
{
    public int ExamTypeID { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ScheduledExam> ScheduledExams { get; set; } = new List<ScheduledExam>();
}
