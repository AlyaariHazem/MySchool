using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Backend.Repository.School.Implements;

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
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();
        return account;
    }

    public async Task<AccountStudentGuardian> AddAccountStudentGuardianAsync(AccountStudentGuardian accountStudentGuardian)
    {
      await  _dbContext.AccountStudentGuardians.AddAsync(accountStudentGuardian);
        await _dbContext.SaveChangesAsync();
        return accountStudentGuardian;
    }
}
