namespace Backend.DTOS.School.Students;

public class StudentNameIdSearchRequestDTO
{
    public int? StudentID { get; set; }
    public string? FullName { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 8;
}
