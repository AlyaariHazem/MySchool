using System.ComponentModel.DataAnnotations;

namespace Backend.DTOS.School.Activity;

public class ActivityFilterDto
{
    public int? SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public int? Status { get; set; }
    public int? EmployeeProfileID { get; set; }
}

public class ActivityListItemDto
{
    public int ActivityRequestID { get; set; }
    public int SchoolID { get; set; }
    public int AcademicYearID { get; set; }
    public int EmployeeProfileID { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
}

public class ActivityApprovalReadDto
{
    public int ActivityApprovalID { get; set; }
    public int ActivityRequestID { get; set; }
    public int ApproverEmployeeProfileID { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public int Decision { get; set; }
    public string? Comment { get; set; }
    public DateTime? DecidedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class ActivityExecutionReadDto
{
    public int ActivityExecutionID { get; set; }
    public int ActivityRequestID { get; set; }
    public int Status { get; set; }
    public string? Notes { get; set; }
    public int ProgressPercent { get; set; }
    public DateTime? DueAtUtc { get; set; }
    public DateTime? ExecutedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public int? ResponsibleEmployeeProfileID { get; set; }
    public string? ResponsibleName { get; set; }
}

public class ActivityEvaluationReadDto
{
    public int ActivityEvaluationID { get; set; }
    public int ActivityRequestID { get; set; }
    public int EvaluatorEmployeeProfileID { get; set; }
    public string EvaluatorName { get; set; } = string.Empty;
    public int Score { get; set; }
    public string? Feedback { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class ActivityPointsReadDto
{
    public int ActivityPointsID { get; set; }
    public int ActivityRequestID { get; set; }
    public int Points { get; set; }
    public string? Reason { get; set; }
    public int AwardedByEmployeeProfileID { get; set; }
    public string AwardedByName { get; set; } = string.Empty;
    public DateTime AwardedAtUtc { get; set; }
}

public class ActivityDetailDto : ActivityListItemDto
{
    public string? Details { get; set; }
    public List<ActivityApprovalReadDto> Approvals { get; set; } = new();
    public List<ActivityExecutionReadDto> Executions { get; set; } = new();
    public List<ActivityEvaluationReadDto> Evaluations { get; set; } = new();
    public List<ActivityPointsReadDto> Points { get; set; } = new();
}

public class ActivityApprovalWriteDto
{
    [Required]
    public int ApproverEmployeeProfileID { get; set; }

    public int SortOrder { get; set; }

    [Required]
    public int Decision { get; set; }

    [MaxLength(2000)]
    public string? Comment { get; set; }

    public DateTime? DecidedAtUtc { get; set; }
}

public class ActivityExecutionWriteDto
{
    [Required]
    public int Status { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public int ProgressPercent { get; set; }

    public DateTime? DueAtUtc { get; set; }

    public DateTime? ExecutedAtUtc { get; set; }

    public int? ResponsibleEmployeeProfileID { get; set; }
}

public class ActivityEvaluationWriteDto
{
    [Required]
    public int EvaluatorEmployeeProfileID { get; set; }

    [Required]
    public int Score { get; set; }

    [MaxLength(4000)]
    public string? Feedback { get; set; }
}

public class ActivityPointsWriteDto
{
    [Required]
    public int Points { get; set; }

    [MaxLength(1000)]
    public string? Reason { get; set; }

    [Required]
    public int AwardedByEmployeeProfileID { get; set; }
}

public class ActivityRequestWriteDto
{
    [Required]
    public int SchoolID { get; set; }

    public int? AcademicYearID { get; set; }

    [Required]
    public int EmployeeProfileID { get; set; }

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Details { get; set; }

    [Required]
    public int Status { get; set; }

    public List<ActivityApprovalWriteDto> Approvals { get; set; } = new();

    public List<ActivityExecutionWriteDto> Executions { get; set; } = new();

    public List<ActivityEvaluationWriteDto> Evaluations { get; set; } = new();

    public List<ActivityPointsWriteDto> Points { get; set; } = new();
}
