using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.GradeTypes;

public record GradeTypeMonthDTO
{
    public string Name { get; set; }
    public int?MaxGrade { get; set; } = 0;
}
