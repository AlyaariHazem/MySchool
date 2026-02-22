using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS.School.Curriculms;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School
{
    public class CurriculumRepository : ICurriculumRepository
    {
        private readonly TenantDbContext _context;
        private readonly IMapper _mapper;

        public CurriculumRepository(TenantDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Get all curricula for active year only
        public async Task<List<CurriculumReturnDTO>> GetAllAsync()
        {
            // Get active year
            var activeYear = await _context.Years
                .Where(y => y.Active)
                .FirstOrDefaultAsync();

            if (activeYear == null)
            {
                // If no active year, return empty list
                return new List<CurriculumReturnDTO>();
            }

            var list = await _context.Curriculums
                .Include(c => c.Subject)
                .Include(c => c.Class)
                    .ThenInclude(cl => cl.Year)
                .Include(c => c.Class)
                    .ThenInclude(cl => cl.Stage)
                        .ThenInclude(s => s.Year)
                .Where(c => c.Class != null &&
                           ((c.Class.Year != null && c.Class.Year.YearID == activeYear.YearID) ||
                            (c.Class.Stage != null && c.Class.Stage.Year != null && c.Class.Stage.Year.YearID == activeYear.YearID)))
                .ToListAsync();

            return _mapper.Map<List<CurriculumReturnDTO>>(list);
        }

        // Get curriculum by SubjectID and ClassID
        public async Task<CurriculumReturnDTO?> GetByIdAsync(int subjectId, int classId)
        {
            var entity = await _context.Curriculums
                .Include(c => c.Subject)
                .Include(c => c.Class)
                .FirstOrDefaultAsync(c => c.SubjectID == subjectId && c.ClassID == classId);

            return entity == null ? null : _mapper.Map<CurriculumReturnDTO>(entity);
        }

        // Add a new curriculum
        public async Task<Boolean> AddAsync(CurriculumDTO dto)
        {
            try{
                var entity = _mapper.Map<Curriculum>(dto);
            _context.Curriculums.Add(entity);
            await _context.SaveChangesAsync();
            return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding curriculum: {ex.Message}");
                return false;
            }

        }

        // Update existing curriculum
        public async Task UpdateAsync(CurriculumDTO dto)
        {
            var entity = await _context.Curriculums
                .FirstOrDefaultAsync(c => c.SubjectID == dto.SubjectID && c.ClassID == dto.ClassID);

            if (entity == null)
                throw new KeyNotFoundException("Curriculum not found.");

            _mapper.Map(dto, entity);
            _context.Curriculums.Update(entity);
            await _context.SaveChangesAsync();
        }

        // Delete curriculum
        public async Task DeleteAsync(int subjectId, int classId)
        {
            var entity = await _context.Curriculums
                .FirstOrDefaultAsync(c => c.SubjectID == subjectId && c.ClassID == classId);

            if (entity == null)
                throw new KeyNotFoundException("Curriculum not found.");

            _context.Curriculums.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
