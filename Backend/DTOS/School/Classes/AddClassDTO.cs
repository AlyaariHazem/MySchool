using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Stages;

public class AddClassDTO
{
    public int? ClassID { get; set;}
    public string ClassName { get; set; } = string.Empty;
    public int StageID { get; set; }=1;
    public int YearID { get; set; }
}
