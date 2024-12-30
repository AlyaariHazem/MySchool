using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Backend.DTOS.FeesDTO;

using Backend.Models;

namespace Backend.Services.IServices
{

    public interface IFeesServices
    {
        Task<List<FeeDTO>> GetAllAsync(Expression<Func<Fee, bool>> filter = null);
        Task<FeeDTO> GetAsync(Expression<Func<Fee, bool>> filter);
        Task<bool> AddAsync(AddFeeDTO fee);
        Task<bool> UpdateAsync(UpdateFeeDTO fee);
        Task<bool> DeleteAsync(int id);

    }



}

