using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.DTOS.School.Accounts;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School.Classes;

public class AccountRepository : IAccountRepository
{
    private readonly DatabaseContext _dbContext;

    public AccountRepository(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Accounts> AddAccountAsync(Accounts account)
    {
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.SaveChangesAsync();
        return account;
    }

    public async Task<AccountStudentGuardian> AddAccountStudentGuardianAsync(AccountStudentGuardian accountStudentGuardian)
    {
        await _dbContext.AccountStudentGuardians.AddAsync(accountStudentGuardian);
        await _dbContext.SaveChangesAsync();
        return accountStudentGuardian;
    }

    public async Task<AccountStudentGuardian> GetAccountStudentGuardianByGuardianIdAsync(int guardianId)
    {
        return await _dbContext.AccountStudentGuardians.FirstOrDefaultAsync(a => a.GuardianID == guardianId);
    }

    public async Task<List<AccountsDTO>> GetAllAccounts()
    {
        var accounts = await _dbContext.Guardians
        .Include(g => g.AccountStudentGuardians)
        .ThenInclude(a => a.Accounts)
        .ToListAsync();
        return accounts.Select(guardian => new AccountsDTO
        {
            AccountID = guardian.AccountStudentGuardians.FirstOrDefault()?.Accounts.AccountID,
            GuardianName = guardian.FullName,
            State = guardian.AccountStudentGuardians.FirstOrDefault()?.Accounts.State ?? true,
            Note = guardian.AccountStudentGuardians.FirstOrDefault()?.Accounts.Note,
            OpenBalance = guardian.AccountStudentGuardians.FirstOrDefault()?.Accounts.OpenBalance,
            TypeOpenBalance = guardian.AccountStudentGuardians.FirstOrDefault()?.Accounts.TypeOpenBalance ?? false,
            HireDate = guardian.AccountStudentGuardians.FirstOrDefault()?.Accounts.HireDate ?? DateTime.Now,
            TypeAccountID = guardian.AccountStudentGuardians.FirstOrDefault()?.Accounts.TypeAccountID ?? 1
        }).ToList();
    }
}
