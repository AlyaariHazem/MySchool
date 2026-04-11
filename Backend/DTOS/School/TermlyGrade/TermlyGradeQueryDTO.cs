namespace Backend.DTOS.School.TermlyGrade;

/// <summary>Filter and pagination for termly grades list (POST body). Year is always the active year on the server.</summary>
public class TermlyGradeQueryDTO
{
    public int TermId { get; set; }
    public int ClassId { get; set; }
    public int SubjectId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
