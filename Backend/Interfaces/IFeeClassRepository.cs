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
        Task<FeeClassDTO> GetByIdAsync(int feeClassID);
        Task<List<FeeClassDTO>> GetAllByClassIdAsync(int classId);
        Task AddAsync(AddFeeClassDTO feeClass);
        Task UpdateAsync(int feeClassID,AddFeeClassDTO feeClass);
        Task DeleteAsync(int feeClassID);
        Task<bool> checkIfExist(int feeClassID);
}
