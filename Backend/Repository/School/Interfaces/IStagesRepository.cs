using System;
using Azure;
using Backend.DTOS;
using Backend.DTOS.School.Stages;
using Backend.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace Backend.Repository;

public interface IStagesRepository:IgenericRepository<Stage>
{
     Task AddStage(StagesDTO obj);
     Task Update(UpdateStageDTO obj);
     Task SaveAsync();
     Task<List<StageDTO>> GetAll();
     Task<bool> UpdatePartial(int id, JsonPatchDocument<StagesDTO> partialStage);
    
}
