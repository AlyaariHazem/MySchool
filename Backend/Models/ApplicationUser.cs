using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Backend.Models;

public class ApplicationUser : IdentityUser
{
    public string? Address { get; set; }
    public string? Gender { get; set; } = string.Empty;
    public DateTime HireDate { get; set; } = DateTime.Now;

    // Relationships with role-specific entities
    public virtual Teacher? Teacher { get; set; }
    public virtual Student? Student { get; set; }
    public virtual Guardian? Guardian { get; set; }
    public virtual Manager? Manager { get; set; }

    // UserType property to specify the role
    public string UserType { get; set; } // "Teacher", "Student", "Guardian", "Manager"
    public ICollection<RefreshToken> RefreshTokens { get; set; }
}