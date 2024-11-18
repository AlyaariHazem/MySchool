using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models;

public class Vouchers
{
    public int VoucherID { get; set; }
    public double Receipt { get; set; }
    public DateOnly HireDate { get; set; }
    public string Note { get; set; }
    public string PayBy { get; set; }
    public int AccountID { get; set; }
    public Accounts Accounts { get; set; }
}
