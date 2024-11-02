using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class Stage
    {
        public int StageID { get; set; }
        public string StageName { get; set; }
        public string? Note { get; set; }
        public bool Active { get; set; }
        public DateTime HireDate { get; set; }
        public int YearID { get; set; }
        [JsonIgnore]
        public Year Year { get; set; }
        public virtual ICollection<Class> Classes { get; set; }

    }
}
