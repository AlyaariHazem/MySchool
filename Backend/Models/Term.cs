using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models;

public class Term
{
    public int TermID { get; set; }
    public string Name { get; set; } // مثل: الفصل الأول، الفصل الثاني
    public int YearID { get; set; }
    public Year Year { get; set; }
    public ICollection<MonthlyGrade> MonthlyGrades { get; set; }
    public ICollection<TermlyGrade> TermlyGrades { get; set; }
    public ICollection<CoursePlan> CoursePlans { get; set; }
    public ICollection<Month> Months { get; set; } 

}
