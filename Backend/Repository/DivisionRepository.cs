using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS;
using Backend.DTOS.School.Stages;
using Backend.Models;
using Backend.Repository.School;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace FirstProjectWithMVC.Repository.School
{
    public class DivisionRepository : IDivisionRepository
    {
        private readonly DatabaseContext _db;
        private readonly IMapper _mapper;
        public DivisionRepository(DatabaseContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;

        }
        public async Task<bool> Add(AddDivisionDTO obj)
        {
            Division newDivision = new Division
            {
                DivisionName = obj.DivisionName,
                ClassID = obj.ClassID,
                State = true,
            };

            try
            {
               await _db.Divisions.AddAsync(newDivision);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception (you can use any logging library here)
                Console.WriteLine($"Error adding class: {ex.Message}");
                throw; // Re-throw or handle as needed
            }
        }


        public async Task DeleteAsync(int id)
        {
            var existingDivision = await GetByIdAsync(id);
            if (existingDivision != null)
            {
                _db.Divisions.Remove(existingDivision);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<List<DivisionDTO>> GetAll()
        {
            var divisions = await _db.Divisions
                .Include(d => d.Class)
                .Select(d => new DivisionDTO
                {
                    DivisionID = d.DivisionID,
                    DivisionName = d.DivisionName,
                    ClassID = d.ClassID,
                    ClassesName = d.Class.ClassName,  // Access the class name
                    StudentCount = d.Students != null ? d.Students.Count() : 0,
                    State = d.State,
                }).ToListAsync(); 

            return divisions;
        }



        public async Task<Division> GetByIdAsync(int id)
        {

            return await _db.Divisions.FirstOrDefaultAsync(d => d.DivisionID == id)!;
        }

        public async Task<bool> Update(DivisionDTO model)
        {
            var existingDivision = await _db.Divisions.FirstOrDefaultAsync(d => d.DivisionID == model.DivisionID);
            if (existingDivision != null)
            {
                existingDivision.DivisionName = model.DivisionName;
                existingDivision.ClassID = model.ClassID;

              await  _db.SaveChangesAsync();
              return true;
            }
            return false;
        }

        public async Task<bool> UpdatePartial(int id, JsonPatchDocument<UpdateDivisionDTO> partialDivision)
        {
               if (partialDivision == null || id == 0)
                return false;

            // Retrieve the Class entity by its ID
            var division = await _db.Divisions.SingleOrDefaultAsync(s => s.DivisionID == id);
            if (division == null)
                return false;

            // Map the Class entity to the DTO (this will be modified)
            var divisionDTO = _mapper.Map<UpdateDivisionDTO>(division);

            // Apply the patch to the DTO
            partialDivision.ApplyTo(divisionDTO);

            // Map the patched DTO back to the entity (class)
            _mapper.Map(divisionDTO, division);

            // Mark the entity as modified and save changes
            _db.Entry(division).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return true;
        }
    

        }
    }
