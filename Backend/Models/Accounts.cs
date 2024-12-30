using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Backend.Models;

public class Accounts
{
    public int AccountID { get; set; }
    public bool State { get; set; } = true;
    public string? Note { get; set; }
    public decimal? OpenBalance { get; set; }
    public bool TypeOpenBalance { get; set; }=false;
    public DateTime HireDate { get; set; }=DateTime.Now;
    public int TypeAccountID { get; set; }
    public TypeAccount TypeAccount { get; set; }
    public virtual ICollection<AccountStudentGuardian> AccountStudentGuardians { get; set; }

}
