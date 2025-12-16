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
        var guardiansData = await _db.Guardians
        .Include(g => g.ApplicationUser).ToListAsync();

        if (guardiansData == null)
            throw new Exception("Guardian not found.");

        var mappedGuardians = guardiansData.Select(guardian => new GuardianDTO
        {
            GuardianID = guardian.GuardianID,
            FullName = guardian.FullName,
            Gender = guardian.ApplicationUser.Gender,
            Type = guardian.Type,
            UserID = guardian.UserID,
            GuardianAddress = guardian.ApplicationUser.Address!,
            GuardianDOB = guardian.GuardianDOB,
            GuardianEmail = guardian.ApplicationUser.Email!,
            GuardianPhone = guardian.ApplicationUser.PhoneNumber
        }).ToList();

        return mappedGuardians;
    }

    public async Task<GuardianDTO> GetGuardianByIdAsync(int guardianId)
    {
        var guardianData = await _db.Guardians
         .Include(g => g.ApplicationUser)
         .FirstOrDefaultAsync(g => g.GuardianID == guardianId);

        if (guardianData == null)
            return null!;

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

    public async Task<GuardianDTO> GetGuardianByIdForUpdateAsync(int guardianId)
    {
        var guardianData = await _db.Guardians
        .Include(g => g.ApplicationUser)
        .FirstOrDefaultAsync(g => g.GuardianID == guardianId);
        if (guardianData == null)
            throw new Exception("Guardian not found.");

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
    public async Task<List<GuardiansInfo>> GetAllGuardiansInfoAsync()
    {
        // Use AsNoTracking to prevent duplicate entity tracking issues
        var guardiansData = await _db.Guardians
        .AsNoTracking()
        .Include(g => g.ApplicationUser)
        .Include(g => g.AccountStudentGuardians)
            .ThenInclude(ASG => ASG.Vouchers)
        .Include(g => g.AccountStudentGuardians)
            .ThenInclude(ASG => ASG.Accounts)
        .AsSplitQuery()
        .ToListAsync();

        if (guardiansData == null)
            throw new Exception("Guardian not found.");

        // Group by GuardianID to ensure we only get one record per guardian
        // Filter out guardians with no students (StudentCount = 0)
        var mappedGuardians = guardiansData
            .GroupBy(g => g.GuardianID)
            .Select(group => 
            {
                var guardian = group.First();
                var studentCount = guardian.AccountStudentGuardians?.Count() ?? 0;
                
                // Skip guardians with no students
                if (studentCount == 0)
                    return null;

                var requiredFee = guardian.AccountStudentGuardians?.Sum(ASG => ASG.Amount) ?? 0;
                var piad = guardian.AccountStudentGuardians?.Sum(ASG => ASG.Vouchers?.Sum(V => V.Receipt) ?? 0) ?? 0;

                return new GuardiansInfo
                {
                    GuardianID = guardian.GuardianID,
                    FullName = guardian.FullName,
                    Gender = guardian.ApplicationUser?.Gender ?? "Unknown",
                    StudentCount = studentCount,
                    RequiredFee = requiredFee,
                    Piad = piad,
                    Remaining = requiredFee - piad,
                    Address = guardian.ApplicationUser?.Address ?? "N/A",
                    DOB = guardian.GuardianDOB,
                    Phone = guardian.ApplicationUser?.PhoneNumber ?? "N/A",
                    AccountId = guardian.AccountStudentGuardians?.FirstOrDefault()?.Accounts?.AccountID ?? 1
                };
            })
            .Where(g => g != null)
            .Cast<GuardiansInfo>()
            .ToList();

        return mappedGuardians;
    }

    public async Task<Guardian> GetGuardianByGuardianIdAsync(int guardianId)
    {
        return await _db.Guardians
            .FirstOrDefaultAsync(g => g.GuardianID == guardianId)?? null!;
    }
}
