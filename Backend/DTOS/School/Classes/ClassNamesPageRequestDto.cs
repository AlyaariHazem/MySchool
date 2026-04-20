using Backend.Common;

namespace Backend.DTOS.School.Classes;

/// <summary>POST <c>Classes/GetAllNameClasses/page</c> — paging plus optional class-name filter.</summary>
public class ClassNamesPageRequestDto : PageRequestDto
{
    public string? Search { get; set; }
}
