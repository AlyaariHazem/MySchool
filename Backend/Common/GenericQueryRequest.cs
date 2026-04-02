namespace Backend.Common;

/// <summary>
/// Unified request body for generic query endpoints (all / page).
/// Matches the BFF pattern: { filters: {}, orders: {}, pageIndex: 0, pageSize: 10 }
/// </summary>
public class GenericQueryRequest
{
    /// <summary>
    /// Dynamic column filters. Key = column name, Value = filter value (string).
    /// String columns use Contains; numeric/date columns use exact match.
    /// </summary>
    public Dictionary<string, string> Filters { get; set; } = new();

    /// <summary>
    /// Dynamic column ordering. Key = column name, Value = 1 (ASC) or -1 (DESC).
    /// </summary>
    public Dictionary<string, int> Orders { get; set; } = new();

    /// <summary>
    /// Zero-based page index for pagination (used by /page endpoint).
    /// </summary>
    public int PageIndex { get; set; } = 0;

    /// <summary>
    /// Number of items per page (used by /page endpoint).
    /// </summary>
    public int PageSize { get; set; } = 10;
}
