using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.DTOS.School.Teachers;
using Backend.Interfaces;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly TenantDbContext _context;
        private readonly IUserRepository _userRepository;

        public TeacherRepository(TenantDbContext context, IUserRepository userRepository)
        {
            _context = context;
            _userRepository = userRepository;
        }

        // Add a new Teacher
        public async Task<TeacherDTO> AddTeacherAsync(TeacherDTO teacher)
        {
            if (teacher == null)
                throw new ArgumentNullException(nameof(teacher), "Teacher DTO cannot be null.");

            // Create the User for Teacher using IUserRepository (works with DatabaseContext)
            var teacherUser = new ApplicationUser
            {
                UserName = teacher.UserName,
                Email = teacher.Email,
                Address = teacher.Address,
                Gender = teacher.Gender,
                HireDate = DateTime.Now,
                PhoneNumber = teacher.PhoneNumber,
                UserType = "TEACHER" // Ensure the UserType field exists in the ApplicationUser model
            };
            
            // Use IUserRepository to create user (this uses DatabaseContext, not TenantDbContext)
            var createdUser = await _userRepository.CreateUserAsync(teacherUser, "TEACHER", "TEACHER");
            teacher.UserID = createdUser.Id;

            // Create Teacher entity
            var newTeacher = new Teacher
            {
                FullName = new Name
                {
                    FirstName = teacher.FirstName,
                    MiddleName = teacher.MiddleName,
                    LastName = teacher.LastName
                },
                DOB = teacher.DOB,
                ImageURL = teacher.ImageURL,
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
                DOB = t.DOB,
                ImageURL = "https://localhost:7258/uploads/Teacher/"+t.ImageURL,
                Gender = t.ApplicationUser.Gender??"Male",
                Address = t.ApplicationUser.Address??"Yemen",
                Email = t.ApplicationUser.Email??"User@gmail.com",
                ManagerID = t.ManagerID
            }).ToList();
        }
    }
}
