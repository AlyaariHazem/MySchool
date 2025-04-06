using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Accounts;

public class AccountsDTO
{
    public int? AccountID { get; set; }
    public string? GuardianName { get; set; }
    public string? AccountName { get; set; }
    public bool State { get; set; } = true;
    public string? Note { get; set; }
    public decimal? OpenBalance { get; set; }
    public bool TypeOpenBalance { get; set; } = false;
    public DateTime HireDate { get; set; } = DateTime.Now;
    public int TypeAccountID { get; set; }
}
