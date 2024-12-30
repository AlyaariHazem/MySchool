using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Backend.DTOS.yearsDTO;
using Backend.Models;

namespace Backend.Services.IServices
{
    public interface IYearServices
    {
        public Task<bool> AddAsync(YearDTO obj);
        Task<bool> UpdateAsync(YearDTO obj);
        Task<YearDTO> GetAsync(Expression<Func<Year, bool>> filter);
        Task<List<YearDTO>> GetAllAsync(Expression<Func<Year, bool>> filter = null);
        Task<bool> DeleteAsync(int id);

    }
}


