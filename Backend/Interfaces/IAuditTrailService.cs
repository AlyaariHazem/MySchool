namespace Backend.Interfaces;

/// <summary>Records who did what and when: structured logs plus durable rows in the tenant (or admin) database when available.</summary>
public interface IAuditTrailService
{
    /// <param name="category">e.g. Grades, Fees, Database</param>
    /// <param name="action">Stable action key, e.g. MonthlyGrade.BulkUpdate</param>
    /// <param name="details">Anonymous object or dictionary serialized to JSON for the audit row</param>
    Task RecordAsync(string category, string action, object? details = null, CancellationToken cancellationToken = default);
}
