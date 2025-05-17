using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models;

public class RefreshToken
{
    public Guid Id            { get; set; }
    public string TokenHash   { get; set; } = default!;
    public string UserId      { get; set; } = default!;
    public DateTime Expires   { get; set; }
    public DateTime? Revoked  { get; set; }
    public ApplicationUser User { get; set; } = default!;
}