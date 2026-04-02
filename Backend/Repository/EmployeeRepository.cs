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
    public async Task<EmployeeDTO> AddEmployeeAsync(EmployeeDTO employee)
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
            return MapTeacherToDto(teacher, createdUser);
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
            return MapManagerToDto(manager, createdUser);
        }
        else
        {
            throw new NotImplementedException("Job type not implemented");
        }
    }

    public async Task DeleteEmployeeAsync(int employeeId, string jopName)
    {
        var kind = NormalizeJobKind(jopName);
        if (kind == null)
            return;

        if (kind == "Teacher")
        {
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.TeacherID == employeeId);
            if (teacher != null)
            {
                await RemoveTeacherDependenciesAsync(employeeId);
                await _userRepository.DeleteAsync(teacher.UserID);
                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();
            }
        }
        else if (kind == "Manager")
        {
            var manager = await _context.Managers.FirstOrDefaultAsync(m => m.ManagerID == employeeId);
            if (manager != null)
            {
                await _userRepository.DeleteAsync(manager.UserID);
                _context.Managers.Remove(manager);
                await _context.SaveChangesAsync();
            }
        }
    }

    /// <summary>
    /// Tenant DB FKs to teachers often use NO ACTION in SQL even when the EF model suggests cascade.
    /// Remove or detach dependents so the teacher row can be deleted.
    /// </summary>
    private async Task RemoveTeacherDependenciesAsync(int teacherId)
    {
        var coursePlans = await _context.CoursePlans.Where(c => c.TeacherID == teacherId).ToListAsync();
        if (coursePlans.Count > 0)
            _context.CoursePlans.RemoveRange(coursePlans);

        var salaries = await _context.Salarys.Where(s => s.TeacherID == teacherId).ToListAsync();
        if (salaries.Count > 0)
            _context.Salarys.RemoveRange(salaries);

        var schedules = await _context.WeeklySchedules.Where(w => w.TeacherID == teacherId).ToListAsync();
        foreach (var s in schedules)
            s.TeacherID = null;

        var classes = await _context.Classes.Where(c => c.TeacherID == teacherId).ToListAsync();
        foreach (var c in classes)
            c.TeacherID = null;
    }

    public async Task<List<EmployeeDTO>> GetAllEmployeesAsync()
    {
        try
        {
            // ApplicationUser is Ignored() on Teacher/Manager in TenantDbContext — load users via IUserRepository.
            var teachers = await _context.Teachers.AsNoTracking().ToListAsync();
            var managers = await _context.Managers.AsNoTracking().ToListAsync();

            var userIds = teachers.Select(t => t.UserID)
                .Concat(managers.Select(m => m.UserID))
                .Where(uid => !string.IsNullOrEmpty(uid))
                .Distinct()
                .ToList();

            var userById = new Dictionary<string, ApplicationUser?>(StringComparer.Ordinal);
            foreach (var uid in userIds)
                userById[uid] = await _userRepository.GetUserByIdAsync(uid);

            var result = new List<EmployeeDTO>(teachers.Count + managers.Count);
            foreach (var t in teachers)
            {
                userById.TryGetValue(t.UserID ?? "", out var user);
                result.Add(MapTeacherToDto(t, user));
            }

            foreach (var m in managers)
            {
                userById.TryGetValue(m.UserID ?? "", out var user);
                result.Add(MapManagerToDto(m, user));
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting all employees: {ex.Message}");
            return new List<EmployeeDTO>();
        }
    }

    public async Task<EmployeeDTO?> GetEmployeeByIdAsync(int employeeId)
    {
        var teacher = await _context.Teachers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TeacherID == employeeId);
        if (teacher != null)
        {
            var user = string.IsNullOrEmpty(teacher.UserID)
                ? null
                : await _userRepository.GetUserByIdAsync(teacher.UserID);
            return MapTeacherToDto(teacher, user);
        }

        var manager = await _context.Managers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ManagerID == employeeId);
        if (manager != null)
        {
            var user = string.IsNullOrEmpty(manager.UserID)
                ? null
                : await _userRepository.GetUserByIdAsync(manager.UserID);
            return MapManagerToDto(manager, user);
        }

        return null;
    }

    public async Task<EmployeeDTO?> UpdateEmployeeAsync(int id, EmployeeDTO dto)
    {
        var kind = NormalizeJobKind(dto.JopName);
        if (kind == null)
        {
            if (await _context.Teachers.AnyAsync(t => t.TeacherID == id))
                kind = "Teacher";
            else if (await _context.Managers.AnyAsync(m => m.ManagerID == id))
                kind = "Manager";
            else
                return null;
        }

        if (kind == "Teacher")
        {
            // Note: FullName is an owned entity, so it's automatically included - no need to .Include() it
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.TeacherID == id);

            if (teacher == null) return null;

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

            return await GetEmployeeByIdAsync(id);
        }

        if (kind == "Manager")
        {
            // Note: FullName is an owned entity, so it's automatically included - no need to .Include() it
            var manager = await _context.Managers
                .FirstOrDefaultAsync(m => m.ManagerID == id);

            if (manager == null) return null;

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

            return await GetEmployeeByIdAsync(id);
        }

        return null;
    }

    private static string? NormalizeJobKind(string? jopName)
    {
        if (string.IsNullOrWhiteSpace(jopName))
            return null;
        if (string.Equals(jopName, "Teacher", StringComparison.OrdinalIgnoreCase))
            return "Teacher";
        if (string.Equals(jopName, "Manager", StringComparison.OrdinalIgnoreCase))
            return "Manager";
        return null;
    }

    private static EmployeeDTO MapTeacherToDto(Teacher u, ApplicationUser? user = null)
    {
        return new EmployeeDTO
        {
            EmployeeID = u.TeacherID,
            FirstName = u.FullName.FirstName,
            MiddleName = u.FullName.MiddleName,
            LastName = u.FullName.LastName,
            JopName = "Teacher",
            Address = user?.Address,
            Gender = user?.Gender,
            Mobile = user?.PhoneNumber,
            HireDate = user?.HireDate,
            DOB = u.DOB,
            Email = user?.Email,
            ImageURL = u.ImageURL,
            ManagerID = u.ManagerID,
        };
    }

    private static EmployeeDTO MapManagerToDto(Manager u, ApplicationUser? user = null)
    {
        return new EmployeeDTO
        {
            EmployeeID = u.ManagerID,
            FirstName = u.FullName.FirstName,
            MiddleName = u.FullName.MiddleName,
            LastName = u.FullName.LastName,
            JopName = "Manager",
            Address = user?.Address,
            Gender = user?.Gender,
            Mobile = user?.PhoneNumber,
            HireDate = user?.HireDate,
            DOB = u.DOB,
            Email = user?.Email,
            ImageURL = u.ImageURL,
            ManagerID = u.ManagerID,
        };
    }
}
