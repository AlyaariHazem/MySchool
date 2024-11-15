using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.DTOS;

public class StagesDTO
{
    public string StageName { get; set; }
    public string? Note { get; set; } = "لا يوجد";
    public bool Active { get; set; } = true;
}
