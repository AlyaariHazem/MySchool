namespace Backend.DTOS.School.Employee;

/// <summary>Explicit archive (deactivate) for a school year — used by POST <c>api/Employee/{id}/archive</c>.</summary>
public class ArchiveEmployeeRequestDTO
{
    /// <summary>Teacher or Manager (must match stored employee).</summary>
    public string JobType { get; set; } = "";

    /// <summary>Omitted = active academic year (or latest year).</summary>
    public int? YearId { get; set; }

    public DateTime? ExitDate { get; set; }
    public string? ExitReason { get; set; }
    public string? Notes { get; set; }
}
