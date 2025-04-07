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
        private readonly DatabaseContext _context;
        private readonly IMapper _mapper;

        public CurriculumRepository(DatabaseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Get all curricula
        public async Task<List<CurriculumReturnDTO>> GetAllAsync()
        {
            var list = await _context.Curriculums
                .Include(c => c.Subject)
                .Include(c => c.Class)
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
        public async Task<CurriculumReturnDTO> AddAsync(CurriculumDTO dto)
        {
            var entity = _mapper.Map<Curriculum>(dto);
            _context.Curriculums.Add(entity);
            await _context.SaveChangesAsync();

            // Create the CurriculumReturnDTO to return
            var curriculumReturn = new CurriculumReturnDTO
            {
                SubjectName = entity.Subject.SubjectName, // Ensure SubjectName exists in your model
                CurriculumName = entity.CurriculumName,
                ClassName = entity.Class.ClassName, // Ensure ClassName exists in your model
                Not = entity.Not,
                HireDate = entity.HireDate
            };

            return curriculumReturn;
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
