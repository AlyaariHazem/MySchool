using System.Collections.Generic;
using Backend.Models.Master;

namespace Backend.Models;

public class Tenant
{
    public int TenantId { get; set; }
    public string SchoolName { get; set; } = default!;
    public string ConnectionString { get; set; } = default!;

    public ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<TenantSettings> TenantSettings { get; set; } = new List<TenantSettings>();
}
