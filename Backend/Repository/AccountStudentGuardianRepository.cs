using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Common;
using Backend.Data;
using Backend.DTOS.School.AccountStudentGuardian;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class AccountStudentGuardianRepository : IAccountStudentGuardianRepository
{
    private readonly TenantDbContext _context;
    private readonly IMapper _mapper;
    public AccountStudentGuardianRepository(TenantDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<AccountStudentGuardianDTO>> GetAcountStudentGuardianByIdAsync(int studentId)
    {
        try
        {
            var entity = await _context.AccountStudentGuardians
                .FirstOrDefaultAsync(x => x.StudentID == studentId);

            if (entity == null)
                return Result<AccountStudentGuardianDTO>.Fail("Not found.");

            var dto = _mapper.Map<AccountStudentGuardianDTO>(entity);
            return Result<AccountStudentGuardianDTO>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<AccountStudentGuardianDTO>.Fail($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<AccountsGuardiansDTO>>> GetAllAcountStudentGuardianAsync()
    {
        var guardians = await _context.AccountStudentGuardians
    .Include(g => g.Guardian)
    .GroupBy(g => new { g.GuardianID, g.Guardian.FullName })
    .Select(g => new AccountsGuardiansDTO
    {
        GuardianID = g.Key.GuardianID,
        GuardianName = g.Key.FullName,
        AccountStudentGuardianID = g.Select(x => x.AccountStudentGuardianID).FirstOrDefault(),
    })
    .OrderBy(g => g.GuardianName)
    .ToListAsync();

        if (guardians == null || !guardians.Any())
            return Result<List<AccountsGuardiansDTO>>.Fail("No data found.");

        return Result<List<AccountsGuardiansDTO>>.Success(guardians);
    }
    public async Task<AccountStudentGuardian> GetAccountStudentGuardianByGuardianIdAsync(int guardianId) =>
         await _context.AccountStudentGuardians.FirstOrDefaultAsync(a => a.GuardianID == guardianId);

    public async Task<AccountStudentGuardian> AddAccountStudentGuardianAsync(AccountStudentGuardian accountStudentGuardian)
    {
        await _context.AccountStudentGuardians.AddAsync(accountStudentGuardian);
        await _context.SaveChangesAsync();
        return accountStudentGuardian;
    }
}
