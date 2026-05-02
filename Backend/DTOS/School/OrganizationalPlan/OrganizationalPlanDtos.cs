using System.ComponentModel.DataAnnotations;

namespace Backend.DTOS.School.OrganizationalPlan;

// ----- Strategic goals -----

public class StrategicGoalFilterDto
{
    public int? SchoolID { get; set; }
    public int? Status { get; set; }
}

public class StrategicGoalListItemDto
{
    public int StrategicGoalID { get; set; }
    public int SchoolID { get; set; }
    public string? ReferenceCode { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public int SortOrder { get; set; }
    public DateTime? EffectiveFromUtc { get; set; }
    public DateTime? EffectiveToUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public class StrategicGoalDetailDto : StrategicGoalListItemDto
{
    public string? Details { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class StrategicGoalWriteDto
{
    [Required]
    public int SchoolID { get; set; }

    [MaxLength(64)]
    public string? ReferenceCode { get; set; }

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Details { get; set; }

    [Required]
    public int Status { get; set; }

    public int SortOrder { get; set; }

    public DateTime? EffectiveFromUtc { get; set; }

    public DateTime? EffectiveToUtc { get; set; }
}

// ----- Annual goals (with operational tree) -----

public class AnnualGoalFilterDto
{
    public int? SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public int? StrategicGoalID { get; set; }
    public int? Status { get; set; }
}

public class AnnualGoalListItemDto
{
    public int AnnualGoalID { get; set; }
    public int SchoolID { get; set; }
    public int AcademicYearID { get; set; }
    public int? StrategicGoalID { get; set; }
    public string? StrategicGoalTitle { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public int SortOrder { get; set; }
    public int OperationalPlanCount { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public class PlanProgressUpdateReadDto
{
    public int PlanProgressUpdateID { get; set; }
    public int PlanTaskID { get; set; }
    public string? Note { get; set; }
    public int? ProgressPercent { get; set; }
    public int? AuthorEmployeeProfileID { get; set; }
    public string? AuthorName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class PlanTaskReadDto
{
    public int PlanTaskID { get; set; }
    public int OperationalPlanID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Details { get; set; }
    public int Status { get; set; }
    public int SortOrder { get; set; }
    public int ProgressPercent { get; set; }
    public DateTime? DueAtUtc { get; set; }
    public int? AssignedToEmployeeProfileID { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public List<PlanProgressUpdateReadDto> ProgressUpdates { get; set; } = new();
}

public class OperationalPlanReadDto
{
    public int OperationalPlanID { get; set; }
    public int AnnualGoalID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Details { get; set; }
    public int Status { get; set; }
    public int SortOrder { get; set; }
    public DateTime? StartDateUtc { get; set; }
    public DateTime? EndDateUtc { get; set; }
    public int? OwnerEmployeeProfileID { get; set; }
    public string? OwnerName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public List<PlanTaskReadDto> Tasks { get; set; } = new();
}

public class AnnualGoalDetailDto : AnnualGoalListItemDto
{
    public string? Details { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<OperationalPlanReadDto> OperationalPlans { get; set; } = new();
}

public class PlanProgressUpdateWriteDto
{
    [MaxLength(4000)]
    public string? Note { get; set; }

    public int? ProgressPercent { get; set; }

    public int? AuthorEmployeeProfileID { get; set; }
}

public class PlanTaskWriteDto
{
    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Details { get; set; }

    [Required]
    public int Status { get; set; }

    public int SortOrder { get; set; }

    public int ProgressPercent { get; set; }

    public DateTime? DueAtUtc { get; set; }

    public int? AssignedToEmployeeProfileID { get; set; }

    public List<PlanProgressUpdateWriteDto> ProgressUpdates { get; set; } = new();
}

public class OperationalPlanWriteDto
{
    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Details { get; set; }

    [Required]
    public int Status { get; set; }

    public int SortOrder { get; set; }

    public DateTime? StartDateUtc { get; set; }

    public DateTime? EndDateUtc { get; set; }

    public int? OwnerEmployeeProfileID { get; set; }

    public List<PlanTaskWriteDto> Tasks { get; set; } = new();
}

public class AnnualGoalWriteDto
{
    [Required]
    public int SchoolID { get; set; }

    public int? StrategicGoalID { get; set; }

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Details { get; set; }

    [Required]
    public int Status { get; set; }

    public int SortOrder { get; set; }

    public List<OperationalPlanWriteDto> OperationalPlans { get; set; } = new();
}

// ----- Department goals -----

public class DepartmentGoalFilterDto
{
    public int? SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public int? Status { get; set; }
}

public class DepartmentGoalListItemDto
{
    public int DepartmentGoalID { get; set; }
    public int SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public int? StrategicGoalID { get; set; }
    public int? AnnualGoalID { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public int SortOrder { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public class DepartmentGoalDetailDto : DepartmentGoalListItemDto
{
    public string? Details { get; set; }
    public int? OwnerEmployeeProfileID { get; set; }
    public string? OwnerName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class DepartmentGoalWriteDto
{
    [Required]
    public int SchoolID { get; set; }

    public int? StrategicGoalID { get; set; }

    public int? AnnualGoalID { get; set; }

    [Required]
    [MaxLength(256)]
    public string DepartmentName { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Details { get; set; }

    [Required]
    public int Status { get; set; }

    public int SortOrder { get; set; }

    public int? OwnerEmployeeProfileID { get; set; }
}
