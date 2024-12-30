using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.StagesDTO;

public class UpdateStageDTO
{
    public int? ID { get; set; }
    public string StageName { get; set; }
    public string? Note { get; set; } = "لا يوجد";
    public bool Active { get; set; } = true;
}
