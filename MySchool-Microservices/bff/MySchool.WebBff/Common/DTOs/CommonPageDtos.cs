namespace MySchool.WebBff.Common.DTOs;

public sealed class CommonGetPageRequestDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
}

public sealed class CommonPageResponseDto<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
}

/// <summary>Placeholder for resources that do not yet expose create/update DTOs on the BFF.</summary>
public sealed class EmptyRequestDto;
