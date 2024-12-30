using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.FeeClassesDTO;


public class AddFeeClassDTO
{
    public int ClassID { get; set; }
    public int FeeID { get; set; }
    public double? Amount { get; set; }
    public bool Mandatory { get; set; } = false;
}
