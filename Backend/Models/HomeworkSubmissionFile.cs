namespace Backend.Models;

public class HomeworkSubmissionFile
{
    public int HomeworkSubmissionFileID { get; set; }

    public int HomeworkSubmissionID { get; set; }
    public HomeworkSubmission HomeworkSubmission { get; set; } = null!;

    public string FileUrl { get; set; } = string.Empty;
    public string? FileName { get; set; }
}
