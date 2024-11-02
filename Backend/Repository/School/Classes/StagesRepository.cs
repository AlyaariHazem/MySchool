using Backend.Data;
using Backend.DTOS;
using Backend.Models;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository
{
    public class StagesRepository : IStagesRepository
    {
        DatabaseContext context;
        public StagesRepository(DatabaseContext _context)
        {
            context = _context;
        }
        public void Add(StagesDTO model)
        {

            Stage newStage = new Stage
            {
                StageName = model.StageName,
                Note = model.Note ?? string.Empty,
                Active = model.Active,
                HireDate = model.HireDate,
                YearID = model.YearID
            };

            context.Add(newStage);
            Save();
        }

        public void Update(StagesDTO model)
        {
            var existingStage = context.Stages.FirstOrDefault(s => s.StageID == model.ID);
            if (existingStage != null)
            {
                existingStage.StageName = model.StageName;
                existingStage.Note = model.Note ?? string.Empty;

                context.Entry(existingStage).State = EntityState.Modified; // Mark the entity as modified
                Save(); // Save changes
            }
        }



        public void Delete(int id)
        {
            var stage = GetById(id); // Fetch the stage by ID
            if (stage != null)
            {
                context.Stages.Remove(stage); // Remove the stage from the DbSet
                Save(); // Commit changes to the database
            }
        }

        public Stage GetById(int id)
        {
            return context.Stages.FirstOrDefault(S => S.StageID == id)!;
        }
        public void Save()
        {
            context.SaveChanges();
        }
        public List<StageModel> DisplayStages()
        {
            var stages = context.Stages
            .Select(stage => new StageModel
            {
                StageID = stage.StageID,
                StageName = stage.StageName,
                Note = stage.Note ?? string.Empty,
                Active = stage.Active,
                Classes = stage.Classes.ToList(),
                Students = stage.Classes.SelectMany(c => c.StudentClass)
                                          .Select(sc => sc.Student)
                                          .ToList()!,
                StudentCount = stage.Classes.SelectMany(c => c.StudentClass).Count()
            }).ToList();
            return stages;
        }

    }
}