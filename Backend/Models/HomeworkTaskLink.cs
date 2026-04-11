namespace Backend.Models;

public class HomeworkTaskLink
{
    public int HomeworkTaskLinkID { get; set; }
    public int HomeworkTaskID { get; set; }
    public HomeworkTask HomeworkTask { get; set; } = null!;

    public string Url { get; set; } = string.Empty;
    public string? Label { get; set; }
    public int SortOrder { get; set; }
}
