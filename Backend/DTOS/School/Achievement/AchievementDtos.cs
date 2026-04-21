using System.ComponentModel.DataAnnotations;

namespace Backend.DTOS.School.Achievement;

public class AchievementRequestFilterDto
{
    public int? SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public int? EmployeeProfileID { get; set; }
    public int? Status { get; set; }
}

/// <summary>Load catalog achievements for the request form (school-scoped).</summary>
public class AchievementCatalogFilterDto
{
    [Required]
    public int SchoolID { get; set; }

    public int? AcademicYearID { get; set; }
}

public class AchievementCatalogItemDto
{
    public int AchievementID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int DefaultPoints { get; set; }
    public int? AcademicYearID { get; set; }
}

public class AchievementRequestListItemDto
{
    public int AchievementRequestID { get; set; }
    public int SchoolID { get; set; }
    public int AcademicYearID { get; set; }
    public int EmployeeProfileID { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int? AchievementID { get; set; }
    public string? AchievementTitle { get; set; }
    public string? CustomTitle { get; set; }
    public int Status { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
}

public class AchievementApprovalReadDto
{
    public int AchievementApprovalID { get; set; }
    public int ApproverEmployeeProfileID { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public int Decision { get; set; }
    public string? Comment { get; set; }
    public int SortOrder { get; set; }
    public DateTime? DecidedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class AchievementAttachmentReadDto
{
    public int AchievementAttachmentID { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long? FileSizeBytes { get; set; }
    public DateTime UploadedAtUtc { get; set; }
}

public class AchievementPointsLedgerReadDto
{
    public int AchievementPointsLedgerID { get; set; }
    public int DeltaPoints { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public class AchievementRequestDetailDto : AchievementRequestListItemDto
{
    public string? Notes { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public List<AchievementApprovalReadDto> Approvals { get; set; } = new();
    public List<AchievementAttachmentReadDto> Attachments { get; set; } = new();
    public List<AchievementPointsLedgerReadDto> LedgerEntries { get; set; } = new();
}

public class AchievementRequestWriteDto
{
    [Required]
    public int SchoolID { get; set; }

    /// <summary>When null or not set, the server uses the school active academic year (same rules as listing).</summary>
    public int? AcademicYearID { get; set; }

    [Required]
    public int EmployeeProfileID { get; set; }

    public int? AchievementID { get; set; }

    [MaxLength(256)]
    public string? CustomTitle { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public int Status { get; set; }
}
