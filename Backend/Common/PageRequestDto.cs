namespace Backend.Common;

/// <summary>
/// Shared paging body for POST list/page endpoints.
/// <see cref="PageIndex"/> is zero-based (first page = 0).
/// </summary>
public class PageRequestDto
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
}
