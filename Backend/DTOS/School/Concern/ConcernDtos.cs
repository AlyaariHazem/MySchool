using System.ComponentModel.DataAnnotations;

namespace Backend.DTOS.School.Concern;

public class ConcernCategoriesFilterDto
{
    [Required]
    public int SchoolID { get; set; }

    /// <summary>0 = complaint, 1 = suggestion; omit to return all active categories.</summary>
    public int? CategoryKind { get; set; }
}

public class ConcernCategoryListItemDto
{
    public int ConcernCategoryID { get; set; }
    public int SchoolID { get; set; }
    public string Code { get; set; } = string.Empty;
    public int CategoryKind { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class ConcernFilterDto
{
    public int? SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public int? Status { get; set; }
    public int? SubmitterEmployeeProfileID { get; set; }
}

public class ComplaintListItemDto
{
    public int ComplaintID { get; set; }
    public int SchoolID { get; set; }
    public int AcademicYearID { get; set; }
    public int ConcernCategoryID { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryNameAr { get; set; }
    public int SubmitterEmployeeProfileID { get; set; }
    public string SubmitterName { get; set; } = string.Empty;
    public int? AssignedToEmployeeProfileID { get; set; }
    public string? AssignedToName { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
}

public class SuggestionListItemDto
{
    public int SuggestionID { get; set; }
    public int SchoolID { get; set; }
    public int AcademicYearID { get; set; }
    public int ConcernCategoryID { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryNameAr { get; set; }
    public int SubmitterEmployeeProfileID { get; set; }
    public string SubmitterName { get; set; } = string.Empty;
    public int? AssignedToEmployeeProfileID { get; set; }
    public string? AssignedToName { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
}

public class ConcernActionLogReadDto
{
    public int ConcernActionLogID { get; set; }
    public int ActionKind { get; set; }
    public int? OldStatus { get; set; }
    public int? NewStatus { get; set; }
    public string? Comment { get; set; }
    public int? ActorEmployeeProfileID { get; set; }
    public string? ActorName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class ComplaintDetailDto : ComplaintListItemDto
{
    public string? Details { get; set; }
    public List<ConcernActionLogReadDto> ActionLogs { get; set; } = new();
}

public class SuggestionDetailDto : SuggestionListItemDto
{
    public string? Details { get; set; }
    public List<ConcernActionLogReadDto> ActionLogs { get; set; } = new();
}

public class ComplaintWriteDto
{
    [Required]
    public int SchoolID { get; set; }

    public int? AcademicYearID { get; set; }

    [Required]
    public int ConcernCategoryID { get; set; }

    [Required]
    public int SubmitterEmployeeProfileID { get; set; }

    public int? AssignedToEmployeeProfileID { get; set; }

    [Required]
    [MaxLength(512)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Details { get; set; }

    [Required]
    public int Status { get; set; }
}

public class SuggestionWriteDto
{
    [Required]
    public int SchoolID { get; set; }

    public int? AcademicYearID { get; set; }

    [Required]
    public int ConcernCategoryID { get; set; }

    [Required]
    public int SubmitterEmployeeProfileID { get; set; }

    public int? AssignedToEmployeeProfileID { get; set; }

    [Required]
    [MaxLength(512)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Details { get; set; }

    [Required]
    public int Status { get; set; }
}
