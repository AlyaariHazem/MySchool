using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;

using Backend.Models;
using Backend.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class GuardianRepository : Repository<Guardian>, IGuardianRepository
{
    private readonly DatabaseContext _db;


    public GuardianRepository(DatabaseContext db) : base(db)
    {
        _db = db;

    }


    public async Task UpdateAsync(Guardian? guardian)
    {
        var guardianExist = await _db.Guardians.FirstOrDefaultAsync(g => g.GuardianID == guardian.GuardianID);
        if (guardianExist != null)
        {
            guardianExist.FullName = guardian.FullName;
            guardianExist.GuardianDOB = guardian.GuardianDOB;
            guardianExist.Type = guardian.Type;
            guardianExist.ApplicationUser.Address = guardian.ApplicationUser.Address;
            guardianExist.ApplicationUser.Email = guardian.ApplicationUser.Email;
            guardianExist.ApplicationUser.PhoneNumber = guardian.ApplicationUser.PhoneNumber;
            _db.Update(guardianExist);
            await SaveAsync();
        }


    }
}
