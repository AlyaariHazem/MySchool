using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.ClassesDTO;

public class UpdateClassDTO
{
    public string ClassName { get; set; } = string.Empty;
    public bool State { get; set; } = true;
}
