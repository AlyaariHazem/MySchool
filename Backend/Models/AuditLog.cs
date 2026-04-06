namespace Backend.Models;

/// <summary>Immutable record of a sensitive domain action for compliance and operations (tenant database).</summary>
public class AuditLog
{
    public long AuditLogId { get; set; }

    /// <summary>Logical area, e.g. Grades, Fees, Database.</summary>
    public string Category { get; set; } = "";

    /// <summary>Specific operation, e.g. MonthlyGrade.BulkUpdate.</summary>
    public string Action { get; set; } = "";

    public string? ActorUserId { get; set; }

    public string? ActorDisplayName { get; set; }

    /// <summary>JSON payload with entity ids and before/after snapshots (keep small; no PII beyond what the app already stores).</summary>
    public string? DetailsJson { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public int? TenantId { get; set; }

    public string? CorrelationId { get; set; }
}
