using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Years;

public class YearDTO
{
 public int YearID { get; set; }
        public DateOnly YearDateStart { get; set; }
        public DateOnly YearDateEnd { get; set; }
        public DateOnly HireDate { get; set; }
        public bool Active { get; set; }
        public int SchoolID { get; set; }   
}
