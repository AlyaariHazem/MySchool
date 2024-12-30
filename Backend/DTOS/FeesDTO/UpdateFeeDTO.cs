using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.FeesDTO;

public class UpdateFeeDTO
{
    public int? FeeID { get; set; }
    public string FeeName { get; set; } = string.Empty;
    public string? FeeNameAlis { get; set; }
    public string? Note { get; set; }
}
