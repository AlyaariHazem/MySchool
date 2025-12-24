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
    private readonly TenantDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;

    public GuardianRepository(TenantDbContext db, IMapper mapper, IUserRepository userRepository)
    {
        _db = db;
        _mapper = mapper;
        _userRepository = userRepository;
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
        // ApplicationUser is in admin database, not tenant database, so we can't Include it
        // Filter guardians that have students in active years
        var guardiansData = await _db.Guardians
            .Include(g => g.AccountStudentGuardians)
                .ThenInclude(asg => asg.Student)
                    .ThenInclude(s => s.Division)
                        .ThenInclude(d => d.Class)
                            .ThenInclude(c => c.Year)
            .Include(g => g.AccountStudentGuardians)
                .ThenInclude(asg => asg.Student)
                    .ThenInclude(s => s.Division)
                        .ThenInclude(d => d.Class)
                            .ThenInclude(c => c.Stage)
                                .ThenInclude(s => s.Year)
            .Where(g => g.AccountStudentGuardians != null && 
                       g.AccountStudentGuardians.Any(asg => asg.Student != null &&
                                                           asg.Student.Division != null &&
                                                           asg.Student.Division.Class != null &&
                                                           ((asg.Student.Division.Class.Year != null && asg.Student.Division.Class.Year.Active == true) || 
                                                            (asg.Student.Division.Class.Stage != null && asg.Student.Division.Class.Stage.Year != null && asg.Student.Division.Class.Stage.Year.Active == true))))
            .ToListAsync();

        if (guardiansData == null)
            throw new Exception("Guardian not found.");

        // Fetch all user data in batch from admin database
        var userIds = guardiansData.Select(g => g.UserID).Distinct().ToList();
        var users = new Dictionary<string, ApplicationUser>();
        foreach (var userId in userIds)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user != null)
            {
                users[userId] = user;
            }
        }

        var mappedGuardians = guardiansData.Select(guardian =>
        {
            var user = users.ContainsKey(guardian.UserID) ? users[guardian.UserID] : null;
            return new GuardianDTO
            {
                GuardianID = guardian.GuardianID,
                FullName = guardian.FullName,
                Gender = user?.Gender ?? string.Empty,
                Type = guardian.Type,
                UserID = guardian.UserID,
                GuardianAddress = user?.Address ?? string.Empty,
                GuardianDOB = guardian.GuardianDOB,
                GuardianEmail = user?.Email ?? string.Empty,
                GuardianPhone = user?.PhoneNumber ?? string.Empty
            };
        }).ToList();

        return mappedGuardians;
    }

    public async Task<GuardianDTO> GetGuardianByIdAsync(int guardianId)
    {
        // ApplicationUser is in admin database, not tenant database, so we can't Include it
        var guardianData = await _db.Guardians
            .FirstOrDefaultAsync(g => g.GuardianID == guardianId);

        if (guardianData == null)
            return null!;

        // Fetch user data from admin database separately
        var user = await _userRepository.GetUserByIdAsync(guardianData.UserID);

        var guardian = new GuardianDTO
        {
            GuardianID = guardianData.GuardianID,
            FullName = guardianData.FullName,
            Gender = user?.Gender ?? string.Empty,
            Type = guardianData.Type,
            UserID = guardianData.UserID,
            GuardianAddress = user?.Address ?? string.Empty,
            GuardianDOB = guardianData.GuardianDOB,
            GuardianEmail = user?.Email ?? string.Empty,
            GuardianPhone = user?.PhoneNumber ?? string.Empty
        };
        return guardian;
    }

    public async Task<GuardianDTO> GetGuardianByIdForUpdateAsync(int guardianId)
    {
        // ApplicationUser is in admin database, not tenant database, so we can't Include it
        var guardianData = await _db.Guardians
            .FirstOrDefaultAsync(g => g.GuardianID == guardianId);
        
        if (guardianData == null)
            throw new Exception("Guardian not found.");

        // Fetch user data from admin database separately
        var user = await _userRepository.GetUserByIdAsync(guardianData.UserID);

        var guardian = new GuardianDTO
        {
            GuardianID = guardianData.GuardianID,
            FullName = guardianData.FullName,
            Gender = user?.Gender ?? string.Empty,
            Type = guardianData.Type,
            UserID = guardianData.UserID,
            GuardianAddress = user?.Address ?? string.Empty,
            GuardianDOB = guardianData.GuardianDOB,
            GuardianEmail = user?.Email ?? string.Empty,
            GuardianPhone = user?.PhoneNumber ?? string.Empty
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
            
            // Update user data in admin database (ApplicationUser is not in TenantDbContext)
            var user = await _userRepository.GetUserByIdAsync(guardianExist.UserID);
            if (user != null)
            {
                user.Address = guardian.GuardianAddress;
                user.Email = guardian.GuardianEmail;
                user.PhoneNumber = guardian.GuardianPhone;
                await _userRepository.UpdateAsync(user);
            }
            
            _db.Entry(guardianExist).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }
    }
    public async Task<List<GuardiansInfo>> GetAllGuardiansInfoAsync()
    {
        // Use AsNoTracking to prevent duplicate entity tracking issues
        // ApplicationUser is in admin database, not tenant database, so we can't Include it
        var guardiansData = await _db.Guardians
            .AsNoTracking()
            .Include(g => g.AccountStudentGuardians)
                .ThenInclude(ASG => ASG.Student)
                    .ThenInclude(s => s.Division)
                        .ThenInclude(d => d.Class)
                            .ThenInclude(c => c.Year)
            .Include(g => g.AccountStudentGuardians)
                .ThenInclude(ASG => ASG.Student)
                    .ThenInclude(s => s.Division)
                        .ThenInclude(d => d.Class)
                            .ThenInclude(c => c.Stage)
                                .ThenInclude(s => s.Year)
            .Include(g => g.AccountStudentGuardians)
                .ThenInclude(ASG => ASG.Vouchers)
            .Include(g => g.AccountStudentGuardians)
                .ThenInclude(ASG => ASG.Accounts)
            .Where(g => g.AccountStudentGuardians != null && 
                       g.AccountStudentGuardians.Any(asg => asg.Student != null &&
                                                           asg.Student.Division != null &&
                                                           asg.Student.Division.Class != null &&
                                                           ((asg.Student.Division.Class.Year != null && asg.Student.Division.Class.Year.Active == true) || 
                                                            (asg.Student.Division.Class.Stage != null && asg.Student.Division.Class.Stage.Year != null && asg.Student.Division.Class.Stage.Year.Active == true))))
            .AsSplitQuery()
            .ToListAsync();

        if (guardiansData == null)
            throw new Exception("Guardian not found.");

        // Fetch all user data in batch from admin database
        var userIds = guardiansData.Select(g => g.UserID).Distinct().ToList();
        var users = new Dictionary<string, ApplicationUser>();
        foreach (var userId in userIds)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user != null)
            {
                users[userId] = user;
            }
        }

        // Group by GuardianID to ensure we only get one record per guardian
        // Filter out guardians with no students (StudentCount = 0)
        var mappedGuardians = guardiansData
            .GroupBy(g => g.GuardianID)
            .Select(group => 
            {
                var guardian = group.First();
                var user = users.ContainsKey(guardian.UserID) ? users[guardian.UserID] : null;
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
                    Gender = user?.Gender ?? "Unknown",
                    StudentCount = studentCount,
                    RequiredFee = requiredFee,
                    Piad = piad,
                    Remaining = requiredFee - piad,
                    Address = user?.Address ?? "N/A",
                    DOB = guardian.GuardianDOB,
                    Phone = user?.PhoneNumber ?? "N/A",
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
