using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS.School;
using school = Backend.Models.School; // Alias for the model class
using Backend.Repository.School.Implements;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School.Classes
{
    public class SchoolRepository : ISchoolRepository
    {
        private readonly DatabaseContext _db;
        private readonly IMapper _mapper;

        public SchoolRepository(DatabaseContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<SchoolDTO>> GetByIdAsync()
        {
            var schools = await _db.Schools.ToListAsync();
            var schoolDTO = _mapper.Map<List<SchoolDTO>>(schools);
            return schoolDTO;
        }

        public async Task AddAsync(SchoolDTO school)
        {
            var newSchool = _mapper.Map<school>(school);
            await _db.Schools.AddAsync(newSchool);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(SchoolDTO schoolDTO)
        {
            var existingSchool = await _db.Schools.FindAsync(schoolDTO.SchoolID);
            if (existingSchool == null)
            {
                throw new KeyNotFoundException($"School with ID {schoolDTO.SchoolID} not found.");
            }

            _mapper.Map(schoolDTO, existingSchool); // Update the entity with new values
            _db.Schools.Update(existingSchool);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int schoolId)
        {
            var schoolToDelete = await _db.Schools.FindAsync(schoolId);
            if (schoolToDelete == null)
            {
                throw new KeyNotFoundException($"School with ID {schoolId} not found.");
            }

            _db.Schools.Remove(schoolToDelete);
            await _db.SaveChangesAsync();
        }
    }
}
