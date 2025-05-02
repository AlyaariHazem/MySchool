using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Common;
using Backend.DTOS.School.AccountStudentGuardian;
using Backend.Models;

namespace Backend.Interfaces;

public interface IAccountStudentGuardianRepository
{
    // Task<Result<bool>> AddAcountStudentGuardianAsync(AccountStudentGuardianDTO dto);
    // Task<Result<bool>> UpdateAcountStudentGuardianAsync(AccountStudentGuardianDTO dto);
    Task<Result<List<AccountsGuardiansDTO>>> GetAllAcountStudentGuardianAsync();
    Task<Result<AccountStudentGuardianDTO>> GetAcountStudentGuardianByIdAsync(int studentId);
    Task<AccountStudentGuardian> GetAccountStudentGuardianByGuardianIdAsync(int guardianId);
    Task<AccountStudentGuardian> AddAccountStudentGuardianAsync(AccountStudentGuardian accountStudentGuardian);
}
