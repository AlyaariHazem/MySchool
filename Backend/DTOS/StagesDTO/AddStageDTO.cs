using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.DTOS.StagesDTO;

public class AddStageDTO
{
    public string StageName { get; set; }
    public string? Note { get; set; } = "لا يوجد";
    public bool Active { get; set; } = true;
    public int YearID { get; set; }
}
