using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Years;

public class YearDTO
{
        public int? YearID { get; set; }
        public DateTime YearDateStart { get; set; }
        public DateTime YearDateEnd { get; set; }
        public DateTime HireDate { get; set; }
        public bool Active { get; set; }=true;
        public int SchoolID { get; set; }   
}
