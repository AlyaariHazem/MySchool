using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class Class
    {
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public string ClassYear { get; set; }=DateTime.Now.ToString("yyyy-MM-dd");
        public int StageID { get; set; }
        public bool State { get; set; }
        [JsonIgnore]
        public Stage Stage { get; set; }
        [JsonIgnore]
        public virtual ICollection<Subject> Subjects { get; set; }
        [JsonIgnore]
        public virtual ICollection<Division> Divisions { get; set; }
        [JsonIgnore]
        public virtual ICollection<StudentClass> StudentClass { get; set; }
    }
}
