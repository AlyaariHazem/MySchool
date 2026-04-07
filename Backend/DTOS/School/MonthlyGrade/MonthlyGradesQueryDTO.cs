namespace Backend.DTOS.School.MonthlyGrade;

public class MonthlyGradesQueryDTO
{
    public int TermId { get; set; }
    public int MonthId { get; set; }
    public int ClassId { get; set; }
    public int SubjectId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
