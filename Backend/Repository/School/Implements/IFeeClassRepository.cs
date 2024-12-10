using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Fees;
using Backend.Models;

namespace Backend.Repository.School.Implements;

public interface IFeeClassRepository
{  
        Task<List<FeeClassDTO>> GetAllAsync();
        Task<FeeClassDTO> GetByIdAsync(int classId, int feeId);
        Task<List<FeeClassDTO>> GetByIdAsync(int classId);
        Task AddAsync(AddFeeClassDTO feeClass);
        Task UpdateAsync(AddFeeClassDTO feeClass);
        Task DeleteAsync(int classId, int feeId);
        Task<bool> checkIfExist(int classId, int feeId);
}
