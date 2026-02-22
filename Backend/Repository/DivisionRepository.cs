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
        private readonly TenantDbContext _db;
        private readonly IMapper _mapper;
        public DivisionRepository(TenantDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;

        }
        public async Task<bool> Add(AddDivisionDTO obj)
        {
            // Get the active year - if multiple active years exist, take the first one
            var activeYear = await _db.Years
                .Where(y => y.Active == true)
                .OrderBy(y => y.YearID) // Order by YearID to ensure consistent selection
                .FirstOrDefaultAsync();

            if (activeYear == null)
                throw new InvalidOperationException("No active year found. Please activate a year before adding a division.");

            // Verify that the Class belongs to the active year
            var classEntity = await _db.Classes
                .Include(c => c.Year)
                .Include(c => c.Stage)
                    .ThenInclude(s => s.Year)
                .FirstOrDefaultAsync(c => c.ClassID == obj.ClassID);

            if (classEntity == null)
                throw new InvalidOperationException($"Class with ID {obj.ClassID} not found.");

            // Check if the class belongs to the active year (either directly or through its stage)
            bool belongsToActiveYear = (classEntity.Year != null && classEntity.Year.YearID == activeYear.YearID) ||
                                      (classEntity.Stage != null && classEntity.Stage.Year != null && classEntity.Stage.Year.YearID == activeYear.YearID);

            if (!belongsToActiveYear)
                throw new InvalidOperationException($"Class with ID {obj.ClassID} does not belong to the active year. Please select a class from the active year.");

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
                Console.WriteLine($"Error adding division: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                throw;
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
                    .ThenInclude(c => c.Year)
                .Include(d => d.Class)
                    .ThenInclude(c => c.Stage)
                        .ThenInclude(s => s.Year)
                .Where(d => d.Class != null && 
                           ((d.Class.Year != null && d.Class.Year.Active == true) || 
                            (d.Class.Stage != null && d.Class.Stage.Year != null && d.Class.Stage.Year.Active == true)))
                .Select(d => new DivisionDTO
                {
                    DivisionID = d.DivisionID,
                    DivisionName = d.DivisionName,
                    ClassID = d.ClassID,
                    ClassesName = d.Class != null ? d.Class.ClassName : string.Empty,  // Access the class name
                    StageName = d.Class != null && d.Class.Stage != null ? d.Class.Stage.StageName : string.Empty,  // Access the stage name
                    StudentCount = d.Students != null ? d.Students.Count() : 0,
                    State = d.State,
                }).ToListAsync();

            return divisions;
        }

        public async Task<List<DivisionDTO>> GetAllByYearID(int? yearID = null)
        {
            var query = _db.Divisions
                .Include(d => d.Class)
                    .ThenInclude(c => c.Year)
                .Include(d => d.Class)
                    .ThenInclude(c => c.Stage)
                        .ThenInclude(s => s.Year)
                .Where(d => d.Class != null);

            // If yearID is provided, filter by that year; otherwise, use active year
            if (yearID.HasValue)
            {
                query = query.Where(d => 
                    (d.Class.Year != null && d.Class.Year.YearID == yearID.Value) ||
                    (d.Class.Stage != null && d.Class.Stage.Year != null && d.Class.Stage.Year.YearID == yearID.Value));
                
                // If no divisions found for target year, fallback to active year divisions
                var divisionsForTargetYear = await query
                    .Select(d => new DivisionDTO
                    {
                        DivisionID = d.DivisionID,
                        DivisionName = string.IsNullOrWhiteSpace(d.DivisionName) ? $"قسم {d.DivisionID}" : d.DivisionName, // Use ID as fallback if name is empty
                        ClassID = d.ClassID,
                        ClassesName = d.Class != null ? d.Class.ClassName : string.Empty,
                        StageName = d.Class != null && d.Class.Stage != null ? d.Class.Stage.StageName : string.Empty,
                        StudentCount = d.Students != null ? d.Students.Count() : 0,
                        State = d.State,
                    }).ToListAsync();

                // If no divisions found for target year, return active year divisions as fallback
                if (divisionsForTargetYear.Count == 0)
                {
                    var activeYearQuery = _db.Divisions
                        .Include(d => d.Class)
                            .ThenInclude(c => c.Year)
                        .Include(d => d.Class)
                            .ThenInclude(c => c.Stage)
                                .ThenInclude(s => s.Year)
                        .Where(d => d.Class != null &&
                                   ((d.Class.Year != null && d.Class.Year.Active == true) ||
                                    (d.Class.Stage != null && d.Class.Stage.Year != null && d.Class.Stage.Year.Active == true)));

                    return await activeYearQuery
                        .Select(d => new DivisionDTO
                        {
                            DivisionID = d.DivisionID,
                            DivisionName = string.IsNullOrWhiteSpace(d.DivisionName) ? $"قسم {d.DivisionID}" : d.DivisionName, // Use ID as fallback if name is empty
                            ClassID = d.ClassID,
                            ClassesName = d.Class != null ? d.Class.ClassName : string.Empty,
                            StageName = d.Class != null && d.Class.Stage != null ? d.Class.Stage.StageName : string.Empty,
                            StudentCount = d.Students != null ? d.Students.Count() : 0,
                            State = d.State,
                        }).ToListAsync();
                }

                return divisionsForTargetYear;
            }
            else
            {
                // Default to active year (same as GetAll())
                query = query.Where(d => 
                    (d.Class.Year != null && d.Class.Year.Active == true) ||
                    (d.Class.Stage != null && d.Class.Stage.Year != null && d.Class.Stage.Year.Active == true));
            }

            var divisions = await query
                .Select(d => new DivisionDTO
                {
                    DivisionID = d.DivisionID,
                    DivisionName = d.DivisionName,
                    ClassID = d.ClassID,
                    ClassesName = d.Class != null ? d.Class.ClassName : string.Empty,
                    StageName = d.Class != null && d.Class.Stage != null ? d.Class.Stage.StageName : string.Empty,
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
                // Get the active year - if multiple active years exist, take the first one
                var activeYear = await _db.Years
                    .Where(y => y.Active == true)
                    .OrderBy(y => y.YearID) // Order by YearID to ensure consistent selection
                    .FirstOrDefaultAsync();

                if (activeYear == null)
                    throw new InvalidOperationException("No active year found. Please activate a year before updating a division.");

                // Verify that the Class belongs to the active year (only if ClassID is being changed)
                if (existingDivision.ClassID != model.ClassID)
                {
                    var classEntity = await _db.Classes
                        .Include(c => c.Year)
                        .Include(c => c.Stage)
                            .ThenInclude(s => s.Year)
                        .FirstOrDefaultAsync(c => c.ClassID == model.ClassID);

                    if (classEntity == null)
                        throw new InvalidOperationException($"Class with ID {model.ClassID} not found.");

                    // Check if the class belongs to the active year (either directly or through its stage)
                    bool belongsToActiveYear = (classEntity.Year != null && classEntity.Year.YearID == activeYear.YearID) ||
                                              (classEntity.Stage != null && classEntity.Stage.Year != null && classEntity.Stage.Year.YearID == activeYear.YearID);

                    if (!belongsToActiveYear)
                        throw new InvalidOperationException($"Class with ID {model.ClassID} does not belong to the active year. Please select a class from the active year.");
                }

                existingDivision.DivisionName = model.DivisionName;
                existingDivision.ClassID = model.ClassID;

                await _db.SaveChangesAsync();
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
