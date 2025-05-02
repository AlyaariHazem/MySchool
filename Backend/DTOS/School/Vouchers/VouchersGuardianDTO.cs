using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Vouchers;

public class VouchersGuardianDTO
{
    public int GuardianID { get; set; }
    public string? ClassName { get; set; }
    public List<decimal>? RequiredFee { get; set; }
    public List<string>? ImageURL { get; set; }
    public string? StudentName { get; set; }
    public decimal? receiptionFee { get; set; }
}
