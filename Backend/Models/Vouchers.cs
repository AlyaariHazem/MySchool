using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models;

public class Vouchers
{
    public int VoucherID { get; set; }
    public decimal Receipt { get; set; }
    public DateTime HireDate { get; set; } = DateTime.Now;
    public string Note { get; set; }
    public string PayBy { get; set; }

    // Account (Guardian-level association)
    public int AccountID { get; set; }
    public Accounts Accounts { get; set; }
    public virtual ICollection<Attachments> Attachments { get; set; }
}
