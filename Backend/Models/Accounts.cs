using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Backend.Models;

public class Accounts
{
    public int AccountID { get; set; }
    public decimal? Amount { get; set; } = 0;
    public bool State { get; set; } = true;
    public string? Note { get; set; }
    public decimal? OpenBalance { get; set; }
    public bool TypeOpenBalance { get; set; }=false;
    public DateTime HireDate { get; set; }=DateTime.Now;
    public int TypeAccountID { get; set; }
    public TypeAccount TypeAccount { get; set; }
    public int GuardianID { get; set; }
    public Guardian Guardian { get; set; }
    public int StudentID { get; set; }
    public Student Student { get; set; }
    public virtual ICollection<Vouchers> Vouchers { get; set; }

}
