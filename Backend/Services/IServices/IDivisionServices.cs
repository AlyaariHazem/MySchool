using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Backend.DTOS;
using Backend.DTOS.DivisionsDTO;

using Backend.Models;
using Backend.Repository;
using Microsoft.AspNetCore.JsonPatch;

namespace Backend.Services.IServices
{
    public interface IDivisionServices
    {
        Task<List<DivisionDTO>> GetAllAsync(Expression<Func<Division, bool>> filter = null);
        Task<bool> AddAsync(AddDivisionDTO model);
        Task<bool> UpdateAsync(UpdateDivisionDTO model);
        Task<DivisionDTO> GetAsync(Expression<Func<Division, bool>> filter);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdatePartialAsync(int id, JsonPatchDocument<UpdateDivisionDTO> partialClass);
    }
}