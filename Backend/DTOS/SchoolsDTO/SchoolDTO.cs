using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.SchoolsDTO;

public class SchoolDTO
{
    public int? SchoolID { get; set; }
    public string SchoolName { get; set; }
    public string SchoolNameEn { get; set; }
    public DateTime HireDate { get; set; }
    public string? SchoolVison { get; set; }
    public string? SchoolMission { get; set; }
    public string SchoolGoal { get; set; }
    public string? Notes { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public int SchoolPhone { get; set; }
    public string Street { get; set; }
    public string SchoolType { get; set; }
    public string? Email { get; set; }
    public int? fax { get; set; }
    public string? zone { get; set; }
}