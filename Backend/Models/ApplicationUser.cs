using System;
using System.Collections.Generic;
using Backend.Models.Master;
using Microsoft.AspNetCore.Identity;

namespace Backend.Models;

public class ApplicationUser : IdentityUser
{
    public string? Address { get; set; }
    public string? Gender { get; set; } = string.Empty;
    public DateTime HireDate { get; set; } = DateTime.Now;

    /// <summary>Legacy coarse category; prefer <see cref="UserTenants"/> for per-school access.</summary>
    public string UserType { get; set; } = string.Empty;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    /// <summary>Central mapping: which schools this login may access and with which tenant role.</summary>
    public ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
}