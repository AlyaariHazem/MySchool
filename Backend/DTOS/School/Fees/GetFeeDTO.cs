using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Fees;

public class GetFeeDTO
{
    public int FeeID { get; set; }     
    public string FeeName { get; set;}=string.Empty;
    public string? FeeNameAlis { get; set;}
    public string? Note{ get; set; }
    public DateTime HireDate { get; set; }=DateTime.Now;
    public bool State { get; set; } = true;
}
