using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS;
using Backend.DTOS.DivisionsDTO;
using Backend.Models;
using Backend.Repository.IRepository;
using Backend.Repository.School;
using Backend.Services.IServices;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class DivisionServices : IDivisionServices
    {

        private readonly IDivisionRepository _divisionRepository;
        private readonly IStudentRepository _studentRepository;
        public DivisionServices(IDivisionRepository divisionRepository, IStudentRepository studentRepository)
        {

            _divisionRepository = divisionRepository;
            _studentRepository = studentRepository;

        }
        public async Task<bool> AddAsync(AddDivisionDTO obj)
        {

            try
            {
                Division newDivision = new Division
                {
                    DivisionName = obj.DivisionName,
                    ClassID = obj.ClassID
                };
                await _divisionRepository.CreateAsync(newDivision);
                return true;
            }
            catch
            {
                return false;
            }
        }


        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var existingDivision = await _divisionRepository.GetAsync(s => s.DivisionID == id);
                if (existingDivision != null)
                {
                    await _divisionRepository.RemoveAsync(existingDivision);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }

        }

        public async Task<List<DivisionDTO>> GetAllAsync(Expression<Func<Division, bool>> filter = null)
        {
            var divisions = await _divisionRepository.GetAllAsync(filter, includeProperties: "Class");
            var divisionsDTO = new List<DivisionDTO>();
            foreach (var division in divisions)
            {
                divisionsDTO.Add(new DivisionDTO()
                {
                    DivisionID = division.DivisionID,
                    DivisionName = division.DivisionName,
                    ClassID = division.ClassID,
                    ClassesName = division.Class.ClassName,
                    StudentCount = _studentRepository.GetCount(s => s.DivisionID == division.DivisionID).Result

                });
            }
            return divisionsDTO;

        }



        public async Task<DivisionDTO> GetAsync(Expression<Func<Division, bool>> filter)
        {
            var division = await _divisionRepository.GetAsync(filter, includeProperties: "Class");
            if (division == null)
            {
                return null;
            }
            var divisionDTO = new DivisionDTO()
            {
                DivisionID = division.DivisionID,
                DivisionName = division.DivisionName,
                ClassID = division.ClassID,
                ClassesName = division.Class.ClassName,
                StudentCount = _studentRepository.GetCount(s => s.DivisionID == division.DivisionID).Result
            };
            return divisionDTO;


        }

        public async Task<bool> UpdateAsync(UpdateDivisionDTO model)
        {
            try
            {
                var existingDivision = await _divisionRepository.GetAsync(s => s.DivisionID == model.DivisionID, tracked: false);
                if (existingDivision != null)
                {
                    existingDivision.DivisionName = model.DivisionName;
                    existingDivision.State = model.State;

                    await _divisionRepository.UpdateAsync(existingDivision);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }


        }

        public async Task<bool> UpdatePartialAsync(int id, JsonPatchDocument<UpdateDivisionDTO> partialDivision)
        {
            try
            {
                var division = await _divisionRepository.GetAsync(s => s.DivisionID == id, tracked: false);
                if (division == null)
                    return false;

                var divisionDTO = new UpdateDivisionDTO()
                {
                    DivisionName = division.DivisionName,
                    State = division.State
                };

                partialDivision.ApplyTo(divisionDTO);

                division.DivisionName = divisionDTO.DivisionName;
                division.State = divisionDTO.State;

                await _divisionRepository.UpdateAsync(division);
                return true;
            }
            catch
            {
                return false;
            }

        }


    }
}
