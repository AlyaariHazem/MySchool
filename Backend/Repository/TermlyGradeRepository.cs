using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS.School.TermlyGrade;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository
{
    public class TermlyGradeRepository : ITermlyGradeRepository
    {
        private readonly DatabaseContext _context;
        private readonly IMapper _mapper;

        public TermlyGradeRepository(DatabaseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Add a new TermlyGrade
        public async Task<TermlyGradeDTO> AddAsync(TermlyGradeDTO termlyGradeDTO)
        {
            if (termlyGradeDTO == null)
                throw new ArgumentNullException(nameof(termlyGradeDTO), "TermlyGradeDTO cannot be null.");

            var entity = _mapper.Map<TermlyGrade>(termlyGradeDTO);
            _context.TermlyGrades.Add(entity);
            await _context.SaveChangesAsync();

            termlyGradeDTO.TermlyGradeID = entity.TermlyGradeID; // Set the ID back to DTO after adding to DB
            return termlyGradeDTO;
        }

        // Delete a TermlyGrade by ID
        public async Task<bool> DeleteAsync(int id)
        {
            var grade = await _context.TermlyGrades.FindAsync(id);
            if (grade == null)
                return false;

            _context.TermlyGrades.Remove(grade);
            await _context.SaveChangesAsync();
            return true;
        }

        // Get all TermlyGrades based on filters: Term, Class, and Subject
        public async Task<List<TermlyGradeDTO>> GetAllAsync(int termId, int classId)
        {
            var termlyGrades = await _context.TermlyGrades
                .Where(g => g.TermID == termId && g.ClassID == classId)
                .Include(g => g.Student)  // Ensure Student is included in the query
                .Include(g => g.Subject)  // Include Subject if necessary
                .Include(g => g.Term)     // Include Term if necessary
                .ToListAsync();

            // Map to DTO
            var termlyGradeDtos = _mapper.Map<List<TermlyGradeDTO>>(termlyGrades);

            return termlyGradeDtos;
        }

        // Update an existing TermlyGrade
        public async Task<bool> UpdateAsync(TermlyGradeDTO termlyGradeDTO)
        {
            if (termlyGradeDTO == null)
                throw new ArgumentNullException(nameof(termlyGradeDTO), "TermlyGradeDTO cannot be null.");

            var grade = await _context.TermlyGrades.FindAsync(termlyGradeDTO.TermlyGradeID);
            if (grade == null)
                throw new KeyNotFoundException("Termly grade not found.");

            grade.Grade = termlyGradeDTO.Grade;
            grade.StudentID = termlyGradeDTO.StudentID;
            grade.SubjectID = termlyGradeDTO.SubjectID;
            grade.TermID = termlyGradeDTO.TermID;
            grade.ClassID = termlyGradeDTO.ClassID;
            grade.Note = termlyGradeDTO.Note;

            _context.TermlyGrades.Update(grade);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
