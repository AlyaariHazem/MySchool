using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models;

public class Tenant
{
    public int TenantId { get; set; }
    public string SchoolName { get; set; }
    public string ConnectionString { get; set; }
    public Manager Manager { get; set; }

    // Possibly more fields like Domain, Email, etc.
}
