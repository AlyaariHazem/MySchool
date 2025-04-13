using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models;

public class CoursePlan
{
    // e.g. 2021-2022
    public int YearID { get; set; }
    public Year Year { get; set; }

    // e.g. term #1
    public int TermID { get; set; }
    public Term Term { get; set; }

    // e.g. القرآن الكريم
    public int SubjectID { get; set; }
    public Subject Subject { get; set; }

    // e.g. Teacher Hazem Alyaari
    public int TeacherID { get; set; }
    public Teacher Teacher { get; set; }

    // e.g. شعبة / Grade-level or Class
    public int ClassID { get; set; }
    public Class Class { get; set; }
    public int DivisionID { get; set; }
    public Division Division { get; set; }
}
