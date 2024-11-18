using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models;

public class Fee
{
    public int FeeID { get; set; }     
    public string FeeName { get; set;}=string.Empty;
    public DateTime HireDate { get; set; }=DateTime.Now;
    public bool State { get; set; } = true;
    public string? discount { get; set;}
    public string? NoteDiscount { get; set;}
    public virtual ICollection<FeeClass> FeeClasses { get; set; }

}
