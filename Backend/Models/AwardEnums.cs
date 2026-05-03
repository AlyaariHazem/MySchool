namespace Backend.Models;

/// <summary>مدة الدورة: نجم الأسبوع، الشهر، الفصل، أو العام الدراسي.</summary>
public enum AwardCycleKind
{
    Week = 1,
    Month = 2,
    Term = 3,
    Year = 4
}

public enum AwardCycleStatus
{
    Draft = 0,
    Open = 1,
    NominationsClosed = 2,
    Completed = 3
}

public enum AwardNominationStatus
{
    Pending = 0,
    Shortlisted = 1,
    Rejected = 2,
    Withdrawn = 3
}
