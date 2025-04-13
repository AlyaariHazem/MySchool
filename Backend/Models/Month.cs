using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models;

public class Month
{
     public int MonthID { get; set; }

    // e.g. "October", "November", "July", "August", etc.
    public string Name { get; set; }
    public ICollection<YearTermMonth> YearTermMonths { get; set; }
    public ICollection<MonthlyGrade> MonthlyGrades { get; set; }
    
}
