using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models
{
public class Name
{
    public string FirstName { get; set; }
    public string? MiddleName { get; set; } 
    public string LastName { get; set; }
    
}
public class NameAlis
{
    public string? FirstNameEng { get; set; }
    public string? MiddleNameEng { get; set; } 
    public string? LastNameEng { get; set; }
    
}
}