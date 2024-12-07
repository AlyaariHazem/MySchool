using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.DTOS.School.Students;

public class StudentDto
{
    public Name FullName { get; set; }
    public NameAlis? FullNameAlis { get; set; }
    public string? ImageURL { get; set; }
    public string? PlaceBirth { get; set; }
    public int DivisionID { get; set; }
    public string UserID { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}
