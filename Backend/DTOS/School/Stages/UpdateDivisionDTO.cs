using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Stages;

public class UpdateDivisionDTO
{
    public string DivisionName { get; set; } = string.Empty;
    public bool State { get; set; } = true;
}
