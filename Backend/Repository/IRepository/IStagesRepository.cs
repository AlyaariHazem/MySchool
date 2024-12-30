using System;
using Azure;
using Backend.DTOS;
using Backend.DTOS.StagesDTO;

using Backend.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace Backend.Repository.IRepository;

public interface IStagesRepository : IRepository<Stage>
{

     Task UpdateAsync(Stage obj);


     Task<bool> UpdatePartial(int id, JsonPatchDocument<UpdateStageDTO> partialStage);

}
