using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School.Classes;

public class GuardianRepository : IGuardianRepository
{
    private readonly DatabaseContext _db;
    private readonly IMapper _mapper;

    public GuardianRepository(DatabaseContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<Guardian> AddGuardianAsync(Guardian guardian)
    {
        _db.Guardians.Add(guardian);
        await _db.SaveChangesAsync();
        return guardian;
    }

    public async Task<List<Guardian>> GetAllGuardiansAsync()
    {
        var guardians=await _db.Guardians.ToListAsync();
        return guardians;
    }

    public async Task<Guardian> GetGuardianByIdAsync(int guardianId)
    {
        var guardianData=await _db.Guardians.FirstOrDefaultAsync(g=>g.GuardianID==guardianId);
        if (guardianData == null)
        {
            return null;
        }
        return guardianData;
    }

     public async Task UpdateGuardianAsync(Guardian? guardian)
    {
        var guardianExist = await _db.Guardians.FirstOrDefaultAsync(g => g.GuardianID == guardian.GuardianID);
        if(guardianExist!=null){
            guardianExist.FullName = guardian.FullName;
            guardianExist.GuardianDOB = guardian.GuardianDOB;
            guardianExist.Type = guardian.Type;
            guardianExist.ApplicationUser.Address = guardian.ApplicationUser.Address;
            guardianExist.ApplicationUser.Email = guardian.ApplicationUser.Email;
            guardianExist.ApplicationUser.PhoneNumber = guardian.ApplicationUser.PhoneNumber;
            _db.Entry(guardian).State = EntityState.Modified;
           await _db.SaveChangesAsync();
        }


    }
}
