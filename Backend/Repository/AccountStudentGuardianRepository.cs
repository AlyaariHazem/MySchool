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
    private readonly DatabaseContext _context;
    private readonly IMapper _mapper;
    public AccountStudentGuardianRepository(DatabaseContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public Task<Result<bool>> AddAcountStudentGuardianAsync(AccountStudentGuardianDTO dto)
    {
        throw new NotImplementedException();
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

    public Task<Result<List<AccountStudentGuardianDTO>>> GetAllAcountStudentGuardianAsync()
    {
        throw new NotImplementedException();
    }
    public async Task<AccountStudentGuardian> GetAccountStudentGuardianByGuardianIdAsync(int guardianId) =>
         await _context.AccountStudentGuardians.FirstOrDefaultAsync(a => a.GuardianID == guardianId);

    public async Task<AccountStudentGuardian> AddAccountStudentGuardianAsync(AccountStudentGuardian accountStudentGuardian)
    {
        await _context.AccountStudentGuardians.AddAsync(accountStudentGuardian);
        await _context.SaveChangesAsync();
        return accountStudentGuardian;
    }
    public Task<Result<bool>> UpdateAcountStudentGuardianAsync(AccountStudentGuardianDTO dto)
    {
        throw new NotImplementedException();
    }
}
