using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Accounts;
using Backend.Models;

namespace Backend.Repository.School.Implements;

public interface IAccountRepository
{
    Task<AccountsDTO> AddAccountAsync(AccountsDTO account);
    Task<List<AccountsDTO>> GetAllAccounts();
    Task<List<StudentAndAccountNames>> GetStudentAndAccountNamesAllAsync();
    Task<AccountsDTO> GetAccountByIdAsync(int id);
    Task UpdateAccountAsync(AccountsDTO account);
    Task DeleteAccountAsync(int id);

}
