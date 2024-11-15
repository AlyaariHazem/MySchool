using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS;

public class DivisionDTO
{
        public int DivisionID { get; set; }
        public string DivisionName { get; set; } = string.Empty;
        public bool State { get; set; } = true;
        public int ClassID { get; set; } = 1;
        public string? ClassesName { get; set; } // Change type to string to hold class name
        public int? StudentCount { get; set; } // New property to hold the student count
}
