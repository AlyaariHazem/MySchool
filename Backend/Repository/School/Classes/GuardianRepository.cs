using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS.School.Guardians;
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

    public async Task<GuardianDTO> AddGuardianAsync(Guardian guardian)
    {
        _db.Guardians.Add(guardian);
        await _db.SaveChangesAsync();
        var guardianMapped = _mapper.Map<GuardianDTO>(guardian);
        return guardianMapped;
    }

    public async Task<List<GuardianDTO>> GetAllGuardiansAsync()
    {
        var guardians = await _db.Guardians.ToListAsync();
        var guardiansMapped = _mapper.Map<List<GuardianDTO>>(guardians);
        return guardiansMapped;
    }

    public async Task<GuardianDTO> GetGuardianByIdAsync(int guardianId)
    {
        var guardianData = await _db.Guardians.FirstOrDefaultAsync(g => g.GuardianID == guardianId);
        if (guardianData == null)
        {
            throw new Exception("Guardian not found.");
        }
        var guardianMapped = _mapper.Map<GuardianDTO>(guardianData);
        return guardianMapped;
    }
    public async Task<GuardianDTO> GetGuardianByIdForUpdateAsync(int guardianId)
    {
        var guardianData = await _db.Guardians
        .Include(g => g.ApplicationUser)
        .FirstOrDefaultAsync(g => g.GuardianID == guardianId);
        if (guardianData == null)
        {
            throw new Exception("Guardian not found.");
        }
        var guardian = new GuardianDTO
        {
            GuardianID = guardianData.GuardianID,
            FullName = guardianData.FullName,
            Gender = guardianData.ApplicationUser.Gender,
            Type = guardianData.Type,
            UserID = guardianData.UserID,
            GuardianAddress = guardianData.ApplicationUser.Address!,
            GuardianDOB = guardianData.GuardianDOB,
            GuardianEmail = guardianData.ApplicationUser.Email!,
            GuardianPhone = guardianData.ApplicationUser.PhoneNumber
        };
        return guardian;
    }

    public async Task UpdateGuardianAsync(GuardianDTO? guardian)
    {
        var guardianExist = await _db.Guardians.FirstOrDefaultAsync(g => g.GuardianID == guardian!.GuardianID);
        if (guardianExist != null)
        {
            guardianExist.FullName = guardian!.FullName;
            guardianExist.GuardianDOB = guardian.GuardianDOB;
            guardianExist.Type = guardian.Type;
            guardianExist.ApplicationUser.Address = guardian.GuardianAddress;
            guardianExist.ApplicationUser.Email = guardian.GuardianEmail;
            guardianExist.ApplicationUser.PhoneNumber = guardian.GuardianPhone;
            _db.Entry(guardian).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }

    }
}
