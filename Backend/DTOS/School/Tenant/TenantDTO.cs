using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Tenant;

public class TenantDTO
{
    public int? TenantID { get; set; }
    public string SchoolName { get; set; }
    public string ConnectionString { get; set; }
}
