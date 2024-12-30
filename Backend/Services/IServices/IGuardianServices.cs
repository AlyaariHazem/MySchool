using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Backend.DTOS.GuardiansDTO;

using Backend.Models;

namespace Backend.Services.IServices
{
    public interface IGuardianServices
    {
        Task<Guardian> AddAsync(Guardian guardian);
        Task<List<Guardian>?> GetAllAsync(Expression<Func<Guardian, bool>> filter = null);
        Task<Guardian> GetAsync(Expression<Func<Guardian, bool>> filter);
        Task<UpdateGuardianDTO> GetForUpdateAsync(int guardianId);
        Task<bool> DeleteAsync(int guardianId);
        Task<bool> UpdateAsync(UpdateGuardianDTO guardian);
    }
}


