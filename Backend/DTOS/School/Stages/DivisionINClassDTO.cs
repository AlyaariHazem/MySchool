using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Stages;

public class DivisionINClassDTO
{
    public int DivisionID { get; set; }
    public string DivisionName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
}
