using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.DTOS;

public class StageModel
{
            public int StageID { get; set; }
        public string StageName { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public bool Active { get; set; }=true;
        public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
        public int StudentCount { get; set; } // New property to hold the student count
}
