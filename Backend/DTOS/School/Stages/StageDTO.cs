using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Stages;
using Backend.Models;

namespace Backend.DTOS;

public class StageDTO
{
        public int StageID { get; set; }
        public string StageName { get; set; } = string.Empty;
        public string? Note { get; set; } = "لا يوجد";
        public bool Active { get; set; }=true;
        public virtual ICollection<ClassInStageDTO> Classes { get; set; } = new List<ClassInStageDTO>();//when I debgging this to to Class not ClassDTO
        public int StudentCount { get; set; } // New property to hold the student count
}
