using System.ComponentModel.DataAnnotations;

namespace Backend.DTOS.School.EmployeeRequest;

public class EmployeeRequestTypesFilterDto
{
    [Required]
    public int SchoolID { get; set; }
}

public class EmployeeRequestTypeListItemDto
{
    public int RequestTypeID { get; set; }
    public int SchoolID { get; set; }
    public string Code { get; set; } = string.Empty;
    public int Category { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class EmployeeRequestFilterDto
{
    public int? SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public int? Status { get; set; }
    public int? EmployeeProfileID { get; set; }
}

public class EmployeeRequestListItemDto
{
    public int EmployeeRequestID { get; set; }
    public int SchoolID { get; set; }
    public int AcademicYearID { get; set; }
    public int EmployeeProfileID { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int RequestTypeID { get; set; }
    public string RequestTypeCode { get; set; } = string.Empty;
    public int RequestTypeCategory { get; set; }
    public string RequestTypeName { get; set; } = string.Empty;
    public string? RequestTypeNameAr { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public decimal? RequestedAmount { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
}

public class EmployeeRequestApprovalReadDto
{
    public int RequestApprovalStepID { get; set; }
    public int EmployeeRequestID { get; set; }
    public int ApproverEmployeeProfileID { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public int StepOrder { get; set; }
    public int Decision { get; set; }
    public string? Comment { get; set; }
    public DateTime? DecidedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class EmployeeRequestExecutionReadDto
{
    public int RequestExecutionID { get; set; }
    public int EmployeeRequestID { get; set; }
    public int Status { get; set; }
    public string? Notes { get; set; }
    public int ProgressPercent { get; set; }
    public DateTime? DueAtUtc { get; set; }
    public DateTime? ExecutedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public int? ResponsibleEmployeeProfileID { get; set; }
    public string? ResponsibleName { get; set; }
}

public class EmployeeRequestDailySummaryReadDto
{
    public int RequestDailySummaryID { get; set; }
    public int EmployeeRequestID { get; set; }
    public DateTime SummaryDate { get; set; }
    public string Summary { get; set; } = string.Empty;
    public int? ProgressPercent { get; set; }
    public bool IsFinalForDay { get; set; }
    public int? CreatedByEmployeeProfileID { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class EmployeeRequestDetailDto : EmployeeRequestListItemDto
{
    public string? Details { get; set; }
    public List<EmployeeRequestApprovalReadDto> ApprovalSteps { get; set; } = new();
    public List<EmployeeRequestExecutionReadDto> Executions { get; set; } = new();
    public List<EmployeeRequestDailySummaryReadDto> DailySummaries { get; set; } = new();
}

public class EmployeeRequestWriteDto
{
    [Required]
    public int SchoolID { get; set; }

    public int? AcademicYearID { get; set; }

    [Required]
    public int EmployeeProfileID { get; set; }

    [Required]
    public int RequestTypeID { get; set; }

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Details { get; set; }

    public decimal? RequestedAmount { get; set; }

    public int Status { get; set; }
}

public class EmployeeRequestExecutionWriteDto
{
    [Required]
    public int Status { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [Range(0, 100)]
    public int ProgressPercent { get; set; }

    public DateTime? DueAtUtc { get; set; }

    public int? ResponsibleEmployeeProfileID { get; set; }
}

public class EmployeeRequestDailySummaryWriteDto
{
    public DateTime SummaryDate { get; set; }

    [Required]
    [MaxLength(4000)]
    public string Summary { get; set; } = string.Empty;

    [Range(0, 100)]
    public int? ProgressPercent { get; set; }

    public bool IsFinalForDay { get; set; }

    public int? CreatedByEmployeeProfileID { get; set; }
}

public class EmployeeRequestApprovalStepWriteDto
{
    [Required]
    public int ApproverEmployeeProfileID { get; set; }

    public int StepOrder { get; set; }
}

public class EmployeeRequestApprovalDecideDto
{
    /// <summary>1 = Approved, 2 = Rejected (see <see cref="Models.RequestApprovalDecision"/>).</summary>
    [Required]
    public int Decision { get; set; }

    [MaxLength(2000)]
    public string? Comment { get; set; }
}
