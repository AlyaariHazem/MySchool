using System;
using Backend.DTOS;
using Backend.Models;

namespace Backend.Repository;

public interface IStagesRepository:IgenericRepository<Stage>
{
    public void Add(StagesDTO obj);
    public void Update(StagesDTO obj);
     public void Save();
    public List<StageModel> DisplayStages();
    
}
