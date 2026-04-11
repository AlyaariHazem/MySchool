namespace Backend.DTOS.School.Students;

/// <summary>Minimal student row for monthly report filters (guardian: own children only).</summary>
public class GuardianChildReportOptionDTO
{
    public int StudentID { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int ClassID { get; set; }
    public int DivisionID { get; set; }
}
