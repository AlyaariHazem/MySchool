using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.GradeTypes;

namespace Backend.DTOS.School.MonthlyGrade;

public class MonthlyGradesReternDTO
{
    public int StudentID { get; set; }
    public string StudentName { get; set; }
    public int SubjectID { get; set; }
    public string SubjectName { get; set; }
    public List<GradeTypeMonthDTO> Grades { get; set; }
}