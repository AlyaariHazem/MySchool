using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using Backend.Data;
using Backend.DTOS;
using Backend.DTOS.ClassesDTO;
using Backend.DTOS.DivisionsDTO;
using Backend.Models;
using Backend.Repository.IRepository;
using Backend.Services.IServices;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class ClassesServices : IClassesServices
    {

        private readonly IClassesRepository _classesRepository;
        private readonly IStudentRepository _studentRepository;




        public ClassesServices(IClassesRepository classesRepository, IStudentRepository studentRepository)
        {

            _classesRepository = classesRepository;
            _studentRepository = studentRepository;
        }

        public async Task<bool> AddAsync(AddClassDTO obj)
        {
            try
            {
                var Class = new Class()
                {
                    ClassName = obj.ClassName,
                    StageID = obj.StageID
                };

                await _classesRepository.CreateAsync(Class);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public async Task<bool> UpdateAsync(int Id, UpdateClassDTO model)
        {
            try
            {
                var existingClass = await _classesRepository.GetAsync(c => c.ClassID == Id, tracked: false);
                if (existingClass != null)
                {

                    existingClass.ClassName = model.ClassName;
                    existingClass.State = model.State;
                    await _classesRepository.UpdateAsync(existingClass);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var existingClass = await _classesRepository.GetAsync(c => c.ClassID == id);
                if (existingClass != null)
                {
                    await _classesRepository.RemoveAsync(existingClass);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public async Task<ClassDTO> GetAsync(Expression<Func<Class, bool>> filter)
        {
            try
            {
                var Class = await _classesRepository.GetAsync(filter, includeProperties: "Stage,Divisions");

                if (Class == null)
                {
                    return null;
                }
                var ClassDTO = new ClassDTO()
                {
                    ClassID = Class.ClassID,
                    ClassName = Class.ClassName,
                    StageID = Class.StageID,
                    StageName = Class.Stage.StageName

                };
                foreach (var division in Class.Divisions)
                {
                    ClassDTO.Divisions.Add(new DivisionINClassDTO()
                    {
                        DivisionID = division.DivisionID,
                        DivisionName = division.DivisionName,
                        StudentCount = _studentRepository.GetCount(s => s.DivisionID == division.DivisionID).Result
                    });
                }
                return ClassDTO;
            }
            catch (Exception)
            {
                return null;
            }

        }


        public async Task<List<ClassDTO>> GetAllAsync(Expression<Func<Class, bool>> filter = null)
        {

            var ClassList = await _classesRepository.GetAllAsync(filter, includeProperties: "Stage,Divisions");
            List<ClassDTO> ClassDTOList = new();
            foreach (var Class in ClassList)
            {
                ClassDTO ClassDTO = new()
                {
                    ClassID = Class.ClassID,
                    ClassName = Class.ClassName,
                    StageID = Class.StageID,
                    StageName = Class.Stage.StageName
                };
                foreach (var division in Class.Divisions)
                {
                    ClassDTO.Divisions.Add(new DivisionINClassDTO()
                    {
                        DivisionID = division.DivisionID,
                        DivisionName = division.DivisionName,
                        StudentCount = _studentRepository.GetCount(s => s.DivisionID == division.DivisionID).Result
                    });
                }
                ClassDTOList.Add(ClassDTO);
            }


            return ClassDTOList;
        }

        public async Task<bool> UpdatePartialAsync(int id, JsonPatchDocument<UpdateClassDTO> partialClass)
        {
            try
            {
                var Class = await _classesRepository.GetAsync(c => c.ClassID == id, tracked: false);
                if (Class == null)
                {
                    return false;
                }

                var ClassDTO = new UpdateClassDTO()
                {
                    ClassName = Class.ClassName,
                    State = Class.State
                };

                partialClass.ApplyTo(ClassDTO);

                Class.ClassName = ClassDTO.ClassName;
                Class.State = ClassDTO.State;
                await _classesRepository.UpdateAsync(Class);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
}
