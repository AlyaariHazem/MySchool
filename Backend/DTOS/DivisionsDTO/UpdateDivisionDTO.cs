using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.DivisionsDTO;

public class UpdateDivisionDTO
{
    public int DivisionID { get; set; }
    public string DivisionName { get; set; } = string.Empty;
    public bool State { get; set; } = true;
}
