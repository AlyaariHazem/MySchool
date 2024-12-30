using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.FeeClassesDTO;

public class FeeClassDTO
{
    public int FeeClassID { get; set; }
    public int FeeID { get; set; }
    public string FeeName { get; set; } = string.Empty;
    public string? FeeNameAlis { get; set; }
    public int ClassID { get; set; }
    public string ClassName { get; set; }
    public string ClassYear { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
    public double? Amount { get; set; }
    public bool Mandatory { get; set; } = false;

}
