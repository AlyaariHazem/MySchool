using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.TermlyGrade;

public class TermlyGradeDTO
{
    public int? TermlyGradeID { get; set; }
    public int StudentID { get; set; }
    public decimal Grade { get; set; }= 0;
    public int TermID { get; set; }
    public int ClassID { get; set; }
    public int SubjectID { get; set; }
    public string? Note { get; set; }

}
