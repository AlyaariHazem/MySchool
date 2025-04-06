using AutoMapper;
using Azure;
using Backend.Data;
using Backend.DTOS;
using Backend.DTOS.School.Stages;
using Backend.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository
{
    public class StagesRepository : IStagesRepository
    {
        private readonly DatabaseContext context;
        private readonly IMapper _mapper;

        public StagesRepository(DatabaseContext _context, IMapper mapper)
        {
            context = _context;
            _mapper = mapper;
        }

        public async Task AddStage(StagesDTO model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "The model cannot be null.");

            var year = await context.Years.FirstOrDefaultAsync(x => x.Active == true);//when I debugging it reach to here and stop it does not contounus to end
            // var AddStage=_mapper.Map<Stage>(model);
            Stage newStage = new Stage
            {
                StageName = model.StageName,
                Note = model.Note ?? string.Empty,
                Active = model.Active,
                HireDate = DateTime.Now,
                YearID = year!=null?year.YearID:1
            };

           await context.Stages.AddAsync(newStage);
           await context.SaveChangesAsync();
        }



        public async Task Update(UpdateStageDTO model)
        {
            var existingStage = await context.Stages.FirstOrDefaultAsync(s => s.StageID == model.ID);
            if (existingStage != null)
            {
                existingStage.StageName = model.StageName;
                existingStage.Note = model.Note ?? string.Empty;

                context.Entry(existingStage).State = EntityState.Modified; // Mark the entity as modified
              await context.SaveChangesAsync();
            }
        }


        public async Task DeleteAsync(int id)
        {
            var stage = await context.Stages.FirstOrDefaultAsync(s => s.StageID == id); // Use async to fetch the stage by ID
            if (stage != null)
            {
                context.Stages.Remove(stage); // Remove the stage from the DbSet
                await context.SaveChangesAsync(); // Commit changes to the database
            }
        }

        public async Task<Stage> GetByIdAsync(int id)
        {
            return await context.Stages.FirstOrDefaultAsync(S => S.StageID == id);
        }
        public async Task SaveAsync()
        {
           await context.SaveChangesAsync();
        }
        
        public async Task<List<StageDTO>> GetAll()
        {
            var stageList = await context.Stages
                .Include(stage => stage.Classes)  // Include Classes
                .ThenInclude(c => c.FeeClasses) // Include StudentClass for counting students
                .ThenInclude(fc => fc.StudentClassFees)
                .ToListAsync();
                
            var ListstageDTO = _mapper.Map<List<StageDTO>>(stageList);
            return ListstageDTO;
        }

        public async Task<bool> UpdatePartial(int id, JsonPatchDocument<StagesDTO> partialStage)
        {
            if (partialStage == null || id == 0)
                return false;

            // Retrieve the stage entity by its ID
            var stage = await context.Stages.SingleOrDefaultAsync(s => s.StageID == id);
            if (stage == null)
                return false;

            // Map the stage entity to the DTO (this will be modified)
            var stageDTO = _mapper.Map<StagesDTO>(stage);

            // Apply the patch to the DTO
            partialStage.ApplyTo(stageDTO);

            // Map the patched DTO back to the entity (stage)
            _mapper.Map(stageDTO, stage);

            // Mark the entity as modified and save changes
            context.Entry(stage).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return true;
        }

    }
}