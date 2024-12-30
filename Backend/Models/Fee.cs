using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Backend.Models;

public class Fee
{
    public int FeeID { get; set; }     
    public string FeeName { get; set;}=string.Empty;
    public string? FeeNameAlis { get; set;}
    public string? Note{ get; set; }
    public DateTime HireDate { get; set; }=DateTime.Now;
    public bool State { get; set; } = true;
    [JsonIgnore]
    public virtual ICollection<FeeClass> FeeClasses { get; set; }//when I return the fee I want to igrnore this?

}
