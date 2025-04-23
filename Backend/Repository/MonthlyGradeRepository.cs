using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS.School.GradeTypes;
using Backend.DTOS.School.MonthlyGrade;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository
{
    public class MonthlyGradeRepository : IMonthlyGradeRepository
    {
        private readonly DatabaseContext _context;
        private readonly IMapper _mapper;

        public MonthlyGradeRepository(DatabaseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Add a new monthly grade
        public async Task<MonthlyGradeDTO> AddAsync(MonthlyGradeDTO monthlyGradeDTO)
        {
            if (monthlyGradeDTO == null)
                throw new ArgumentNullException(nameof(monthlyGradeDTO), "MonthlyGradeDTO cannot be null.");

            var entity = _mapper.Map<MonthlyGrade>(monthlyGradeDTO);
            _context.MonthlyGrades.Add(entity);
            await _context.SaveChangesAsync();

            // monthlyGradeDTO.MonthlyGradeID = entity.MonthlyGradeID; // Set the ID back to DTO after adding to DB
            return monthlyGradeDTO;
        }

        // Delete a monthly grade by ID
        public async Task<bool> DeleteAsync(int id)
        {
            var grade = await _context.MonthlyGrades.FindAsync(id);
            if (grade == null)
                return false;

            _context.MonthlyGrades.Remove(grade);
            await _context.SaveChangesAsync();
            return true;
        }

        // Get all monthly grades based on filters: Term, Month, Class, GradeType
        public async Task<List<MonthlyGradesReternDTO>> GetAllAsync(int term, int monthId, int classId, int subjectId)
        {
            var query = _context.MonthlyGrades
                .Where(g => g.TermID == term && g.MonthID == monthId && g.ClassID == classId);

            if (subjectId != 0)
                query = query.Where(g => g.SubjectID == subjectId);

            var grades = await query
                .Include(g => g.Student)
                    .ThenInclude(s => s.FullName)
                .Include(g => g.Subject)
                .Include(g => g.GradeType)
                .ToListAsync();

            var studentGrades = grades
                .GroupBy(g => new { g.StudentID, g.Student.FullName, g.SubjectID, g.Subject.SubjectName })
                .Select(group => new MonthlyGradesReternDTO
                {
                    StudentID = group.Key.StudentID,
                    StudentName = $"{group.Key.FullName.FirstName} {group.Key.FullName.MiddleName} {group.Key.FullName.LastName}",
                    SubjectID = group.Key.SubjectID,
                    SubjectName = group.Key.SubjectName,
                    Grades = group.Select(g => new GradeTypeMonthDTO
                    {
                        GradeTypeID = g.GradeTypeID,
                        MaxGrade = g.Grade
                    }).ToList()
                })
                .ToList();

            return studentGrades;
        }


        public async Task<bool> UpdateManyAsync(IEnumerable<MonthlyGradeDTO> dtos)
        {
            foreach (var dto in dtos)
            {
                var grade = await _context.MonthlyGrades
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g =>
                        g.StudentID == dto.StudentID &&
                        g.YearID == dto.YearID &&
                        g.SubjectID == dto.SubjectID &&
                        g.MonthID == dto.MonthID &&
                        g.TermID == dto.TermID &&
                        g.ClassID == dto.ClassID &&
                        g.GradeTypeID == dto.GradeTypeID);

                if (grade != null && dto.Grade != 0 && grade.Grade != dto.Grade)
                {
                    grade.Grade = dto.Grade;
                    _context.Entry(grade).State = EntityState.Modified;
                }
            }

            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }


    }
}
