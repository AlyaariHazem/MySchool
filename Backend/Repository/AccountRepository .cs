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

    public async Task<AccountReportDTO> GetAccountReportAsync(int accountId)
    {
        // Get account info
        var account = await _dbContext.Accounts
            .FirstOrDefaultAsync(a => a.AccountID == accountId);

        if (account == null)
            throw new Exception($"Account with ID {accountId} not found");

        // Get school info (first school or based on context)
        var school = await _dbContext.Schools
            .Include(s => s.Years)
            .FirstOrDefaultAsync();

        // Get current academic year (active year)
        var currentYear = await _dbContext.Years
            .Where(y => y.Active == true)
            .OrderByDescending(y => y.YearID)
            .FirstOrDefaultAsync();

        var schoolInfo = new SchoolInfoDTO();
        if (school != null)
        {
            schoolInfo.SchoolName = school.SchoolName ?? string.Empty;
            schoolInfo.SchoolAddress = school.Address ?? string.Empty;
            schoolInfo.SchoolPhone = school.SchoolPhone > 0 ? school.SchoolPhone.ToString() : (school.Mobile ?? string.Empty);
            schoolInfo.SchoolLogo = school.ImageURL ?? string.Empty;
            
            // Get academic year from current year dates
            if (currentYear != null)
            {
                var startYear = currentYear.YearDateStart.Year;
                var endYear = currentYear.YearDateEnd?.Year ?? currentYear.YearDateStart.Year + 1;
                schoolInfo.AcademicYear = $"{startYear}-{endYear}";
            }
            else
            {
                // Fallback to current year
                var currentYearNum = DateTime.Now.Year;
                schoolInfo.AcademicYear = $"{currentYearNum}-{currentYearNum + 1}";
            }
        }

        var report = new AccountReportDTO
        {
            AccountID = account.AccountID,
            AccountName = account.AccountName ?? string.Empty,
            HireDate = account.HireDate,
            OpenBalance = account.OpenBalance,
            TypeOpenBalance = account.TypeOpenBalance,
            SchoolInfo = schoolInfo
        };

        // Get AccountStudentGuardian records for this account with student information
        var accountStudentGuardians = await _dbContext.AccountStudentGuardians
            .Where(ag => ag.AccountID == accountId)
            .Include(ag => ag.Student)
                .ThenInclude(s => s.FullName)
            .Include(ag => ag.Student)
                .ThenInclude(s => s.Division)
                    .ThenInclude(d => d.Class)
                        .ThenInclude(c => c.Stage)
            .ToListAsync();

        var accountStudentGuardianIds = accountStudentGuardians
            .Select(ag => ag.AccountStudentGuardianID)
            .ToList();

        // Map students to StudentInfoDTO
        var students = accountStudentGuardians
            .Select(ag => ag.Student)
            .Distinct()
            .Select(s => new StudentInfoDTO
            {
                StudentID = s.StudentID,
                StudentName = (s.FullName?.FirstName ?? "") + " " + 
                             (s.FullName?.MiddleName ?? "") + " " + 
                             (s.FullName?.LastName ?? ""),
                DivisionName = s.Division?.DivisionName,
                ClassName = s.Division?.Class?.ClassName,
                StageName = s.Division?.Class?.Stage?.StageName
            })
            .ToList();

        // Get transactions from StudentClassFees (Debits - fees assigned to students)
        var studentClassFees = await _dbContext.StudentClassFees
            .Include(scf => scf.FeeClass)
            .ThenInclude(fc => fc.Fee)
            .Include(scf => scf.Student)
            .ThenInclude(s => s.AccountStudentGuardians)
            .Where(scf => scf.Student.AccountStudentGuardians.Any(ag => accountStudentGuardianIds.Contains(ag.AccountStudentGuardianID)))
            .ToListAsync();

        var transactions = new List<AccountTransactionDTO>();
        decimal totalDebit = 0;

        foreach (var fee in studentClassFees)
        {
            // Amount is in FeeClass, not Fee
            var feeAmount = (decimal)(fee.FeeClass?.Amount ?? 0);
            var discount = fee.AmountDiscount ?? 0;
            var netAmount = feeAmount - discount;

            if (netAmount > 0)
            {
                // Use Fee HireDate for transaction date
                var transactionDate = fee.FeeClass?.Fee?.HireDate ?? DateTime.Now;

                transactions.Add(new AccountTransactionDTO
                {
                    Id = fee.StudentClassFeesID,
                    Description = fee.FeeClass?.Fee?.FeeName ?? "رسوم دراسية",
                    Type = "Debit",
                    Amount = netAmount,
                    Date = transactionDate,
                    StudentID = fee.StudentID
                });
                totalDebit += netAmount;
            }
        }

        // Get payments/credits from Vouchers (Credits - payments made)
        var vouchers = await _dbContext.Vouchers
            .Include(v => v.AccountStudentGuardians)
            .Where(v => accountStudentGuardianIds.Contains(v.AccountStudentGuardianID))
            .OrderBy(v => v.HireDate)
            .ToListAsync();

        decimal totalCredit = 0;
        var savings = new List<AccountSavingsDTO>();

        foreach (var voucher in vouchers)
        {
            if (voucher.Receipt > 0)
            {
                // Payment/credit transaction
                transactions.Add(new AccountTransactionDTO
                {
                    Id = voucher.VoucherID,
                    Description = voucher.Note ?? "دفعة",
                    Type = "Credit",
                    Amount = voucher.Receipt,
                    Date = voucher.HireDate,
                    StudentID = voucher.AccountStudentGuardians?.StudentID
                });
                totalCredit += voucher.Receipt;

                // Also add to savings (deposits)
                savings.Add(new AccountSavingsDTO
                {
                    Id = voucher.VoucherID,
                    Description = voucher.Note ?? "قسط",
                    Type = true, // Savings/deposit
                    Amount = voucher.Receipt,
                    Date = voucher.HireDate
                });
            }
        }

        // Calculate balance
        var initialBalance = account.OpenBalance ?? 0;
        if (account.TypeOpenBalance)
        {
            // If TypeOpenBalance is true, it's a debit (owed)
            totalDebit += initialBalance;
        }
        else
        {
            // If false, it's a credit (paid in advance)
            totalCredit += initialBalance;
        }

        report.Transactions = transactions.OrderBy(t => t.Date).ToList();
        report.Savings = savings.OrderBy(s => s.Date).ToList();
        report.Students = students;
        report.TotalDebit = totalDebit;
        report.TotalCredit = totalCredit;
        report.Balance = totalDebit - totalCredit;

        return report;
    }

    public async Task<int> GetAccountIdByAccountStudentGuardianIdAsync(int accountStudentGuardianId)
    {
        var accountStudentGuardian = await _dbContext.AccountStudentGuardians
            .FirstOrDefaultAsync(ag => ag.AccountStudentGuardianID == accountStudentGuardianId);

        if (accountStudentGuardian == null)
            throw new Exception($"AccountStudentGuardian with ID {accountStudentGuardianId} not found");

        return accountStudentGuardian.AccountID;
    }
}
