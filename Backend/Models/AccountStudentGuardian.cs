using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models;

public class AccountStudentGuardian
{

    public int AccountStudentGuardianID { get; set; }
    public int AccountID { get; set; }
    public Accounts Accounts { get; set; }
    public int GuardianID { get; set; }
    public Guardian Guardian { get; set; }
    public int StudentID { get; set; }
    public Student Student { get; set; }
    public decimal Amount { get; set; }
    public virtual ICollection<Vouchers> Vouchers { get; set; }
}
