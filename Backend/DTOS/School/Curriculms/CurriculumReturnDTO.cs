using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Curriculms;

public class CurriculumReturnDTO
{
     public string? SubjectName { get; set; }
    public string CurriculumName { get; set; }
    public string ClassName { get; set; }
    public string? Not { get; set; }
    public DateTime HireDate { get; set; }= DateTime.Now;
}
