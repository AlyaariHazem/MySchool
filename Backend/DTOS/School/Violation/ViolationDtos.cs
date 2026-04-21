using System.ComponentModel.DataAnnotations;

namespace Backend.DTOS.School.Violation;

public class ViolationTypesFilterDto
{
    [Required]
    public int SchoolID { get; set; }
}

public class ViolationTypeListItemDto
{
    public int ViolationTypeID { get; set; }
    public int SchoolID { get; set; }
    public int Kind { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class ViolationFilterDto
{
    public int? SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public int? SubjectEmployeeProfileID { get; set; }
    public int? Status { get; set; }
}

public class ViolationListItemDto
{
    public int ViolationID { get; set; }
    public int SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public int ViolationTypeID { get; set; }
    public int ViolationTypeKind { get; set; }
    public string ViolationTypeName { get; set; } = string.Empty;
    public int SubjectEmployeeProfileID { get; set; }
    public string SubjectEmployeeName { get; set; } = string.Empty;
    public int? OpenedByEmployeeProfileID { get; set; }
    public string? OpenedByName { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime OpenedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
}

public class ViolationResponseReadDto
{
    public int ViolationResponseID { get; set; }
    public int ViolationID { get; set; }
    public int? AuthorEmployeeProfileID { get; set; }
    public string? AuthorName { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public class ViolationActionReadDto
{
    public int ViolationActionID { get; set; }
    public int ViolationID { get; set; }
    public int Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int PerformedByEmployeeProfileID { get; set; }
    public string PerformedByName { get; set; } = string.Empty;
    public DateTime PerformedAtUtc { get; set; }
}

public class ViolationEscalationHistoryReadDto
{
    public int ViolationEscalationHistoryID { get; set; }
    public int ViolationID { get; set; }
    public int? PreviousViolationTypeID { get; set; }
    public int? PreviousKind { get; set; }
    public string? PreviousTypeName { get; set; }
    public int NewViolationTypeID { get; set; }
    public int NewKind { get; set; }
    public string NewTypeName { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public int ChangedByEmployeeProfileID { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public DateTime ChangedAtUtc { get; set; }
}

public class ViolationDetailDto : ViolationListItemDto
{
    public string? Details { get; set; }
    public List<ViolationResponseReadDto> Responses { get; set; } = new();
    public List<ViolationActionReadDto> Actions { get; set; } = new();
    public List<ViolationEscalationHistoryReadDto> EscalationHistory { get; set; } = new();
}

public class ViolationWriteDto
{
    [Required]
    public int SchoolID { get; set; }

    public int? AcademicYearID { get; set; }

    [Required]
    public int SubjectEmployeeProfileID { get; set; }

    public int? OpenedByEmployeeProfileID { get; set; }

    /// <summary>On create: 0 means Clarification for the school. Ignored on update.</summary>
    public int ViolationTypeID { get; set; }

    [Required]
    [MaxLength(512)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Details { get; set; }

    public int Status { get; set; }
}

public class ViolationResponseWriteDto
{
    [Required]
    [MaxLength(4000)]
    public string Body { get; set; } = string.Empty;

    public int? AuthorEmployeeProfileID { get; set; }
}

public class ViolationActionWriteDto
{
    public int Category { get; set; }

    [Required]
    [MaxLength(512)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Notes { get; set; }

    [Required]
    public int PerformedByEmployeeProfileID { get; set; }
}

public class ViolationEscalateDto
{
    [Required]
    public int NewViolationTypeID { get; set; }

    [Required]
    public int ChangedByEmployeeProfileID { get; set; }

    [MaxLength(2000)]
    public string? Reason { get; set; }
}
