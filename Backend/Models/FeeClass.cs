using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models;

public class FeeClass
{
    public int ClassID { get; set; }
    public Class Class { get; set; }
    public int FeeID { get; set; }
    public Fee Fee { get; set; }
    public double? Amount { get; set; }

}