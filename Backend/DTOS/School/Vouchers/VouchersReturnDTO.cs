using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Vouchers;

public class VouchersReturnDTO
{
    public int? VoucherID { get; set; }
    public string? AccountName { get; set; }
    public string? StudentName { get; set; }
    public decimal Receipt { get; set; } = decimal.Zero;
    public DateTime HireDate { get; set; } = DateTime.Now;
    public string? Note { get; set; }
    public string? PayBy { get; set; }
    public int? AccountAttachments { get; set; }
    public int? AccountStudentGuardianID { get; set; }
    public int? StudentID { get; set; }
}
