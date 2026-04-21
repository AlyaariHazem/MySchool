using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Audit trail of achievement point movements for an employee.</summary>
public class AchievementPointsLedger
{
    public int AchievementPointsLedgerID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [Required]
    public int AcademicYearID { get; set; }

    [JsonIgnore]
    public Year AcademicYear { get; set; } = null!;

    [Required]
    public int EmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile EmployeeProfile { get; set; } = null!;

    /// <summary>Positive for awards, negative for corrections.</summary>
    public int DeltaPoints { get; set; }

    [Required]
    [MaxLength(512)]
    public string Reason { get; set; } = string.Empty;

    public int? AchievementRequestID { get; set; }

    [JsonIgnore]
    public AchievementRequest? AchievementRequest { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public int? CreatedByEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile? CreatedByEmployeeProfile { get; set; }
}
