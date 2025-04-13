using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models;

public class YearTermMonth
{
    public int TermID { get; set; }
    public Term Term { get; set; }
    public int YearID { get; set; }
    public Year Year { get; set; }
    public int MonthID { get; set; }
    public Month Month { get; set; }
}
