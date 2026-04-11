namespace Backend.DTOS.School.MonthlyGrade;

/// <summary>No YearID: bulk update and reads use the active academic year; create maps via mapper and YearID 0 → active year.</summary>
public class MonthlyGradeDTO
{
    public int StudentID { get; set; }
    public int SubjectID { get; set; }
    public int MonthID { get; set; }
    public int ClassID { get; set; }
    public int GradeTypeID { get; set; }
    public decimal? Grade { get; set; }
    public int TermID { get; set; }
}
