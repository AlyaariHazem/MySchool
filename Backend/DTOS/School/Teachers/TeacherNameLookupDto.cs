namespace Backend.DTOS.School.Teachers;

/// <summary>Minimal teacher row for lookups and paged name lists.</summary>
public sealed class TeacherNameLookupDto
{
    public int TeacherID { get; set; }
    public string FullName { get; set; } = string.Empty;
}
