using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Common;
using Backend.DTOS.School.FeeClass;
using Backend.DTOS.School.Fees;
using Backend.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace Backend.Repository.School.Implements;

public interface IFeeClassRepository
{
        Task<Result<List<FeeClassDTO>>> GetAllAsync();
        Task<Result<FeeClassDTO>> GetByIdAsync(int feeClassID);
        Task<Result<List<FeeClassDTO>>> GetAllByClassIdAsync(int classId);

        Task<Result<bool>> AddAsync(AddFeeClassDTO feeClass);
        Task<Result<bool>> UpdateAsync(int feeClassID, AddFeeClassDTO feeClass);
        Task<Result<bool>> DeleteAsync(int feeClassID);
        Task<Result<bool>> UpdatePartial(int id, JsonPatchDocument<ChangeStateFeeClassDTO> partialClass);
}
