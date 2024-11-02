using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class Division
    {
        [Key]
        public int DivisionID { get; set; }
        public string DivisionName { get; set; } = string.Empty;
        public bool Active { get; set; }
        public int ClassID { get; set; } // Fix here
        [JsonIgnore]
        public Class Class { get; set; }
        public virtual ICollection<Student> Students { get; set; }
    }
}
