using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.GradeTypes;

public record GradeTypesDTO
{
    public int? GradeTypeID { get; set; }
    public string Name { get; set; }
    public decimal? MaxGrade { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}
