using AutoMapper;
using Azure;
using Backend.Data;
using Backend.DTOS;
using Backend.DTOS.StagesDTO;

using Backend.Models;
using Backend.Repository.IRepository;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository
{
    public class StagesRepository : Repository<Stage>, IStagesRepository
    {
        private readonly DatabaseContext _db;


        public StagesRepository(DatabaseContext db) : base(db)
        {
            _db = db;

        }
        public async Task UpdateAsync(Stage obj)
        {
            _db.Stages.Update(obj);
            await SaveAsync();
        }

        public Task<bool> UpdatePartial(int id, JsonPatchDocument<UpdateStageDTO> partialStage)
        {
            throw new NotImplementedException();
        }

    }
}