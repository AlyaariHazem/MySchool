using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Classes;

public class AllClassesDTO
{
    public int? ClassID { get; set; }
    public string ClassName { get; set; } = string.Empty;
}
