using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Backend.Models;

public class Accounts
{
    public int AccountID { get; set; }
    public string Type { get; set; }
    public bool State { get; set; }
    public string? Note { get; set; }
    public double OpenBalance { get; set; }
    public bool TypeOpenBalance { get; set; }=false;
    public DateOnly HireDate { get; set; }= DateOnly.FromDateTime(DateTime.Now);
    public int GuardianID { get; set; }
    public Guardian Guardian { get; set; }

}
