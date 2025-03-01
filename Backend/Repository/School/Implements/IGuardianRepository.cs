using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Guardians;
using Backend.Models;

namespace Backend.Repository.School.Implements;

public interface IGuardianRepository
{
    Task<GuardianDTO> AddGuardianAsync(Guardian guardian);
    Task<List<GuardianDTO>> GetAllGuardiansAsync();
    Task<GuardianDTO> GetGuardianByIdAsync(int guardianId);
    Task<GuardianDTO> GetGuardianByIdForUpdateAsync(int guardianId);
    Task UpdateGuardianAsync(GuardianDTO guardian);
}
