using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Months;

public class MonthDTO
{
    public int? MonthID { get; set; }
    public string? Name { get; set; }
    public int? TermID { get; set; }
}
