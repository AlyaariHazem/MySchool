using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Fees;

public class ChangeStateFeeDTO
{
    public string FeeClassName { get; set; } = string.Empty;
    public bool State { get; set; }
}
