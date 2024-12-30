using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Backend.DTOS;
using Backend.DTOS.FeeClassesDTO;

using Backend.Models;

namespace Backend.Services.IServices
{

        public interface IFeeClassServices
        {
                Task<List<FeeClassDTO>> GetAllAsync(Expression<Func<FeeClass, bool>> filter = null);
                Task<FeeClassDTO> GetAsync(Expression<Func<FeeClass, bool>> filter);
                Task<bool> AddAsync(AddFeeClassDTO feeClass);
                Task<bool> UpdateAsync(int feeClassID, UpdateFeeClassDTO feeClass);
                Task<bool> DeleteAsync(int feeClassID);
                Task<bool> checkIfExist(int feeClassID);
        }
}


