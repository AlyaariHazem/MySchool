namespace Backend.DTOS.School.reports;

/// <summary>Monthly certificate query. Year is always the tenant active year (resolved server-side).</summary>
public class MonthlyReportQueryDTO
{
    public int TermId { get; set; }
    public int MonthId { get; set; }
    public int ClassId { get; set; }
    public int DivisionId { get; set; }
    /// <summary>0 = all students in the division.</summary>
    public int StudentId { get; set; }
}
