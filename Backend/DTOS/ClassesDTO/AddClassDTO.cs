using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.ClassesDTO;

public class AddClassDTO
{
    public int? ClassID { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int StageID { get; set; } = 1;
}
