using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Backend.Data;
using Backend.DTOS;
using Backend.DTOS.School.Classes;
using Backend.DTOS.School.Stages;
using Backend.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School
{
    public class ClassesRepository : IClassesRepository
    {
        private readonly TenantDbContext _db;
        private readonly IMapper _mapper;

        public ClassesRepository(TenantDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task Add(AddClassDTO obj)
        {
            // Get the active year - if multiple active years exist, take the first one
            var activeYear = await _db.Years
                .Where(y => y.Active == true)
                .OrderBy(y => y.YearID) // Order by YearID to ensure consistent selection
                .FirstOrDefaultAsync();

            if (activeYear == null)
                throw new InvalidOperationException("No active year found. Please activate a year before adding a class.");

            // Override the YearID from the DTO with the active year's ID
            obj.YearID = activeYear.YearID;

            var newClass = _mapper.Map<Class>(obj);

            try
            {
                var x = _db.Classes.Add(newClass);
              await  _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding class: {ex.Message}");
                throw;
            }
        }
        public async Task<List<AllClassesDTO>> GetAllNamesAsync()
        {
            var stageDictionary = await _db.Classes
                .Include(c => c.Year)
                .Include(c => c.Stage)
                    .ThenInclude(s => s.Year)
                .Where(c => (c.Year != null && c.Year.Active == true) || 
                           (c.Stage != null && c.Stage.Year != null && c.Stage.Year.Active == true))
                .Select(c => new AllClassesDTO
                {
                    ClassID = c.ClassID,//this classID is not return correctlly?
                    ClassName = c.ClassName,
                }).ToListAsync();
            return stageDictionary;
        }
        public async Task Update(AddClassDTO model)
        {
            var existingClass = await _db.Classes.FirstOrDefaultAsync(c => c.ClassID == model.ClassID);
            if (existingClass != null)
            {
                // Get the active year - if multiple active years exist, take the first one
                var activeYear = await _db.Years
                    .Where(y => y.Active == true)
                    .OrderBy(y => y.YearID) // Order by YearID to ensure consistent selection
                    .FirstOrDefaultAsync();

                if (activeYear == null)
                    throw new InvalidOperationException("No active year found. Please activate a year before updating a class.");

                // Map only the properties that need updating
                existingClass.ClassName = model.ClassName;
                existingClass.StageID = model.StageID;
                existingClass.YearID = activeYear.YearID; // Update to use the active year

                _db.Entry(existingClass).State = EntityState.Modified;
          await  _db.SaveChangesAsync();
            }
           
        }

        public async Task DeleteAsync(int id)
        {
            var existingClass = await GetByIdAsync(id);
            if (existingClass != null)
            {
                _db.Classes.Remove(existingClass);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<Class> GetByIdAsync(int id)
        {
            return await _db.Classes.FirstOrDefaultAsync(c => c.ClassID == id)?? null!;
        }


        public async Task<List<ClassDTO>> GetAllAsync()
        {
            var ClassList = await _db.Classes
                .Include(c => c.Year)
                .Include(c => c.Stage)
                    .ThenInclude(s => s.Year)
                .Include(d => d.Divisions)
                    .ThenInclude(s => s.Students)
                .Where(c => (c.Year != null && c.Year.Active == true) || 
                           (c.Stage != null && c.Stage.Year != null && c.Stage.Year.Active == true))
                .ToListAsync();

            var ClassDTOList = ClassList.Select(c => new ClassDTO
            {
                ClassID = c.ClassID,
                ClassName = c.ClassName,
                StageID = c.StageID,
                State = c.State,
                StageName = c.Stage != null ? c.Stage.StageName : string.Empty,
                StudentCount = c.Divisions.Sum(d => d.Students.Count()),
                Divisions = c.Divisions.Select(d => new DivisionINClassDTO
                {
                    DivisionID = d.DivisionID,
                    DivisionName = d.DivisionName,
                    StudentCount = d.Students.Count()
                }).ToList()
            }).ToList();
            return ClassDTOList;
        }

        public async Task<bool> UpdatePartial(int id, JsonPatchDocument<UpdateClassDTO> partialClass)
        {
            if (partialClass == null || id == 0)
                return false;

            // Retrieve the Class entity by its ID
            var Class = await _db.Classes.SingleOrDefaultAsync(s => s.ClassID == id);
            if (Class == null)
                return false;

            // Map the Class entity to the DTO (this will be modified)
            var classDTO = _mapper.Map<UpdateClassDTO>(Class);

            // Apply the patch to the DTO
            partialClass.ApplyTo(classDTO);

            // Map the patched DTO back to the entity (class)
            _mapper.Map(classDTO, Class);

            // Mark the entity as modified and save changes
            _db.Entry(Class).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
