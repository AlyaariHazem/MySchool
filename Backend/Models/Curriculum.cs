using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models;

public class Curriculum
{
    public int SubjectID { get; set; }
    public string CurriculumName { get; set; }
    public Subject Subject { get; set; }
    public int ClassID { get; set; }
    public Class Class { get; set; }
    public string? Note { get; set; }
    public DateTime HireDate { get; set; }= DateTime.Now;
}
