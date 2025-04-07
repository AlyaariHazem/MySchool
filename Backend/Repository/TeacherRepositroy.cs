using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.DTOS.School.Teachers;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly DatabaseContext _context;

        public TeacherRepository(DatabaseContext context)
        {
            _context = context;
        }

        // Add a new Teacher
        public async Task<TeacherDTO> AddTeacherAsync(TeacherDTO teacher)
        {
            if (teacher == null)
                throw new ArgumentNullException(nameof(teacher), "Teacher DTO cannot be null.");

            // Create the User for Teacher
            var guardianUser = new ApplicationUser
            {
                UserName = "Teacher_" + Guid.NewGuid().ToString("N").Substring(0, 5),
                Email = teacher.Email,
                Address = teacher.Address,
                Gender = teacher.Gender,
                PhoneNumber = teacher.PhoneNumber,
                UserType = "Teacher" // Ensure the UserType field exists in the ApplicationUser model
            };

            // Add User to database
            _context.Users.Add(guardianUser);
            await _context.SaveChangesAsync(); // Use SaveChangesAsync() for async IO

            teacher.UserID = guardianUser.Id;

            // Create Teacher entity
            var newTeacher = new Teacher
            {
                FullName = new Name
                {
                    FirstName = teacher.FirstName,
                    MiddleName = teacher.MiddleName,
                    LastName = teacher.LastName
                },
                UserID = teacher.UserID,
                ManagerID = teacher.ManagerID
            };

            // Add Teacher to database
            _context.Teachers.Add(newTeacher);
            await _context.SaveChangesAsync(); // Use SaveChangesAsync() for async IO
            teacher.TeacherID = newTeacher.TeacherID;

            return teacher; // Return the DTO
        }

        // Get all Teachers
        public async Task<List<TeacherDTO>> GetAllTeachersAsync()
        {
            // Include the Manager data and map it to TeacherDTO
            var teachers = await _context.Teachers
                .Include(t => t.Manager)
                .Include(t => t.ApplicationUser) // Include manager info if applicable
                .ToListAsync();

            // Map the Teacher entity to TeacherDTO
            return teachers.Select(t => new TeacherDTO
            {
                TeacherID = t.TeacherID,
                FirstName = t.FullName.FirstName,
                MiddleName = t.FullName.MiddleName,
                LastName = t.FullName.LastName,
                PhoneNumber = t.ApplicationUser.PhoneNumber??"776137120",
                UserID = t.UserID,
                Gender = t.ApplicationUser.Gender??"Male",
                Address = t.ApplicationUser.Address??"Yemen",
                Email = t.ApplicationUser.Email??"User@gmail.com",
                ManagerID = t.ManagerID
            }).ToList();
        }
    }
}
