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

    // Link to Term, so EF can easily see which Term the month belongs to
    public int TermID { get; set; }
    public Term Term { get; set; }

    // Navigation to MonthlyGrade if you want two-way:
    public ICollection<MonthlyGrade> MonthlyGrades { get; set; }
    
}
