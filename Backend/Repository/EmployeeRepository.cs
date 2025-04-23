using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.DTOS.School.Employee;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly DatabaseContext _context;
    public EmployeeRepository(DatabaseContext context)
    {
        _context = context;
    }
    public async Task<string> AddEmployeeAsync(EmployeeDTO employee)
    {
        if (employee.JopName == "Teacher")
        {
            // Create the User for Teacher
            var teacherUser = new ApplicationUser
            {
                UserName = employee.HireDate!.Value.Year.ToString() + employee.FirstName + employee.LastName,
                Email = employee.Email,
                Address = employee.Address,
                Gender = employee.Gender,
                HireDate = DateTime.Now,
                PhoneNumber = employee.Mobile,
                UserType = "TEACHER" // Ensure the UserType field exists in the ApplicationUser model
            };
            await _context.SaveChangesAsync();
            var teacher = new Teacher()
            {
                FullName = new Name()
                {
                    FirstName = employee.FirstName,
                    MiddleName = employee.MiddleName,
                    LastName = employee.LastName
                },
                DOB = employee.HireDate,
                ImageURL = employee.ImageURL,
                UserID = teacherUser.Id,
                ManagerID = employee.ManagerID ?? 1
            };
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();
            return "Teacher added successfully";
        }
        else if (employee.JopName == "Manager")
        {
            var ManagerUser = new ApplicationUser
            {
                UserName = employee.HireDate!.Value.Year.ToString() + employee.FirstName + employee.LastName,
                Email = employee.Email,
                Address = employee.Address,
                Gender = employee.Gender,
                HireDate = DateTime.Now,
                PhoneNumber = employee.Mobile,
                UserType = "MANAGER" // Ensure the UserType field exists in the ApplicationUser model
            };
            await _context.SaveChangesAsync();
            var manager = new Manager()
            {
                FullName = new Name()
                {
                    FirstName = employee.FirstName,
                    MiddleName = employee.MiddleName,
                    LastName = employee.LastName
                },
                DOB = employee.HireDate,
                ImageURL = employee.ImageURL,
                UserID = ManagerUser.Id
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

    public Task DeleteEmployeeAsync(int employeeId)
    {
        throw new NotImplementedException();
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
               age = u.DOB != null ? DateTime.Now.Year - u.DOB.Value.Year : 0,
               Address = u.ApplicationUser.Address,
               Gender = u.ApplicationUser.Gender,
               Mobile = u.ApplicationUser.PhoneNumber,
               HireDate = u.ApplicationUser.HireDate,
               ImageURL = u.ImageURL,
               ManagerID= u.ManagerID,
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
                age = u.DOB != null ? DateTime.Now.Year - u.DOB.Value.Year : 0,
                Address = u.ApplicationUser.Address,
                Gender = u.ApplicationUser.Gender,
                Mobile = u.ApplicationUser.PhoneNumber,
                HireDate = u.ApplicationUser.HireDate,
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

    public Task UpdateEmployeeAsync(EmployeeDTO employee)
    {
        throw new NotImplementedException();
    }
}
