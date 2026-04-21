using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>
/// Supervisor visit to a teacher’s class/subject context; scored out of 100 with structured observations and recommendations.
/// </summary>
public class SupervisorVisit
{
    public int SupervisorVisitID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [Required]
    public int AcademicYearID { get; set; }

    [JsonIgnore]
    public Year AcademicYear { get; set; } = null!;

    /// <summary>Visited teacher (المعلم).</summary>
    [Required]
    public int VisitedTeacherID { get; set; }

    [JsonIgnore]
    public Teacher VisitedTeacher { get; set; } = null!;

    /// <summary>Optional class (الصف) observed.</summary>
    public int? ClassID { get; set; }

    [JsonIgnore]
    public Class? Class { get; set; }

    /// <summary>Optional subject (المادة) observed.</summary>
    public int? SubjectID { get; set; }

    [JsonIgnore]
    public Subject? Subject { get; set; }

    /// <summary>Supervisor conducting the visit (HR / manager profile).</summary>
    [Required]
    public int SupervisorEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile SupervisorEmployeeProfile { get; set; } = null!;

    [Required]
    public DateOnly VisitDate { get; set; }

    public SupervisorVisitStatus Status { get; set; } = SupervisorVisitStatus.Draft;

    /// <summary>Final visit score out of 100 (الدرجة النهائية من 100).</summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal OverallScoreOutOf100 { get; set; }

    [MaxLength(4000)]
    public string? SummaryNotes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<VisitObservation> Observations { get; set; } = new List<VisitObservation>();

    public ICollection<VisitRecommendation> Recommendations { get; set; } = new List<VisitRecommendation>();
}
