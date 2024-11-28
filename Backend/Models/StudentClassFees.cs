using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models;

public class StudentClassFees
{
    // these ClassID and FeeID are come from FeeClass
    public int ClassID { get; set; }
    public int StudentID { get; set; }
    public int FeeID  { get; set; }
    public FeeClass FeeClass { get; set; }
    public Student Student { get; set; }
    public decimal? AmountDiscount { get; set; }
    public string? NoteDiscount { get; set; }
}
