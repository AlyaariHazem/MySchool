namespace Backend.DTOS.School.CentralPoints;

public class PointsSourceDto
{
    public int PointsSourceID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class PointsRuleDto
{
    public int PointsRuleID { get; set; }
    public int? SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public int PointsSourceID { get; set; }
    public string PointsSourceCode { get; set; } = string.Empty;
    public string RuleKey { get; set; } = "*";
    public int DeltaPoints { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public DateTime? EffectiveFromUtc { get; set; }
    public DateTime? EffectiveToUtc { get; set; }
}

public class PointsRuleWriteDto
{
    public int? SchoolID { get; set; }
    public int PointsSourceID { get; set; }
    public string RuleKey { get; set; } = "*";
    public int DeltaPoints { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? EffectiveFromUtc { get; set; }
    public DateTime? EffectiveToUtc { get; set; }
}

public class PointsRuleFilterDto
{
    public int? SchoolID { get; set; }
    public int? PointsSourceID { get; set; }
    public bool? ActiveOnly { get; set; } = true;
}

public class PointsLedgerFilterDto
{
    public int? SchoolID { get; set; }
    public int? EmployeeProfileID { get; set; }
    public int? PointsSourceID { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; } = 50;
}

public class PointsLedgerListItemDto
{
    public int PointsLedgerID { get; set; }
    public int PointsTransactionID { get; set; }
    public int EmployeeProfileID { get; set; }
    public string? EmployeeDisplayName { get; set; }
    public int SchoolID { get; set; }
    public int AcademicYearID { get; set; }
    public string PointsSourceCode { get; set; } = string.Empty;
    public int? PointsRuleID { get; set; }
    public int DeltaPoints { get; set; }
    public string? Memo { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? CorrelationEntityType { get; set; }
    public int? CorrelationEntityID { get; set; }
    public string? IdempotencyKey { get; set; }
}

public class PointsBalanceDto
{
    public int EmployeeProfileID { get; set; }
    public int SchoolID { get; set; }
    public int AcademicYearID { get; set; }
    public int TotalPoints { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

/// <summary>Post points from any module; resolves rule unless override is set.</summary>
public class PostCentralPointsDto
{
    public int EmployeeProfileID { get; set; }
    public int SchoolID { get; set; }

    /// <summary>Must match <see cref="PointsSource.Code"/>.</summary>
    public string PointsSourceCode { get; set; } = string.Empty;

    /// <summary>Rule match key; default "*".</summary>
    public string RuleKey { get; set; } = "*";

    /// <summary>When set, skips rule resolution and uses this delta.</summary>
    public int? OverrideDeltaPoints { get; set; }

    public string? CorrelationEntityType { get; set; }
    public int? CorrelationEntityID { get; set; }

    public string? IdempotencyKey { get; set; }
    public string? Notes { get; set; }
    public string? Memo { get; set; }
}

public class PostCentralPointsResultDto
{
    public int PointsTransactionID { get; set; }
    public int PointsLedgerID { get; set; }
    public int AppliedDeltaPoints { get; set; }
    public int? MatchedPointsRuleID { get; set; }
    public int NewBalanceTotal { get; set; }
    public bool WasIdempotentReplay { get; set; }
}
