using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Accounts;

public class StudentAndAccountNames
{
    public int StudentID { get; set; }
    public string? StudentName { get; set; }
    public string? AccountName { get; set; }
    public int? GuardianID { get; set; }
    public int? AccountStudentGuardianID { get; set; }
}
