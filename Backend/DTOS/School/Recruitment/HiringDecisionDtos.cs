using System.ComponentModel.DataAnnotations;
using Backend.Models;

namespace Backend.DTOS.School.Recruitment;

public class HiringDecisionCreateDto
{
    public HiringDecisionStatus DecisionStatus { get; set; } = HiringDecisionStatus.Pending;

    public DateTime? DecisionDate { get; set; }

    [MaxLength(450)]
    public string? DecidedByUserID { get; set; }

    public int? DecidedByEmployeeProfileID { get; set; }

    [Required]
    public int OfferJobTypeID { get; set; }

    public DateTime? ProposedHireDate { get; set; }

    [MaxLength(2000)]
    public string? ProposedSalaryNotes { get; set; }

    [MaxLength(4000)]
    public string? Reason { get; set; }

    [MaxLength(4000)]
    public string? InternalNotes { get; set; }

    /// <summary>When false (default), at least one candidate evaluation must exist unless the workflow is still pending.</summary>
    public bool SkipEvaluationCheck { get; set; }
}

public class HiringDecisionUpdateDto
{
    public HiringDecisionStatus? DecisionStatus { get; set; }

    public DateTime? DecisionDate { get; set; }

    [MaxLength(450)]
    public string? DecidedByUserID { get; set; }

    public int? DecidedByEmployeeProfileID { get; set; }

    public int? OfferJobTypeID { get; set; }

    public DateTime? ProposedHireDate { get; set; }

    [MaxLength(2000)]
    public string? ProposedSalaryNotes { get; set; }

    [MaxLength(4000)]
    public string? Reason { get; set; }

    [MaxLength(4000)]
    public string? InternalNotes { get; set; }
}

public class HiringDecisionReadDto
{
    public int HiringDecisionID { get; set; }
    public int JobApplicationID { get; set; }
    public int SchoolID { get; set; }
    public int AcademicYearID { get; set; }
    public HiringDecisionStatus DecisionStatus { get; set; }
    public DateTime DecisionDate { get; set; }
    public string? DecidedByUserID { get; set; }
    public int? DecidedByEmployeeProfileID { get; set; }
    public int OfferJobTypeID { get; set; }
    public string? OfferJobTypeName { get; set; }
    public DateTime? ProposedHireDate { get; set; }
    public string? ProposedSalaryNotes { get; set; }
    public string? Reason { get; set; }
    public string? InternalNotes { get; set; }
    public int? ConvertedEmployeeProfileID { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
