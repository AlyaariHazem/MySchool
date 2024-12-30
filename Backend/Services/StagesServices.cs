using System.Linq.Expressions;
using AutoMapper;
using Azure;
using Backend.Data;
using Backend.DTOS;
using Backend.DTOS.ClassesDTO;
using Backend.DTOS.StagesDTO;
using Backend.Models;
using Backend.Repository.IRepository;
using Backend.Services.IServices;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;

namespace Backend.Services
{
    public class StagesServices : IStagesServices
    {

        private readonly IStagesRepository _stagesRepository;
        private readonly IStudentRepository _studentRepository;

        private readonly IClassesRepository _classesRepository;

        public StagesServices(IStagesRepository stagesRepository, IStudentRepository studentRepository, IClassesRepository classesRepository)
        {

            _stagesRepository = stagesRepository;
            _studentRepository = studentRepository;
            _classesRepository = classesRepository;
        }


        public async Task<bool> AddAsync(AddStageDTO obj)
        {
            try
            {
                var newStage = new Stage()
                {
                    StageName = obj.StageName,
                    Note = obj.Note,
                    Active = obj.Active,
                    HireDate = DateTime.Now,
                    YearID = obj.YearID

                };
                await _stagesRepository.CreateAsync(newStage);
                return true;
            }
            catch
            {
                return false;
            }




        }

        async Task<bool> IStagesServices.UpdateAsync(UpdateStageDTO obj)
        {
            var existingStage = await _stagesRepository.GetAsync(s => s.StageID == obj.ID);
            if (existingStage != null)
            {
                existingStage.StageName = obj.StageName;
                existingStage.Note = obj.Note;
                existingStage.Active = obj.Active;

                await _stagesRepository.UpdateAsync(existingStage);
                return true;
            }
            return false;
        }

        async Task<StageDTO> IStagesServices.GetAsync(Expression<Func<Stage, bool>> filter)
        {
            var stage = await _stagesRepository.GetAsync(filter, includeProperties: "Classes ");
            if (stage == null)
            {
                return null;
            }
            var stageDTO = new StageDTO()
            {
                StageID = stage.StageID,
                StageName = stage.StageName,
                Note = stage.Note,
                Active = stage.Active,


            };
            return stageDTO;
        }

        public async Task<List<StageDTO>> GetAllAsync(Expression<Func<Stage, bool>> filter = null)
        {
            var stages = await _stagesRepository.GetAllAsync(filter);
            var allStudent = await _studentRepository.GetAllAsync(s => s.DivisionID != null);
            var stagesDTO = new List<StageDTO>();

            foreach (var stage in stages)
            {
                int StudenCountINStage = 0;
                var stageDTO = (new StageDTO()
                {
                    StageID = stage.StageID,
                    StageName = stage.StageName,
                    Note = stage.Note,
                    Active = stage.Active,
                });
                var Classes = await _classesRepository.GetAllAsync(c => c.StageID == stage.StageID, includeProperties: "Divisions");
                foreach (var Class in Classes)
                {
                    var StudenCountINClass = allStudent.Where(s => Class.Divisions.Any(D => D.DivisionID == s.DivisionID)).ToList().Count;
                    stageDTO.Classes.Add(new ClassInStageDTO()
                    {
                        ClassID = Class.ClassID,
                        ClassName = Class.ClassName,
                        StudentCount = StudenCountINClass


                    });
                    StudenCountINStage += StudenCountINClass;
                }
                stageDTO.StudentCount = StudenCountINStage;
                stagesDTO.Add(stageDTO);



            }
            return stagesDTO;
        }
        public async Task<bool> UpdatePartialAsync(int id, JsonPatchDocument<UpdateStageDTO> partialStage)
        {
            if (partialStage == null || id == 0)
                return false;

            var stage = await _stagesRepository.GetAsync(s => s.StageID == id);
            if (stage == null)
                return false;

            var stageDTO = new UpdateStageDTO()
            {
                StageName = stage.StageName,
                Note = stage.Note,
                Active = stage.Active
            };
            partialStage.ApplyTo(stageDTO);

            stage.StageName = stageDTO.StageName;
            stage.Note = stageDTO.Note;
            stage.Active = stageDTO.Active;

            await _stagesRepository.UpdateAsync(stage);
            return true;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var stage = await _stagesRepository.GetAsync(s => s.StageID == id);
            if (stage != null)
            {
                await _stagesRepository.RemoveAsync(stage);
                return true;
            }
            return false;
        }
    }
}










