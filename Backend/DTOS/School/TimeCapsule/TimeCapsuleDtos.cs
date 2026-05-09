using Backend.Models;

namespace Backend.DTOS.School.TimeCapsule;

public sealed class TimeCapsuleStatusDto
{
    public int EmployeeProfileId { get; set; }
    public int? TimeCapsuleId { get; set; }
    /// <summary>LockedNoResignation | ResignationPending | ResignationRejected | UnlockPending | Unlocked | UnlockRejected</summary>
    public string Phase { get; set; } = string.Empty;
    public string MessageAr { get; set; } = string.Empty;
    public int? ResignationRequestId { get; set; }
    public ResignationRequestStatus? ResignationStatus { get; set; }
    public int? PendingUnlockApprovalId { get; set; }
    public bool IsUnlocked { get; set; }
}

public sealed class ResignationRequestCreateDto
{
    public int EmployeeProfileId { get; set; }
    public int AcademicYearId { get; set; }
    public string? Reason { get; set; }
}

public sealed class ResignationRequestReadDto
{
    public int ResignationRequestId { get; set; }
    public int EmployeeProfileId { get; set; }
    public int SchoolId { get; set; }
    public int AcademicYearId { get; set; }
    public DateTime RequestDateUtc { get; set; }
    public string? Reason { get; set; }
    public ResignationRequestStatus Status { get; set; }
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public string? Notes { get; set; }
}

public sealed class TimeCapsuleSectionReadDto
{
    public int TimeCapsuleSectionId { get; set; }
    public TimeCapsuleSectionType SectionType { get; set; }
    public string Title { get; set; } = string.Empty;
    /// <summary>Snapshot JSON (parse on the client for charts/timeline).</summary>
    public string DataJson { get; set; } = "{}";
    public int SortOrder { get; set; }
}

public sealed class TimeCapsuleDetailDto
{
    public int TimeCapsuleId { get; set; }
    public int EmployeeProfileId { get; set; }
    public int SchoolId { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? UnlockedAtUtc { get; set; }
    public string? UnlockedByUserId { get; set; }
    public string? UnlockReason { get; set; }
    public IReadOnlyList<TimeCapsuleSectionReadDto> Sections { get; set; } = Array.Empty<TimeCapsuleSectionReadDto>();
    public string? NarrativeText { get; set; }
    public DateTime? NarrativeGeneratedAtUtc { get; set; }
    public CapsuleNarrativeGeneratedBy? NarrativeGeneratedBy { get; set; }
}

public sealed class CapsuleAccessLogReadDto
{
    public int CapsuleAccessLogId { get; set; }
    public string AccessedByUserId { get; set; } = string.Empty;
    public DateTime AccessedAtUtc { get; set; }
    public CapsuleAccessActionType ActionType { get; set; }
    public string? Notes { get; set; }
}

public sealed class ApproveRejectNotesDto
{
    public string? Notes { get; set; }
}

public sealed class CapsuleUnlockApproveDto
{
    public string? UnlockReason { get; set; }
    public string? Notes { get; set; }
}
