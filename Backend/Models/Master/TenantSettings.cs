namespace Backend.Models.Master;

/// <summary>Optional key/value settings for a tenant (locale, feature flags, branding ids).</summary>
public class TenantSettings
{
    public int TenantSettingsId { get; set; }

    public int TenantId { get; set; }

    public string Key { get; set; } = default!;

    public string? Value { get; set; }

    public global::Backend.Models.Tenant Tenant { get; set; } = default!;
}
