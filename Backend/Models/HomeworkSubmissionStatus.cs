namespace Backend.Models;

/// <summary>Per-student homework workflow state.</summary>
public enum HomeworkSubmissionStatus : byte
{
    Pending = 0,
    Submitted = 1,
    Late = 2,
    Graded = 3,
    Completed = 4,
    Missing = 5
}
