namespace Backend.Common;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Data,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages
);

