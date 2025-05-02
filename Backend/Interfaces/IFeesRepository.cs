using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Common;
using Backend.DTOS.School.Fees;
using Backend.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace Backend.Repository.School.Interfaces;

public interface IFeesRepository
{
    Task<Result<List<GetFeeDTO>>> GetAllAsync();
    Task<Result<GetFeeDTO>> GetByIdAsync(int id);
    Task<Result<bool>> AddAsync(FeeDTO fee);
    Task<Result<bool>> UpdateAsync(FeeDTO fee);
    Task<Result<bool>> DeleteAsync(int id);
    Task<Result<bool>> UpdatePartial(int id, JsonPatchDocument<ChangeStateFeeDTO> partialClass);

}
