using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS.School;
using school = Backend.Models.School; // Alias for the model class
using Backend.Repository.School.Implements;
using Microsoft.EntityFrameworkCore;
using Backend.DTOS.School.Years;
using Backend.Repository.School.Interfaces;

namespace Backend.Repository.School.Classes
{
    public class SchoolRepository : ISchoolRepository
    {
        private readonly DatabaseContext _db;
        private readonly IMapper _mapper;
        private readonly IYearRepository _yearRepository;

        public SchoolRepository(DatabaseContext db, IYearRepository yearRepository, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
            _yearRepository = yearRepository;
        }

        public async Task<SchoolDTO> GetByIdAsync(int id)
        {
            var schools = await _db.Schools.FirstOrDefaultAsync(x => x.SchoolID == id);
            if (schools == null)
            {
                throw new KeyNotFoundException($"School with ID {id} not found.");
            }
            var schoolDTO = _mapper.Map<SchoolDTO>(schools);
            return schoolDTO;
        }

        public async Task<List<SchoolDTO>> GetAllAsync()
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
            var currentYear = new YearDTO
            {
                YearDateStart = DateTime.Now,
                YearDateEnd = DateTime.Now.AddYears(1),
                HireDate = DateTime.Now,
                Active = true,
                SchoolID = newSchool.SchoolID
            };
            await _yearRepository.Add(currentYear);
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
