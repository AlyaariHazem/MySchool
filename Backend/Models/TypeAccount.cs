using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models;

public class TypeAccount
{
    public int TypeAccountID { get; set; }
    public string TypeAccountName { get; set;}
    public virtual ICollection<Accounts> Accounts { get; set; }
}
