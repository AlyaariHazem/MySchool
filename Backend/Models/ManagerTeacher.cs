using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class ManagerTeacher
    {
        public int TeacherID { get; set; }
        [JsonIgnore]
        public Teacher Teacher { get; set; }
        public int ManagerID { get; set; }
        [JsonIgnore]
        public Manager Manager { get; set; }
    }
}
