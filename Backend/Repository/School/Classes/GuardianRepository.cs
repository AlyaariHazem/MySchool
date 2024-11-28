using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Backend.Repository.School.Implements;

namespace Backend.Repository.School.Classes;

public class GuardianRepository : IGuardianRepository
{
    private readonly DatabaseContext _context;

    public GuardianRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Guardian> AddGuardianAsync(Guardian guardian)
    {
        _context.Guardians.Add(guardian);
        await _context.SaveChangesAsync();
        return guardian;
    }
}
