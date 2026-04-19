using Backend.Models;

namespace Backend.DTOS.School.DailyEvaluation;

// --- Templates ---

public class DailyEvaluationTemplateFilterDto
{
    public int? SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public int? EmployeeJobTypeID { get; set; }
    public EvaluationTemplateStatus? Status { get; set; }
    public bool? IsActive { get; set; }
}

public class DailyEvaluationTemplateCreateDto
{
    public int SchoolID { get; set; }
    public int AcademicYearID { get; set; }
    public int? EmployeeJobTypeID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsDefault { get; set; }
}

public class DailyEvaluationTemplateUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

public class DailyEvaluationTemplateReadDto
{
    public int DailyEvaluationTemplateID { get; set; }
    public int SchoolID { get; set; }
    public int AcademicYearID { get; set; }
    public int? EmployeeJobTypeID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EvaluationTemplateStatus Status { get; set; }
    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public class DailyEvaluationTemplateListDto
{
    public int DailyEvaluationTemplateID { get; set; }
    public int SchoolID { get; set; }
    public int AcademicYearID { get; set; }
    public string Name { get; set; } = string.Empty;
    public EvaluationTemplateStatus Status { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

// --- Criteria ---

public class DailyEvaluationCriteriaCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Weight { get; set; } = 1m;
    public decimal MaxScore { get; set; } = 10m;
    public decimal MinScore { get; set; } = 0m;
    public bool IsMandatory { get; set; }
    public int SortOrder { get; set; }
    public string? Notes { get; set; }
}

public class DailyEvaluationCriteriaUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Weight { get; set; } = 1m;
    public decimal MaxScore { get; set; } = 10m;
    public decimal MinScore { get; set; } = 0m;
    public bool IsMandatory { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}

public class DailyEvaluationCriteriaReadDto
{
    public int DailyEvaluationCriteriaID { get; set; }
    public int DailyEvaluationTemplateID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Weight { get; set; }
    public decimal MaxScore { get; set; }
    public decimal MinScore { get; set; }
    public bool IsMandatory { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

// --- Evaluations ---

public class DailyEvaluationFilterDto
{
    public int? SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public int? EvaluatedEmployeeProfileID { get; set; }
    public int? DailyEvaluationTemplateID { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public DailyEvaluationStatus? Status { get; set; }

    /// <summary>When set, only evaluations created by this identity user (e.g. student evaluators).</summary>
    public string? EvaluatorUserId { get; set; }
}

/// <summary>Teacher (HR profile) pick list for student-submitted daily evaluations.</summary>
public class TeacherEvaluationOptionDto
{
    public int EmployeeProfileID { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}

public class DailyEvaluationCreateDto
{
    public int SchoolID { get; set; }
    public int AcademicYearID { get; set; }
    public int EvaluatedEmployeeProfileID { get; set; }
    public int DailyEvaluationTemplateID { get; set; }
    public DateOnly EvaluationDate { get; set; }
    public string? EvaluatorUserId { get; set; }
    public int? EvaluatorEmployeeProfileID { get; set; }
    public string? Notes { get; set; }
}

public class DailyEvaluationUpdateDto
{
    public string? Notes { get; set; }
}

public class DailyEvaluationReadDto
{
    public int DailyEvaluationID { get; set; }
    public int SchoolID { get; set; }
    public int AcademicYearID { get; set; }
    public int EvaluatedEmployeeProfileID { get; set; }
    public string? EvaluatorUserId { get; set; }
    public int? EvaluatorEmployeeProfileID { get; set; }
    public int DailyEvaluationTemplateID { get; set; }
    public DateOnly EvaluationDate { get; set; }
    public DailyEvaluationStatus Status { get; set; }
    public decimal TotalScore { get; set; }
    public string? Notes { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }
    public DateTime? LockedAtUtc { get; set; }
    public bool IsLocked { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public class DailyEvaluationListDto
{
    public int DailyEvaluationID { get; set; }
    public int EvaluatedEmployeeProfileID { get; set; }
    public int DailyEvaluationTemplateID { get; set; }
    public DateOnly EvaluationDate { get; set; }
    public DailyEvaluationStatus Status { get; set; }
    public decimal TotalScore { get; set; }
    public bool IsLocked { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public class DailyEvaluationItemReadDto
{
    public int DailyEvaluationItemID { get; set; }
    public int DailyEvaluationID { get; set; }
    public int DailyEvaluationCriteriaID { get; set; }
    public string CriteriaName { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string? Comment { get; set; }
    public bool IsMandatorySatisfied { get; set; }
}

public class DailyEvaluationFullDto : DailyEvaluationReadDto
{
    public IReadOnlyList<DailyEvaluationItemReadDto> Items { get; set; } = Array.Empty<DailyEvaluationItemReadDto>();
}

public class DailyEvaluationItemCreateDto
{
    public int DailyEvaluationCriteriaID { get; set; }
    public decimal Score { get; set; }
    public string? Comment { get; set; }
}

public class DailyEvaluationItemUpdateDto
{
    public decimal Score { get; set; }
    public string? Comment { get; set; }
}

public class DailyEvaluationItemPatchDto
{
    public int DailyEvaluationItemID { get; set; }
    public decimal Score { get; set; }
    public string? Comment { get; set; }
}

public class EvaluationOverrideRequestDto
{
    public string Reason { get; set; } = string.Empty;
    public DailyEvaluationUpdateDto? Evaluation { get; set; }
    public List<DailyEvaluationItemPatchDto>? Items { get; set; }
    public string? Notes { get; set; }
}

public class EvaluationOverrideLogReadDto
{
    public int EvaluationOverrideLogID { get; set; }
    public int? DailyEvaluationID { get; set; }
    public int? EvaluationLockID { get; set; }
    public EvaluationOverrideActionType OverrideActionType { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? PreviousValuesJson { get; set; }
    public string? NewValuesJson { get; set; }
    public string PerformedByUserId { get; set; } = string.Empty;
    public DateTime PerformedAtUtc { get; set; }
}

// --- Locks ---

public class EvaluationLockCreateDto
{
    public int SchoolID { get; set; }
    public int AcademicYearID { get; set; }
    public DateOnly LockDate { get; set; }
    public int? DailyEvaluationTemplateID { get; set; }
    public string? Notes { get; set; }
}

public class EvaluationLockReadDto
{
    public int EvaluationLockID { get; set; }
    public int SchoolID { get; set; }
    public int AcademicYearID { get; set; }
    public DateOnly LockDate { get; set; }
    public int? DailyEvaluationTemplateID { get; set; }
    public EvaluationLockStatus Status { get; set; }
    public DateTime? LockedAtUtc { get; set; }
    public string? LockedByUserId { get; set; }
    public DateTime? ReopenedAtUtc { get; set; }
    public string? ReopenedByUserId { get; set; }
    public string? Notes { get; set; }
}

public class EvaluationReopenDto
{
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
