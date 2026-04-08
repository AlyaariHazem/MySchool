using System;

namespace Backend.Models.Master;

/// <summary>Optional SaaS subscription row per tenant.</summary>
public class Subscription
{
    public int SubscriptionId { get; set; }

    public int TenantId { get; set; }

    public string PlanCode { get; set; } = "standard";

    public DateTime StartUtc { get; set; } = DateTime.UtcNow;

    public DateTime? EndUtc { get; set; }

    /// <summary>e.g. Active, Trial, PastDue, Cancelled.</summary>
    public string Status { get; set; } = "Active";

    public int? MaxStudents { get; set; }

    public global::Backend.Models.Tenant Tenant { get; set; } = default!;
}
