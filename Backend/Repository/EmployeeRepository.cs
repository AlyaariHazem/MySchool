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
    private readonly DatabaseContext _context;
    private readonly IUserRepository _userRepository;
    public EmployeeRepository(DatabaseContext context, IUserRepository userRepository)
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

    public Task DeleteEmployeeAsync(int employeeId, string jopName)
    {
        if (jopName == "Teacher")
        {
            var teacher = _context.Teachers.FirstOrDefault(t => t.TeacherID == employeeId);
            if (teacher != null)
            {
                var user= _context.Users.FirstOrDefault(u => u.Id == teacher.UserID);
                if (user != null)
                {
                    _context.Users.Remove(user);
                }
                _context.Teachers.Remove(teacher);
                return _context.SaveChangesAsync();
            }
        }
        else if (jopName == "Manager")
        {
            var manager = _context.Managers.FirstOrDefault(m => m.ManagerID == employeeId);
            if (manager != null)
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == manager.UserID);
                if (user != null)
                {
                    _context.Users.Remove(user);
                }
                _context.Managers.Remove(manager);
                return _context.SaveChangesAsync();
            }
        }
        return Task.CompletedTask;
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
            var teacher = await _context.Teachers
                .Include(t => t.FullName)
                .Include(t => t.ApplicationUser)
                .FirstOrDefaultAsync(t => t.TeacherID == id);

            if (teacher == null) return "Teacher not found";

            teacher.ApplicationUser ??= new ApplicationUser();

            teacher.FullName.FirstName = dto.FirstName;
            teacher.FullName.LastName = dto.LastName;
            teacher.DOB = dto.DOB;
            teacher.ImageURL = dto.ImageURL;
            teacher.ManagerID = dto.ManagerID ?? 1;
            teacher.ApplicationUser.Address = dto.Address;
            teacher.ApplicationUser.Email = dto.Email;
            teacher.ApplicationUser.Gender = dto.Gender;
            teacher.ApplicationUser.PhoneNumber = dto.Mobile;

            _context.Entry(teacher).State = EntityState.Modified;
            _context.Entry(teacher.ApplicationUser).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return "Teacher updated successfully";
        }

        if (dto.JopName == "Manager")
        {
            var manager = await _context.Managers
                .Include(m => m.FullName)
                .Include(m => m.ApplicationUser)
                .FirstOrDefaultAsync(m => m.ManagerID == id);

            if (manager == null) return "Manager not found";

            manager.ApplicationUser ??= new ApplicationUser();

            manager.FullName.FirstName = dto.FirstName;
            manager.FullName.LastName = dto.LastName;
            manager.DOB = dto.DOB;
            manager.ImageURL = dto.ImageURL;
            manager.ApplicationUser.Email = dto.Email;
            manager.ApplicationUser.Address = dto.Address;
            manager.ApplicationUser.PhoneNumber = dto.Mobile;
            manager.ApplicationUser.Gender = dto.Gender;

            _context.Entry(manager).State = EntityState.Modified;
            _context.Entry(manager.ApplicationUser).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return "Manager updated successfully";
        }

        return "Unknown JopName value";
    }
}
