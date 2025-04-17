using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.GradeTypes;

public record GradeTypeMonthDTO
{
    public int GradeTypeID { get; set; }
    public decimal?MaxGrade { get; set; } = 0;
}
