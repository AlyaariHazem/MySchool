using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Vouchers;

public class VouchersDTO
{
    public int? VoucherID { get; set; }
    public decimal Receipt { get; set; } = decimal.Zero;
    public DateTime HireDate { get; set; } = DateTime.Now;
    public string? Note { get; set; }
    public string? PayBy { get; set; }

    // Account (Guardian-level association)
    public int AccountStudentGuardianID { get; set; }
    public List<string>? Attachments { get; set; }
    public int? StudentID { get; set; }
    public List<IFormFile>? Files { get; set; }

}
