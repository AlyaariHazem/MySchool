using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Common;
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
            // ApplicationUser is in admin database, not tenant database, so we can't Include it
            var teachers = await _context.Teachers
                .Include(t => t.Manager)
                .ToListAsync();

            // Fetch user data from admin database separately
            var userIds = teachers.Select(t => t.UserID).Distinct().ToList();
            var users = new Dictionary<string, ApplicationUser>();
            foreach (var userId in userIds)
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await _userRepository.GetUserByIdAsync(userId);
                    if (user != null)
                    {
                        users[userId] = user;
                    }
                }
            }

            // Map the Teacher entity to TeacherDTO
            return teachers.Select(t =>
            {
                var user = users.GetValueOrDefault(t.UserID);
                return new TeacherDTO
                {
                    TeacherID = t.TeacherID,
                    FirstName = t.FullName?.FirstName ?? string.Empty,
                    MiddleName = t.FullName?.MiddleName,
                    LastName = t.FullName?.LastName ?? string.Empty,
                    PhoneNumber = user?.PhoneNumber ?? string.Empty,
                    UserID = t.UserID,
                    UserName = user?.UserName,
                    DOB = t.DOB,
                    ImageURL = string.IsNullOrEmpty(t.ImageURL) 
                        ? null 
                        : "https://localhost:7258/uploads/Teacher/" + t.ImageURL,
                    Gender = user?.Gender ?? "Male",
                    Address = user?.Address,
                    Email = user?.Email ?? string.Empty,
                    ManagerID = t.ManagerID
                };
            }).ToList();
        }

        // Get Teachers with pagination
        public async Task<(List<TeacherDTO> Items, int TotalCount)> GetTeachersPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            // Base query
            var baseQuery = _context.Teachers
                .Include(t => t.Manager)
                .AsQueryable();

            // Get total count
            var totalCount = await baseQuery.CountAsync(cancellationToken);

            if (totalCount == 0)
                return (new List<TeacherDTO>(), 0);

            // Apply pagination
            var teachers = await baseQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            if (teachers == null || !teachers.Any())
                return (new List<TeacherDTO>(), totalCount);

            // Fetch user data from admin database separately
            var userIds = teachers.Select(t => t.UserID).Distinct().ToList();
            var users = new Dictionary<string, ApplicationUser>();
            foreach (var userId in userIds)
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await _userRepository.GetUserByIdAsync(userId);
                    if (user != null)
                    {
                        users[userId] = user;
                    }
                }
            }

            // Map the Teacher entity to TeacherDTO
            var items = teachers.Select(t =>
            {
                var user = users.GetValueOrDefault(t.UserID);
                return new TeacherDTO
                {
                    TeacherID = t.TeacherID,
                    FirstName = t.FullName?.FirstName ?? string.Empty,
                    MiddleName = t.FullName?.MiddleName,
                    LastName = t.FullName?.LastName ?? string.Empty,
                    PhoneNumber = user?.PhoneNumber ?? string.Empty,
                    UserID = t.UserID,
                    UserName = user?.UserName,
                    DOB = t.DOB,
                    ImageURL = string.IsNullOrEmpty(t.ImageURL) 
                        ? null 
                        : "https://localhost:7258/uploads/Teacher/" + t.ImageURL,
                    Gender = user?.Gender ?? "Male",
                    Address = user?.Address,
                    Email = user?.Email ?? string.Empty,
                    ManagerID = t.ManagerID
                };
            }).ToList();

            return (items, totalCount);
        }

        // Get Teachers with pagination and filters
        public async Task<(List<TeacherDTO> Items, int TotalCount)> GetTeachersPageWithFiltersAsync(int pageNumber, int pageSize, Dictionary<string, FilterValue> filters, CancellationToken cancellationToken = default)
        {
            // Start with base query
            var query = _context.Teachers
                .Include(t => t.Manager)
                .AsQueryable();

            // Apply filters dynamically
            foreach (var filter in filters)
            {
                var columnName = filter.Key;
                var filterValue = filter.Value;

                query = columnName.ToLower() switch
                {
                    "userid" or "userId" => !string.IsNullOrEmpty(filterValue.Value)
                        ? query.Where(t => t.UserID == filterValue.Value)
                        : query,
                    "teacherid" or "teacherId" => filterValue.IntValue.HasValue 
                        ? query.Where(t => t.TeacherID == filterValue.IntValue.Value)
                        : query,
                    "managerid" or "managerId" => filterValue.IntValue.HasValue
                        ? query.Where(t => t.ManagerID == filterValue.IntValue.Value)
                        : query,
                    "firstname" or "firstName" => !string.IsNullOrEmpty(filterValue.Value)
                        ? query.Where(t => t.FullName != null && t.FullName.FirstName.Contains(filterValue.Value))
                        : query,
                    "lastname" or "lastName" => !string.IsNullOrEmpty(filterValue.Value)
                        ? query.Where(t => t.FullName != null && t.FullName.LastName.Contains(filterValue.Value))
                        : query,
                    "middlename" or "middleName" => !string.IsNullOrEmpty(filterValue.Value)
                        ? query.Where(t => t.FullName != null && t.FullName.MiddleName != null && t.FullName.MiddleName.Contains(filterValue.Value))
                        : query,
                    "dob" => filterValue.DateValue.HasValue
                        ? query.Where(t => t.DOB.HasValue && t.DOB.Value.Date == filterValue.DateValue.Value.Date)
                        : query,
                    "email" => !string.IsNullOrEmpty(filterValue.Value)
                        ? query.Where(t => t.UserID != null) // We'll filter by email after fetching users
                        : query,
                    _ => query // Unknown filter, ignore it
                };
            }

            // Check if email filter is present (requires special handling since email is in separate database)
            var hasEmailFilter = filters.TryGetValue("email", out var emailFilterValue) && !string.IsNullOrEmpty(emailFilterValue.Value);
            
            List<Teacher> teachers;
            int totalCount;
            Dictionary<string, ApplicationUser> users = new Dictionary<string, ApplicationUser>();

            if (hasEmailFilter)
            {
                // For email filter, we need to fetch all teachers first, then filter by email, then paginate
                // This is because email is in the ApplicationUser table (admin database), not Teacher table
                var allTeachers = await query.ToListAsync(cancellationToken);
                
                // Fetch user data for all teachers
                var userIds = allTeachers.Select(t => t.UserID).Distinct().ToList();
                foreach (var userId in userIds)
                {
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var user = await _userRepository.GetUserByIdAsync(userId);
                        if (user != null)
                        {
                            users[userId] = user;
                        }
                    }
                }

                // Filter by email
                var emailFilter = emailFilterValue?.Value?.ToLower() ?? string.Empty;
                var filteredTeachers = allTeachers.Where(t => 
                {
                    var user = users.GetValueOrDefault(t.UserID);
                    return user != null && !string.IsNullOrEmpty(user.Email) && user.Email.ToLower().Contains(emailFilter);
                }).ToList();

                totalCount = filteredTeachers.Count;
                
                if (totalCount == 0)
                    return (new List<TeacherDTO>(), 0);

                // Apply pagination after email filtering
                teachers = filteredTeachers
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }
            else
            {
                // Get total count with filters applied
                totalCount = await query.CountAsync(cancellationToken);

                if (totalCount == 0)
                    return (new List<TeacherDTO>(), 0);

                // Apply pagination
                teachers = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                // Fetch user data from admin database separately
                var userIds = teachers.Select(t => t.UserID).Distinct().ToList();
                foreach (var userId in userIds)
                {
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var user = await _userRepository.GetUserByIdAsync(userId);
                        if (user != null)
                        {
                            users[userId] = user;
                        }
                    }
                }
            }

            if (teachers == null || !teachers.Any())
                return (new List<TeacherDTO>(), totalCount);

            // Map the Teacher entity to TeacherDTO
            var items = teachers.Select(t =>
            {
                var user = users.GetValueOrDefault(t.UserID);
                return new TeacherDTO
                {
                    TeacherID = t.TeacherID,
                    FirstName = t.FullName?.FirstName ?? string.Empty,
                    MiddleName = t.FullName?.MiddleName,
                    LastName = t.FullName?.LastName ?? string.Empty,
                    PhoneNumber = user?.PhoneNumber ?? string.Empty,
                    UserID = t.UserID,
                    UserName = user?.UserName,
                    DOB = t.DOB,
                    ImageURL = string.IsNullOrEmpty(t.ImageURL) 
                        ? null 
                        : "https://localhost:7258/uploads/Teacher/" + t.ImageURL,
                    Gender = user?.Gender ?? "Male",
                    Address = user?.Address,
                    Email = user?.Email ?? string.Empty,
                    ManagerID = t.ManagerID
                };
            }).ToList();

            return (items, totalCount);
        }

        // Update Teacher
        public async Task<TeacherDTO> UpdateTeacherAsync(int id, TeacherDTO teacher)
        {
            if (teacher == null)
                throw new ArgumentNullException(nameof(teacher), "Teacher DTO cannot be null.");

            // Get the existing teacher from tenant database
            // Note: FullName is an owned entity, so it's automatically included - no need to .Include() it
            var existingTeacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.TeacherID == id);

            if (existingTeacher == null)
                throw new KeyNotFoundException($"Teacher with ID {id} not found.");

            // Update Teacher entity in tenant database
            if (existingTeacher.FullName == null)
            {
                existingTeacher.FullName = new Name();
            }

            existingTeacher.FullName.FirstName = teacher.FirstName;
            existingTeacher.FullName.MiddleName = teacher.MiddleName;
            existingTeacher.FullName.LastName = teacher.LastName;
            existingTeacher.DOB = teacher.DOB;
            existingTeacher.ImageURL = teacher.ImageURL;
            existingTeacher.ManagerID = teacher.ManagerID;

            // Update Teacher in tenant database
            _context.Entry(existingTeacher).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Update ApplicationUser in admin database using IUserRepository
            if (!string.IsNullOrEmpty(existingTeacher.UserID))
            {
                var existingUser = await _userRepository.GetUserByIdAsync(existingTeacher.UserID);
                if (existingUser != null)
                {
                    existingUser.Email = teacher.Email;
                    existingUser.PhoneNumber = teacher.PhoneNumber;
                    existingUser.Address = teacher.Address;
                    existingUser.Gender = teacher.Gender ?? "Male";
                    existingUser.UserName = teacher.UserName ?? existingUser.UserName;
                    
                    await _userRepository.UpdateAsync(existingUser);
                }
            }

            // Return updated DTO
            teacher.TeacherID = existingTeacher.TeacherID;
            teacher.UserID = existingTeacher.UserID;
            return teacher;
        }
    }
}
