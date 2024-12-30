using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
namespace Backend.Models
{

    public class TeacherSubjectStudent
    {
        public int TeacherID { get; set; }
        [JsonIgnore]
        public Teacher Teacher { get; set; }
        public int StudentID { get; set; }
        [JsonIgnore]
        public Student Student { get; set; }
        public int SubjectID { get; set; }
        [JsonIgnore]
        public Subject Subject { get; set; }
    }
}