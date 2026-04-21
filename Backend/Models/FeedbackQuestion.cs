using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

public class FeedbackQuestion
{
    public int FeedbackQuestionID { get; set; }

    [Required]
    public int TeacherFeedbackCycleID { get; set; }

    [JsonIgnore]
    public TeacherFeedbackCycle TeacherFeedbackCycle { get; set; } = null!;

    public int SortOrder { get; set; }

    [Required]
    [MaxLength(1000)]
    public string QuestionText { get; set; } = string.Empty;

    public FeedbackQuestionType QuestionType { get; set; } = FeedbackQuestionType.Rating1To5;

    public FeedbackQuestionAudience Audience { get; set; } = FeedbackQuestionAudience.Both;

    public bool IsRequired { get; set; } = true;
}
