using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Accounts;
using Backend.Models;

namespace Backend.Repository.School.Implements;

public interface IAccountRepository
{
    Task<Accounts> AddAccountAsync(Accounts account);
    Task<AccountStudentGuardian> AddAccountStudentGuardianAsync(AccountStudentGuardian accountStudentGuardian);
    Task<AccountStudentGuardian> GetAccountStudentGuardianByGuardianIdAsync(int guardianId);
    Task<List<AccountsDTO>> GetAllAccounts();

}
