using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;            // or your DbContext namespace
using Backend.DTOS.School.Subjects;
using Backend.Interfaces;
using Backend.Models;         // where your Subject entity lives
using Backend.Repository.School.Implements;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School.Classes
{
    public class SubjectRepository : ISubjectsRepository
    {
        private readonly DatabaseContext _context;
        private readonly IMapper _mapper;

        public SubjectRepository(DatabaseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // READ ALL
        public async Task<List<SubjectsDTO>> GetSubjectsPaginatedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                throw new ArgumentException("Page number and page size must be greater than zero.");

            var subjects = await _context.Subjects
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var subjectDtos = _mapper.Map<List<SubjectsDTO>>(subjects);
            return subjectDtos;
        }

        public async Task<int> GetTotalSubjectsCountAsync()
        {
            return await _context.Subjects.CountAsync();
        }


        public async Task<SubjectsDTO> GetSubjectByIdAsync(int id)
        {
            var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.SubjectID == id);
            if (subject == null)
                throw new ArgumentNullException(nameof(Subject), $"Subject with ID {id} not found.");

            // Map Entity → DTO
            var subjectDto = _mapper.Map<SubjectsDTO>(subject);
            return subjectDto;
        }

        // UPDATE
        public async Task UpdateSubjectAsync(SubjectsDTO subjectDto)
        {
            if (subjectDto == null)
                throw new ArgumentNullException(nameof(subjectDto), "Subject DTO cannot be null.");

            var subjectEntity = await _context.Subjects.FirstOrDefaultAsync(s => s.SubjectID == subjectDto.SubjectID);
            if (subjectEntity == null)
                throw new ArgumentNullException(nameof(Subject), $"Subject with ID {subjectDto.SubjectID} not found.");

            // Map DTO → Entity
            subjectEntity.SubjectName= subjectDto.SubjectName!;
            subjectEntity.SubjectReplacement = subjectDto.SubjectReplacement;
            subjectEntity.Note = subjectDto.Note;
            _context.Entry(subjectEntity).State = EntityState.Modified;

            // Save changes
            await _context.SaveChangesAsync();
        }
        // READ ALL
        public async Task<List<SubjectsNameDTO>> GetAllSubjectsAsync()
        {
            var subjects = await _context.Subjects.ToListAsync();
            var subjectDtos = _mapper.Map<List<SubjectsNameDTO>>(subjects);
            return subjectDtos;
        }
        // DELETE
        public async Task DeleteSubjectAsync(int id)
        {
            var subjectEntity = await _context.Subjects.FirstOrDefaultAsync(s => s.SubjectID == id);
            if (subjectEntity == null)
                throw new ArgumentNullException(nameof(Subject), $"Subject with ID {id} not found.");

            _context.Subjects.Remove(subjectEntity);
            await _context.SaveChangesAsync();
        }

      public async  Task<SubjectsDTO> AddSubjectAsync(SubjectsDTO subjectDto)
        {
             if (subjectDto == null)
                throw new ArgumentNullException(nameof(subjectDto), "Subject DTO cannot be null.");

            // Map DTO → Entity
            var subjectEntity = _mapper.Map<Subject>(subjectDto);

            await _context.Subjects.AddAsync(subjectEntity);
            subjectDto.SubjectID = subjectEntity.SubjectID;
            await _context.SaveChangesAsync();
            return subjectDto;
        }
    }
}
