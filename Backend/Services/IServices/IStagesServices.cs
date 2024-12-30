using System;
using System.Linq.Expressions;
using Azure;
using Backend.DTOS;
using Backend.DTOS.StagesDTO;
using Backend.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace Backend.Services.IServices
{
     public interface IStagesServices
     {
          Task<bool> AddAsync(AddStageDTO obj);
          Task<bool> UpdateAsync(UpdateStageDTO obj);
          Task<StageDTO> GetAsync(Expression<Func<Stage, bool>> filter);
          Task<List<StageDTO>> GetAllAsync(Expression<Func<Stage, bool>> filter = null);
          Task<bool> UpdatePartialAsync(int id, JsonPatchDocument<UpdateStageDTO> partialStage);
          Task<bool> DeleteAsync(int id);

     }

}


