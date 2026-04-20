using Backend.Common;

namespace Backend.DTOS.School.Teachers;

/// <summary>POST <c>Teacher/names/page</c> — paged id + display name in the active teacher year scope.</summary>
public class TeacherNamesPageRequestDto : PageRequestDto
{
    public string? Search { get; set; }
}
