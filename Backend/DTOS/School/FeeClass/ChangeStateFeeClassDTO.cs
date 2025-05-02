using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.FeeClass;

public class ChangeStateFeeClassDTO
{
    public string FeeClassName { get; set; } = string.Empty;
     public bool Mandatory { get; set; }
}
