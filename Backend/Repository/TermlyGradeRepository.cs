using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Common;
using Backend.Data;
using Backend.DTOS.School.TermlyGrade;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;

namespace Backend.Repository
{
    public class TermlyGradeRepository : ITermlyGradeRepository
    {
        private readonly TenantDbContext _context;
        private readonly IMapper _mapper;

        public TermlyGradeRepository(TenantDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Add a new TermlyGrade
        public async Task<Result<TermlyGradeDTO>> AddAsync(TermlyGradeDTO termlyGradeDTO)
        {
            if (termlyGradeDTO == null)
                return Result<TermlyGradeDTO>.Fail("TermlyGradeDTO cannot be null.");

            var entity = _mapper.Map<TermlyGrade>(termlyGradeDTO);
            _context.TermlyGrades.Add(entity);
            await _context.SaveChangesAsync();

            termlyGradeDTO.TermlyGradeID = entity.TermlyGradeID; // Set the ID back to DTO after adding to DB
            return Result<TermlyGradeDTO>.Success(termlyGradeDTO);
        }

        // Delete a TermlyGrade by ID
        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var grade = await _context.TermlyGrades.FindAsync(id);
            if (grade == null)
                return Result<bool>.Fail("TermlyGrade not found.");

            _context.TermlyGrades.Remove(grade);
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        // Update an existing TermlyGrade
        public async Task<Result<bool>> UpdateAsync(IEnumerable<TermlyGradeDTO> termlyGradeDTO)
        {
            var changed = false;
            foreach (var grade in termlyGradeDTO)
            {
                var existing = await _context.TermlyGrades.FirstOrDefaultAsync(g => g.TermlyGradeID == grade.TermlyGradeID);
                if (existing == null)
                    return Result<bool>.Fail("TermlyGrade not found.");
                existing.Grade = grade.Grade;
                existing.Note = grade.Note;
                existing.TermID = grade.TermID;
                existing.ClassID = grade.ClassID;
                existing.SubjectID = grade.SubjectID;
                existing.StudentID = grade.StudentID;
                changed = true;
            }
            if (!changed)
                return Result<bool>.Fail("Nothing to update.");
            try
            {
                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (DbUpdateException ex)
            {
                return Result<bool>.Fail($"DB error: {ex.Message}");
            }
        }
        // Get a TermlyGrade by ID
        public async Task<Result<TermlyGradeDTO>> GetByIdAsync(int id)
        {
            var termlyGrade = await _context.TermlyGrades
                .Include(g => g.Student)  // Include Student if necessary
                .Include(g => g.Subject)  // Include Subject if necessary
                .Include(g => g.Term)     // Include Term if necessary
                .FirstOrDefaultAsync(g => g.TermlyGradeID == id);

            if (termlyGrade == null)
                return Result<TermlyGradeDTO>.Fail("TermlyGrade not found.");

            var termlyGradeDto = _mapper.Map<TermlyGradeDTO>(termlyGrade);
            return Result<TermlyGradeDTO>.Success(termlyGradeDto);
        }

        public async Task<Result<List<TermlyGradesReturnDTO>>> GetAllAsync(int term, int yearId, int classId, int subjectId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1 || pageSize < 1)
                return Result<List<TermlyGradesReturnDTO>>.Fail("Page number must be greater than 0.");

            // Get the active year - ignore the yearId parameter from frontend
            var activeYear = await _context.Years
                .Where(y => y.Active == true)
                .OrderBy(y => y.YearID)
                .FirstOrDefaultAsync();

            if (activeYear == null)
                return Result<List<TermlyGradesReturnDTO>>.Fail("No active year found. Please activate a year before viewing termly grades.");

            var baseQuery = _context.TermlyGrades
                .Where(g => g.TermID == term && g.ClassID == classId && g.YearID == activeYear.YearID);

            if (subjectId != 0)
                baseQuery = baseQuery.Where(g => g.SubjectID == subjectId);

            var TermlyGrades = await baseQuery
                .Include(g => g.Student).ThenInclude(s => s.FullName)
                .Include(g => g.Subject)
                .ToListAsync();
            var grouped = TermlyGrades
                .GroupBy(g => new
                {
                    g.StudentID,
                    g.Student.FullName,
                    g.SubjectID,
                    g.Subject.SubjectName,
                    g.Student.ImageURL
                })
                .Select(grp => new TermlyGradesReturnDTO
                {
                    TermlyGradeID = grp.First().TermlyGradeID,
                    StudentID = grp.Key.StudentID,
                    StudentName = $"{grp.Key.FullName.FirstName} {grp.Key.FullName.MiddleName} {grp.Key.FullName.LastName}",
                    SubjectID = grp.Key.SubjectID,
                    Note = grp.First().Note,
                    SubjectName = grp.Key.SubjectName,
                    StudentURL = $"https://localhost:7258/uploads/StudentPhotos/{grp.Key.ImageURL}",
                    Grade = grp.Sum(g => g.Grade)
                })
                .ToList();
            var paginatedResult = grouped
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return Result<List<TermlyGradesReturnDTO>>.Success(paginatedResult);
        }

        public async Task<int> GetTotalMonthlyGradesCountAsync(int term, int yearId, int classId, int subjectId)
        {
            // Get the active year - ignore the yearId parameter from frontend
            var activeYear = await _context.Years
                .Where(y => y.Active == true)
                .OrderBy(y => y.YearID)
                .FirstOrDefaultAsync();

            if (activeYear == null)
                return 0;

            var query = _context.TermlyGrades
          .Where(g => g.TermID == term &&
                      g.YearID == activeYear.YearID &&
                      g.ClassID == classId);

            if (subjectId != 0)
                query = query.Where(g => g.SubjectID == subjectId);

            return await query
                .Select(g => g.StudentID)
                .Distinct()
                .CountAsync();
        }
    }

}