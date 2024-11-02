using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS;

public class StagesDTO
{
    public int? ID { get; set;}
        public string? StageName { get; set; }
        public string? Note { get; set; }="لا يوجد";
        public bool Active { get; set; }=true;
        public DateTime HireDate { get; set; }= DateTime.Now;
        public int YearID { get; set; }=1;
}
