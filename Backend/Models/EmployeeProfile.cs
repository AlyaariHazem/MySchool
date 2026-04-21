using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>
/// Unified HR profile (aggregate root) for the School Performance Analysis System.
/// Optionally links to one legacy row (<see cref="Teacher"/>, <see cref="Manager"/>, or <see cref="SchoolStaff"/>); at most one should be set.
/// </summary>
public class EmployeeProfile
{
    public int EmployeeProfileID { get; set; }

    /// <summary>Identity user id (admin DB); stored without FK in tenant DB, same pattern as <see cref="Teacher.UserID"/>.</summary>
    [MaxLength(450)]
    public string? UserId { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    /// <summary>Current / default academic year context for this profile.</summary>
    [Required]
    public int CurrentAcademicYearID { get; set; }

    [JsonIgnore]
    public Year CurrentAcademicYear { get; set; } = null!;

    [Required]
    public int EmployeeJobTypeID { get; set; }

    [JsonIgnore]
    public EmployeeJobType JobType { get; set; } = null!;

    [Required]
    [MaxLength(64)]
    public string EmployeeCode { get; set; } = string.Empty;

    public Name FullName { get; set; } = null!;

    public NameAlis? FullNameAlis { get; set; }

    [MaxLength(64)]
    public string? NationalId { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(32)]
    public string? Gender { get; set; }

    [MaxLength(64)]
    public string? Phone { get; set; }

    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(512)]
    public string? Address { get; set; }

    public DateTime? HireDate { get; set; }

    public EmploymentStatus EmploymentStatus { get; set; } = EmploymentStatus.Active;

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Optional link to legacy row. DB uses NO ACTION (SQL Server cannot combine SET NULL here with Teacher→Manager CASCADE).</summary>
    public int? TeacherID { get; set; }

    [JsonIgnore]
    public Teacher? Teacher { get; set; }

    public int? ManagerID { get; set; }

    [JsonIgnore]
    public Manager? Manager { get; set; }

    public int? SchoolStaffID { get; set; }

    [JsonIgnore]
    public SchoolStaff? SchoolStaff { get; set; }

    public ICollection<EmployeeQualification> Qualifications { get; set; } = new List<EmployeeQualification>();

    public ICollection<EmployeeSpecialization> Specializations { get; set; } = new List<EmployeeSpecialization>();

    public ICollection<EmployeeHistory> HistoryRecords { get; set; } = new List<EmployeeHistory>();

    public ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();

    public ICollection<EmployeeLeave> Leaves { get; set; } = new List<EmployeeLeave>();

    public ICollection<EmployeePerformanceSummary> PerformanceSummaries { get; set; } = new List<EmployeePerformanceSummary>();

    public ICollection<DailyEvaluation> DailyEvaluationsAsEvaluated { get; set; } = new List<DailyEvaluation>();

    public ICollection<DailyEvaluation> DailyEvaluationsAsEvaluator { get; set; } = new List<DailyEvaluation>();

    public ICollection<SupervisorVisit> SupervisorVisitsConducted { get; set; } = new List<SupervisorVisit>();

    public ICollection<RecommendationFollowUp> RecommendationFollowUpsAuthored { get; set; } = new List<RecommendationFollowUp>();

    public ICollection<AchievementRequest> AchievementRequests { get; set; } = new List<AchievementRequest>();

    public ICollection<AchievementApproval> AchievementApprovalsAsApprover { get; set; } = new List<AchievementApproval>();

    public ICollection<AchievementAttachment> AchievementAttachmentsUploaded { get; set; } = new List<AchievementAttachment>();

    public ICollection<AchievementPointsLedger> AchievementPointsLedgerEntries { get; set; } = new List<AchievementPointsLedger>();

    public ICollection<AchievementPointsLedger> AchievementPointsLedgerEntriesCreated { get; set; } = new List<AchievementPointsLedger>();

    public ICollection<Violation> ViolationsAsSubject { get; set; } = new List<Violation>();

    public ICollection<Violation> ViolationsOpened { get; set; } = new List<Violation>();

    public ICollection<ViolationResponse> ViolationResponses { get; set; } = new List<ViolationResponse>();

    public ICollection<ViolationAction> ViolationActionsPerformed { get; set; } = new List<ViolationAction>();

    public ICollection<ViolationEscalationHistory> ViolationEscalationsChanged { get; set; } = new List<ViolationEscalationHistory>();
}
