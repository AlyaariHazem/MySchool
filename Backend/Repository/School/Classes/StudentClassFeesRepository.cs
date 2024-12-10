using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School.Classes
{
    public class StudentClassFeesRepository : IStudentClassFeesRepository
    {
        private readonly DatabaseContext _db;

        public StudentClassFeesRepository(DatabaseContext db)
        {
            _db = db;
        }

        // Add fees for a student based on a class
        public async Task AddStudentFeesAsync(List<StudentClassFees> studentClassFees)
        {
            await _db.StudentClassFees.AddRangeAsync(studentClassFees);
            await _db.SaveChangesAsync();
        }

        // Optional: Fetch all fees for a student
        public async Task<List<FeeClass>> GetFeesForClassAsync(int classId)
        {
            return await _db.FeeClass
                .Where(fc => fc.ClassID == classId)
                .Include(fc => fc.Fee) // Include Fee details if needed
                .ToListAsync();
        }

    }
}
