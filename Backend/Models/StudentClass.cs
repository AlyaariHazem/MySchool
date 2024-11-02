using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Backend.Models
{
public class StudentClass
{
    public int ClassID { get; set; } // Corrected property name
    [JsonIgnore]
    public Class Class { get; set; }
    public int StudentID { get; set; }
    [JsonIgnore]
    public Student Student { get; set; }
}
}