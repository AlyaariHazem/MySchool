using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Backend.Data;
using Backend.DTOS;
using Backend.DTOS.School.Stages;
using Backend.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School
{
    public class ClassesRepository : IClassesRepository
    {
        private readonly DatabaseContext _db;
        private readonly IMapper _mapper;

        public ClassesRepository(DatabaseContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task Add(AddClassDTO obj)
        {
            var newClass = _mapper.Map<Class>(obj);

            try
            {
                var x = _db.Classes.Add(newClass);
              await  _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding class: {ex.Message}");
            }
        }

        public async Task Update(AddClassDTO model)
        {
            var existingClass = await _db.Classes.FirstOrDefaultAsync(c => c.ClassID == model.ClassID);
            if (existingClass != null)
            {
            // Map only the properties that need updating
            existingClass.ClassName = model.ClassName;
            existingClass.StageID = model.StageID;

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
            return await _db.Classes.FirstOrDefaultAsync(c => c.ClassID == id);
        }


        public async Task<List<ClassDTO>> GetAllAsync()
        {
            var ClassList = await _db.Classes
             .Include(c => c.Stage) 
            .Include(d => d.Divisions)
            .ThenInclude(s => s.Students).ToListAsync();

            var ClassDTOList = _mapper.Map<List<ClassDTO>>(ClassList);
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
