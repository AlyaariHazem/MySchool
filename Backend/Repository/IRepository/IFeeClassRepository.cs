using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Backend.Models;

namespace Backend.Repository.IRepository;

public interface IFeeClassRepository : IRepository<FeeClass>
{
        /*    Task<List<FeeClassDTO>> GetAllAsync();
           Task<FeeClassDTO> GetByIdAsync(int feeClassID);
           Task<List<FeeClassDTO>> GetAllByClassIdAsync(int classId);
           Task AddAsync(AddFeeClassDTO feeClass); */
        Task UpdateAsync(FeeClass obj);
        /* Task DeleteAsync(int feeClassID); */
        Task<bool> checkIfExist(int feeClassID);
}
