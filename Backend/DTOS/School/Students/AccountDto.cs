using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Students;

public class AccountDto
{
    public bool State { get; set; }
    public string? Note { get; set; }
    public decimal? OpenBalance { get; set; }
    public bool TypeOpenBalance { get; set; }
    public DateTime HireDate { get; set; }
    public int TypeAccountID { get; set; }
}
