using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS.School.Accounts;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School.Classes;

public class AccountRepository : IAccountRepository
{
    private readonly TenantDbContext _dbContext;
    private readonly IMapper _mapper;

    public AccountRepository(TenantDbContext dbContext, IMapper mapper)
    {
        _mapper = mapper;

        _dbContext = dbContext;
    }

    public async Task<AccountsDTO> AddAccountAsync(AccountsDTO account)
    {
        var accountEntity = _mapper.Map<Accounts>(account);

        await _dbContext.Accounts.AddAsync(accountEntity);
        await _dbContext.SaveChangesAsync();
        return _mapper.Map<AccountsDTO>(accountEntity);
    }

        public async Task<List<AccountsDTO>> GetAllAccounts()
    {
        var accounts = await _dbContext.Accounts
        .ToListAsync();
        var accountDTOs = _mapper.Map<List<AccountsDTO>>(accounts);
        return accountDTOs;
    }

    public async Task UpdateAccountAsync(AccountsDTO account)
    {
        var updatedAccount = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.AccountID == account.AccountID);

        updatedAccount!.AccountName = account.AccountName;
        updatedAccount.TypeOpenBalance = account.TypeOpenBalance;
        updatedAccount.HireDate = account.HireDate;
        updatedAccount.OpenBalance = account.OpenBalance;
        updatedAccount.Note = account.Note;
        updatedAccount.TypeAccountID = account.TypeAccountID;
        updatedAccount.State = account.State;

        _dbContext.Entry(updatedAccount).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<AccountsDTO> GetAccountByIdAsync(int id)
    {
        var Accounts = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.AccountID == id);

        var accountDTO = _mapper.Map<AccountsDTO>(Accounts);
        return accountDTO ?? throw new Exception("Account not found");
    }

    public async Task DeleteAccountAsync(int id)
    {
        var account = await GetAccountByIdAsync(id);
        if (account != null)
        {
            var accounts = _mapper.Map<Accounts>(account);
            _dbContext.Accounts.Remove(accounts);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<List<StudentAndAccountNames>> GetStudentAndAccountNamesAllAsync()
    {
        var studentAndAccountNames = await _dbContext.AccountStudentGuardians
            .Include(a => a.Accounts)
            .Include(a => a.Student)
            .Select(a => new StudentAndAccountNames
            {
                StudentName = a.Student.FullName.FirstName + " " + a.Student.FullName.MiddleName + " " + a.Student.FullName.LastName,
                StudentID = a.Student.StudentID,
                GuardianID = a.GuardianID,
                AccountStudentGuardianID = a.AccountStudentGuardianID,
                AccountName = a.Accounts.AccountName
            }).ToListAsync();
        if (studentAndAccountNames == null)
            throw new Exception("No data found");
        return studentAndAccountNames;
    }
}
