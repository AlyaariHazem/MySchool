using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Subjects;

public class SubjectsDTO
{
    public int? SubjectID { get; set; }
    public string? SubjectName { get; set; }
    public int? ClassID { get; set; }
}
