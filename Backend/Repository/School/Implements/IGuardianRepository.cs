using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Guardians;
using Backend.Models;

namespace Backend.Repository.School.Implements;

public interface IGuardianRepository
{
    Task<Guardian> AddGuardianAsync(Guardian guardian);
    Task<List<Guardian>> GetAllGuardiansAsync();
    Task<Guardian> GetGuardianByIdAsync(int guardianId);
    Task<GetGuardianDTO> GetGuardianByIdForUpdateAsync(int guardianId);
    Task UpdateGuardianAsync(Guardian guardian);
}
