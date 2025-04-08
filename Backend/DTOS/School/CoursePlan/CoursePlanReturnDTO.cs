using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.CoursePlan;

public class CoursePlanReturnDTO
{
    public string? CoursePlanName { get; set; }
    public string? DivisionName { get; set; }
    public string? TeacherName { get; set; }
    public string? TermName { get; set; }
    public string? Year { get; set; }


}
