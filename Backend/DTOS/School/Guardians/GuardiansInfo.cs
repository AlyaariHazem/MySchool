using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Guardians;

public class GuardiansInfo
{
    public int GuardianID { get; set; }
    public string FullName { get; set; }
    public int? StudentCount { get; set; }
    public decimal? RequiredFee { get; set; }
    public decimal? Piad { get; set; }
    public decimal? Remaining { get; set; }
    public string? Gender { get; set; }
    public string? Phone { get; set; }
    public DateTime? DOB { get; set; }
    public string? Address { get; set; }
    public int? AccountId { get; set; }
}
