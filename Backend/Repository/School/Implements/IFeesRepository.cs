using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Fees;
using Backend.Models;

namespace Backend.Repository.School.Interfaces;

public interface IFeesRepository
{
    Task<List<GetFeeDTO>> GetAllAsync();
    Task<GetFeeDTO> GetByIdAsync(int id);
    Task AddAsync(FeeDTO fee);
    Task UpdateAsync(FeeDTO fee);
    Task DeleteAsync(int id);

}
