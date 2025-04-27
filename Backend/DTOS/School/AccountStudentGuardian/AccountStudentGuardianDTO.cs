using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.AccountStudentGuardian;

public class AccountStudentGuardianDTO
{
    public int? AccountStudentGuardianID { get; set; }
    public int AccountID { get; set; }
    public int? GuardianID { get; set; }
    public int StudentID { get; set; }
    public decimal? Amount { get; set; }
}
