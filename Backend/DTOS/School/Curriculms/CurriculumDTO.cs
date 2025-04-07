using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Curriculms;

public class CurriculumDTO
{
    public int? SubjectID { get; set; }
    public string CurriculumName { get; set; }
    public int ClassID { get; set; }
    public string? Not { get; set; }
    public DateTime HireDate { get; set; }= DateTime.Now;
}
