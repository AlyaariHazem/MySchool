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
        public async Task<List<MonthlyGradesReternDTO>> GetAllAsync(int term, int monthId, int classId)
        {
            var grades = await _context.MonthlyGrades
                .Where(g => g.TermID == term && g.MonthID == monthId && g.ClassID == classId)
                .Include(g => g.Student)  // Ensure Student is included in the query
                .Include(g => g.Subject)  // Include Subject if necessary
                .Include(g => g.GradeType) // Include GradeType if necessary
                .ToListAsync();

            // Group the grades by student
            var studentGrades = grades
                .GroupBy(g => g.StudentID) // Group by StudentID
                .Select(studentGroup => new MonthlyGradesReternDTO
                {
                    StudentID = studentGroup.Key,
                    StudentName = studentGroup.FirstOrDefault()?.Student.FullName.FirstName + " " +
                                  studentGroup.FirstOrDefault()?.Student.FullName.MiddleName + " " +
                                  studentGroup.FirstOrDefault()?.Student.FullName.LastName,
                    Grades = studentGroup.Select(g => new GradeTypeMonthDTO
                    {
                        Name = g.GradeType.Name,
                        MaxGrade = g.GradeType.MaxGrade
                    }).ToList()
                }).ToList();

            return studentGrades;
        }

        // Update an existing monthly grade
        public async Task<bool> UpdateAsync(MonthlyGradeDTO monthlyGradeDTO)
        {
            if (monthlyGradeDTO == null)
                throw new ArgumentNullException(nameof(monthlyGradeDTO), "MonthlyGradeDTO cannot be null.");

            var grade = await _context.MonthlyGrades.FindAsync(monthlyGradeDTO.MonthlyGradeID);
            if (grade == null)
                throw new KeyNotFoundException("Monthly grade not found.");

            grade.Grade = monthlyGradeDTO.Grade;
            grade.StudentID = monthlyGradeDTO.StudentID;
            grade.SubjectID = monthlyGradeDTO.SubjectID;
            grade.MonthID = monthlyGradeDTO.MonthID;
            grade.ClassID = monthlyGradeDTO.ClassID;
            grade.GradeTypeID = monthlyGradeDTO.GradeTypeID;

            _context.MonthlyGrades.Update(grade);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
