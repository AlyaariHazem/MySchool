using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.DivisionsDTO;

using Backend.Models;

namespace Backend.DTOS.ClassesDTO;

public class ClassDTO
{
  public int ClassID { get; set; }
  public string ClassName { get; set; } = string.Empty;
  public string ClassYear { get; set; } = Convert.ToString(DateTime.Now);
  public int StageID { get; set; }
  public string StageName { get; set; }
  public bool State { get; set; }
  public int StudentCount { get; set; }
  // public virtual Stage Stage { get; set; } // Single Stage reference
  public virtual ICollection<DivisionINClassDTO> Divisions { get; set; } = new List<DivisionINClassDTO>();
}
