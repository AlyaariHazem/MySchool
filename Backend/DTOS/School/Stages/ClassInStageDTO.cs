using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Stages;

public class ClassInStageDTO
{
    public int ClassID { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
}
