using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.DTOS.School.Employee;
using Backend.Interfaces;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly TenantDbContext _context;
    private readonly IUserRepository _userRepository;
    public EmployeeRepository(TenantDbContext context, IUserRepository userRepository)
    {
        _context = context;
        _userRepository = userRepository;
    }
    public async Task<string> AddEmployeeAsync(EmployeeDTO employee)
    {
        if (employee.JopName == "Teacher")
        {
            var userNameTeacher = "Teach_" + Guid.NewGuid().ToString("N").Substring(0, 5);
            // Create the User for Teacher
            var teacherUser = new ApplicationUser
            {

                UserName = userNameTeacher,
                Email = employee.Email,
                Address = employee.Address,
                Gender = employee.Gender,
                HireDate = DateTime.Now,
                PhoneNumber = employee.Mobile,
                UserType = "TEACHER" // Ensure the UserType field exists in the ApplicationUser model
            };
            var createdUser = await _userRepository.CreateUserAsync(teacherUser, "TEACHER", "TEACHER");
            var teacher = new Teacher()
            {
                FullName = new Name()
                {
                    FirstName = employee.FirstName,
                    LastName = employee.LastName
                },
                DOB = employee.DOB ?? DateTime.Now,
                ImageURL = employee.ImageURL,
                UserID = createdUser.Id,
                ManagerID = employee.ManagerID ?? 1
            };
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();
            return "Teacher added successfully";
        }
        else if (employee.JopName == "Manager")
        {
            var userNameManager = "Manage_" + Guid.NewGuid().ToString("N").Substring(0, 5);
            var managerUser = new ApplicationUser
            {
                UserName = userNameManager,
                Email = employee.Email,
                Address = employee.Address,
                Gender = employee.Gender,
                HireDate = DateTime.Now,
                PhoneNumber = employee.Mobile,
                UserType = "MANAGER" // Ensure the UserType field exists in the ApplicationUser model
            };
            var createdUser = await _userRepository.CreateUserAsync(managerUser, "TEACHER", "TEACHER");
            await _context.SaveChangesAsync();
            var manager = new Manager()
            {
                FullName = new Name()
                {
                    FirstName = employee.FirstName,
                    LastName = employee.LastName
                },
                DOB = employee.DOB ?? DateTime.Now,
                ImageURL = employee.ImageURL,
                UserID = createdUser.Id
            };
            _context.Managers.Add(manager);
            await _context.SaveChangesAsync();
            return "Manager added successfully";
        }
        else
        {
            throw new NotImplementedException("Job type not implemented");
        }
    }

    public async Task DeleteEmployeeAsync(int employeeId, string jopName)
    {
        if (jopName == "Teacher")
        {
            var teacher = _context.Teachers.FirstOrDefault(t => t.TeacherID == employeeId);
            if (teacher != null)
            {
                // Use IUserRepository to delete user (works with DatabaseContext)
                await _userRepository.DeleteAsync(teacher.UserID);
                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();
            }
        }
        else if (jopName == "Manager")
        {
            var manager = _context.Managers.FirstOrDefault(m => m.ManagerID == employeeId);
            if (manager != null)
            {
                // Use IUserRepository to delete user (works with DatabaseContext)
                await _userRepository.DeleteAsync(manager.UserID);
                _context.Managers.Remove(manager);
                await _context.SaveChangesAsync();
            }
        }
    }

    public async Task<List<EmployeeDTO>> GetAllEmployeesAsync()
    {
        try
        {
            var employees = await _context.Teachers
           .Include(u => u.ApplicationUser)
           .Select(u => new EmployeeDTO()
           {
               EmployeeID = u.TeacherID,
               FirstName = u.FullName.FirstName,
               MiddleName = u.FullName.MiddleName,
               LastName = u.FullName.LastName,
               JopName = "Teacher",
               Address = u.ApplicationUser.Address,
               Gender = u.ApplicationUser.Gender,
               Mobile = u.ApplicationUser.PhoneNumber,
               HireDate = u.ApplicationUser.HireDate,
               DOB = u.DOB,
               Email = u.ApplicationUser.Email,
               ImageURL = u.ImageURL,
               ManagerID = u.ManagerID,
           }).ToListAsync();

            var employees2 = await _context.Managers
            .Include(u => u.ApplicationUser)
            .Select(u => new EmployeeDTO()
            {
                EmployeeID = u.ManagerID,
                FirstName = u.FullName.FirstName,
                MiddleName = u.FullName.MiddleName,
                LastName = u.FullName.LastName,
                JopName = "Manager",
                Address = u.ApplicationUser.Address,
                Gender = u.ApplicationUser.Gender,
                Mobile = u.ApplicationUser.PhoneNumber,
                HireDate = u.ApplicationUser.HireDate,
                DOB = u.DOB,
                Email = u.ApplicationUser.Email,
                ImageURL = u.ImageURL,
                ManagerID = u.ManagerID,
            }).ToListAsync();
            employees.AddRange(employees2);
            return employees;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting all employees: {ex.Message}");
            return new List<EmployeeDTO>();
        }

    }

    public Task<EmployeeDTO> GetEmployeeByIdAsync(int employeeId)
    {
        throw new NotImplementedException();
    }

    public async Task<string> UpdateEmployeeAsync(int id, EmployeeDTO dto)
    {
        if (dto.JopName == "Teacher")
        {
            // Note: FullName is an owned entity, so it's automatically included - no need to .Include() it
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.TeacherID == id);

            if (teacher == null) return "Teacher not found";

            // Update Teacher entity in tenant database
            if (teacher.FullName == null)
            {
                teacher.FullName = new Name();
            }

            teacher.FullName.FirstName = dto.FirstName;
            teacher.FullName.LastName = dto.LastName;
            teacher.DOB = dto.DOB;
            teacher.ImageURL = dto.ImageURL;
            teacher.ManagerID = dto.ManagerID ?? 1;

            _context.Entry(teacher).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Update ApplicationUser in admin database using IUserRepository
            if (!string.IsNullOrEmpty(teacher.UserID))
            {
                var existingUser = await _userRepository.GetUserByIdAsync(teacher.UserID);
                if (existingUser != null)
                {
                    existingUser.Address = dto.Address;
                    existingUser.Email = dto.Email;
                    existingUser.Gender = dto.Gender;
                    existingUser.PhoneNumber = dto.Mobile;
                    await _userRepository.UpdateAsync(existingUser);
                }
            }

            return "Teacher updated successfully";
        }

        if (dto.JopName == "Manager")
        {
            // Note: FullName is an owned entity, so it's automatically included - no need to .Include() it
            var manager = await _context.Managers
                .FirstOrDefaultAsync(m => m.ManagerID == id);

            if (manager == null) return "Manager not found";

            // Update Manager entity in tenant database
            if (manager.FullName == null)
            {
                manager.FullName = new Name();
            }

            manager.FullName.FirstName = dto.FirstName;
            manager.FullName.LastName = dto.LastName;
            manager.DOB = dto.DOB;
            manager.ImageURL = dto.ImageURL;

            _context.Entry(manager).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Update ApplicationUser in admin database using IUserRepository
            if (!string.IsNullOrEmpty(manager.UserID))
            {
                var existingUser = await _userRepository.GetUserByIdAsync(manager.UserID);
                if (existingUser != null)
                {
                    existingUser.Email = dto.Email;
                    existingUser.Address = dto.Address;
                    existingUser.PhoneNumber = dto.Mobile;
                    existingUser.Gender = dto.Gender;
                    await _userRepository.UpdateAsync(existingUser);
                }
            }

            return "Manager updated successfully";
        }

        return "Unknown JopName value";
    }
}
