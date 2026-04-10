namespace Backend.DTOS.School.TermlyGrade;

/// <summary>Filter and pagination for termly grades list (POST body).</summary>
public class TermlyGradeQueryDTO
{
    public int TermId { get; set; }
    public int YearId { get; set; }
    public int ClassId { get; set; }
    public int SubjectId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
