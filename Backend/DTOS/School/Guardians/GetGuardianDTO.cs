using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Guardians;

public class GetGuardianDTO
{
        public int GuardianID { get; set; }
        public string GuardianFullName { get; set; }
        public string? Type{ get; set; }
        public string? Gender{ get; set; }
        public string? GuardianPhone{ get; set; }
        public string GuardianEmail{ get; set; }
        public DateTime? GuardianDOB{ get; set; }
        public string GuardianAddress{ get; set; }
        public string UserID { get; set; }
}
