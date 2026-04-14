using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Common;
using Backend.Data;
using Backend.DTOS.School.Employee;
using Backend.Interfaces;
using Backend.Models;
using Backend.Repository.School.Implements;
using Backend.Repository.School.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly TenantDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeYearAssignmentService _yearAssignments;
    private readonly IGuardianRepository _guardianRepository;
    private readonly IStudentRepository _studentRepository;

    public EmployeeRepository(
        TenantDbContext context,
        IUserRepository userRepository,
        IEmployeeYearAssignmentService yearAssignments,
        IGuardianRepository guardianRepository,
        IStudentRepository studentRepository)
    {
        _context = context;
        _userRepository = userRepository;
        _yearAssignments = yearAssignments;
        _guardianRepository = guardianRepository;
        _studentRepository = studentRepository;
    }

    public async Task<EmployeeDTO> AddEmployeeAsync(EmployeeDTO employee)
    {
        var jop = (employee.JopName ?? "").Trim();
        if (string.Equals(jop, SchoolUserRoleKeys.Teacher, StringComparison.OrdinalIgnoreCase))
            return await AddTeacherAsync(employee);
        if (string.Equals(jop, SchoolUserRoleKeys.Manager, StringComparison.OrdinalIgnoreCase))
            return await AddManagerAsync(employee);
        var staffCanonical = SchoolUserRoleKeys.ManagerTableRoles.FirstOrDefault(r =>
            string.Equals(r, jop, StringComparison.OrdinalIgnoreCase));
        if (staffCanonical != null)
            return await AddSchoolStaffAsync(employee, staffCanonical);
        if (string.Equals(jop, SchoolUserRoleKeys.Student, StringComparison.OrdinalIgnoreCase))
            return await AddStudentAsync(employee);
        if (string.Equals(jop, SchoolUserRoleKeys.Guardian, StringComparison.OrdinalIgnoreCase))
            return await AddGuardianEmployeeAsync(employee);

        throw new NotSupportedException($"Job type '{employee.JopName}' is not supported.");
    }

    private async Task<int> ResolveSchoolIdAsync(int? requestedSchoolId)
    {
        if (requestedSchoolId is > 0 && await _context.Schools.AnyAsync(s => s.SchoolID == requestedSchoolId))
            return requestedSchoolId.Value;

        return await _context.Schools.OrderBy(s => s.SchoolID).Select(s => s.SchoolID).FirstAsync();
    }

    private static string DefaultPassword(EmployeeDTO employee) =>
        string.IsNullOrWhiteSpace(employee.Password) ? "TEACHER" : employee.Password!;

    private async Task<EmployeeDTO> AddTeacherAsync(EmployeeDTO employee)
    {
        var userNameTeacher = "Teach_" + Guid.NewGuid().ToString("N").Substring(0, 5);
        var teacherUser = new ApplicationUser
        {
            UserName = userNameTeacher,
            Email = employee.Email,
            Address = employee.Address,
            Gender = employee.Gender,
            HireDate = DateTime.Now,
            PhoneNumber = employee.Mobile,
            UserType = "TEACHER"
        };
        var createdUser = await _userRepository.CreateUserAsync(teacherUser, DefaultPassword(employee), "TEACHER");
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
        await _yearAssignments.EnsureActiveAssignmentAsync(
            EmployeeYearAssignmentRoles.Teacher, teacher.TeacherID, null, _context);
        return MapTeacherToDto(teacher, createdUser);
    }

    private async Task<EmployeeDTO> AddManagerAsync(EmployeeDTO employee)
    {
        var userNameManager = "Manage_" + Guid.NewGuid().ToString("N").Substring(0, 5);
        var schoolId = await ResolveSchoolIdAsync(employee.SchoolID);
        var managerUser = new ApplicationUser
        {
            UserName = userNameManager,
            Email = employee.Email,
            Address = employee.Address,
            Gender = employee.Gender,
            HireDate = DateTime.Now,
            PhoneNumber = employee.Mobile,
            UserType = "MANAGER"
        };
        var createdUser = await _userRepository.CreateUserAsync(managerUser, DefaultPassword(employee), "TEACHER");
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
            UserID = createdUser.Id,
            SchoolID = schoolId
        };
        _context.Managers.Add(manager);
        await _context.SaveChangesAsync();
        await _yearAssignments.EnsureActiveAssignmentAsync(
            EmployeeYearAssignmentRoles.Manager, manager.ManagerID, null, _context);
        return MapManagerToDto(manager, createdUser);
    }

    private async Task<EmployeeDTO> AddSchoolStaffAsync(EmployeeDTO employee, string staffRole)
    {
        var schoolId = await ResolveSchoolIdAsync(employee.SchoolID);
        var userName = "Staff_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        var staffUser = new ApplicationUser
        {
            UserName = userName,
            Email = employee.Email,
            Address = employee.Address,
            Gender = employee.Gender,
            HireDate = DateTime.Now,
            PhoneNumber = employee.Mobile,
            UserType = staffRole
        };
        var createdUser = await _userRepository.CreateUserAsync(staffUser, DefaultPassword(employee), "TEACHER");
        var row = new SchoolStaff
        {
            SchoolID = schoolId,
            UserID = createdUser.Id,
            StaffRole = staffRole,
            FullName = new Name
            {
                FirstName = employee.FirstName,
                LastName = employee.LastName
            },
            DOB = employee.DOB,
            ImageURL = employee.ImageURL
        };
        _context.SchoolStaff.Add(row);
        await _context.SaveChangesAsync();
        await _yearAssignments.EnsureActiveAssignmentAsync(
            EmployeeYearAssignmentRoles.SchoolStaff, row.SchoolStaffID, null, _context);
        return MapSchoolStaffToDto(row, createdUser);
    }

    private async Task<EmployeeDTO> AddStudentAsync(EmployeeDTO employee)
    {
        if (employee.DivisionID is not int divId || divId <= 0)
            throw new ArgumentException("DivisionID is required for Student.");
        if (employee.GuardianID is not int gid || gid <= 0)
            throw new ArgumentException("GuardianID is required for Student.");

        var guardianExists = await _context.Guardians.AsNoTracking().AnyAsync(g => g.GuardianID == gid);
        if (!guardianExists)
            throw new ArgumentException($"Guardian {gid} was not found.");

        var userName = "Student_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        var studentUser = new ApplicationUser
        {
            UserName = userName,
            Email = employee.Email,
            Address = employee.Address,
            Gender = employee.Gender,
            HireDate = DateTime.Now,
            PhoneNumber = employee.Mobile,
            UserType = "Student"
        };
        var createdUser = await _userRepository.CreateUserAsync(studentUser, DefaultPassword(employee), "Student");
        var student = new Student
        {
            FullName = new Name
            {
                FirstName = employee.FirstName,
                LastName = employee.LastName
            },
            StudentDOB = employee.DOB ?? DateTime.Now,
            ImageURL = employee.ImageURL,
            DivisionID = divId,
            GuardianID = gid,
            UserID = createdUser.Id
        };
        var added = await _studentRepository.AddStudentAsync(student);
        await _yearAssignments.EnsureActiveAssignmentAsync(
            EmployeeYearAssignmentRoles.Student, added.StudentID, null, _context);
        return MapStudentToDto(added, createdUser);
    }

    private async Task<EmployeeDTO> AddGuardianEmployeeAsync(EmployeeDTO employee)
    {
        var userName = "Guardian_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        var guardianUser = new ApplicationUser
        {
            UserName = userName,
            Email = employee.Email,
            Address = employee.Address,
            Gender = employee.Gender,
            HireDate = DateTime.Now,
            PhoneNumber = employee.Mobile,
            UserType = "Guardian"
        };
        var createdUser = await _userRepository.CreateUserAsync(guardianUser, DefaultPassword(employee), "Guardian");
        var full = $"{employee.FirstName} {employee.LastName}".Trim();
        var guardian = new Guardian
        {
            FullName = string.IsNullOrWhiteSpace(full) ? (employee.Email ?? userName) : full,
            GuardianDOB = employee.DOB,
            UserID = createdUser.Id
        };
        var added = await _guardianRepository.AddGuardianAsync(guardian);
        await _yearAssignments.EnsureActiveAssignmentAsync(
            EmployeeYearAssignmentRoles.Guardian, added.GuardianID, null, _context);
        var row = await _context.Guardians.AsNoTracking().FirstAsync(x => x.GuardianID == added.GuardianID);
        return MapGuardianToDto(row, createdUser);
    }

    public Task<bool> ExistsForJobTypeAsync(int employeeId, string jopName) =>
        EntityExistsForRoleAsync(employeeId, jopName);

    public async Task<bool> DeleteEmployeeAsync(int employeeId, string jopName)
    {
        var role = EmployeeJopNameToYearRole.ToAssignmentRole(jopName);
        var exists = await EntityExistsForRoleAsync(employeeId, jopName);
        if (!exists)
            return false;

        await _yearAssignments.ArchiveEmployeeForYearAsync(
            role,
            employeeId,
            yearId: null,
            exitDate: DateTime.UtcNow,
            exitReason: null,
            notes: "Archived via employee API (replaces physical delete).",
            _context);
        return true;
    }

    private async Task<bool> EntityExistsForRoleAsync(int id, string jopName)
    {
        var j = (jopName ?? "").Trim();
        if (string.Equals(j, SchoolUserRoleKeys.Teacher, StringComparison.OrdinalIgnoreCase))
            return await _context.Teachers.AsNoTracking().AnyAsync(t => t.TeacherID == id);
        if (string.Equals(j, SchoolUserRoleKeys.Manager, StringComparison.OrdinalIgnoreCase))
            return await _context.Managers.AsNoTracking().AnyAsync(m => m.ManagerID == id);
        if (string.Equals(j, SchoolUserRoleKeys.Student, StringComparison.OrdinalIgnoreCase))
            return await _context.Students.AsNoTracking().AnyAsync(s => s.StudentID == id);
        if (string.Equals(j, SchoolUserRoleKeys.Guardian, StringComparison.OrdinalIgnoreCase))
            return await _context.Guardians.AsNoTracking().AnyAsync(g => g.GuardianID == id);
        foreach (var r in SchoolUserRoleKeys.ManagerTableRoles)
        {
            if (!string.Equals(j, r, StringComparison.OrdinalIgnoreCase)) continue;
            var staff = await _context.SchoolStaff.AsNoTracking()
                .FirstOrDefaultAsync(s => s.SchoolStaffID == id);
            if (staff == null) return false;
            return string.Equals(staff.StaffRole?.Trim(), r, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    public async Task<List<EmployeeDTO>> GetAllEmployeesAsync(int? schoolYearId = null)
    {
        try
        {
            var teachers = await _context.Teachers.AsNoTracking().ToListAsync();
            var managers = await _context.Managers.AsNoTracking().ToListAsync();
            var schoolStaff = await _context.SchoolStaff.AsNoTracking().ToListAsync();
            var students = await _context.Students.AsNoTracking().ToListAsync();
            var guardians = await _context.Guardians.AsNoTracking().ToListAsync();

            if (await _yearAssignments.TenantUsesYearAssignmentsAsync(_context))
            {
                var yearId = await _yearAssignments.ResolveYearIdForListAsync(schoolYearId, _context);
                if (yearId is > 0)
                {
                    var activeT = await _yearAssignments.GetActiveEntityIdsForYearAsync(
                        EmployeeYearAssignmentRoles.Teacher, yearId.Value, _context);
                    var activeM = await _yearAssignments.GetActiveEntityIdsForYearAsync(
                        EmployeeYearAssignmentRoles.Manager, yearId.Value, _context);
                    var activeS = await _yearAssignments.GetActiveEntityIdsForYearAsync(
                        EmployeeYearAssignmentRoles.SchoolStaff, yearId.Value, _context);
                    var activeSt = await _yearAssignments.GetActiveEntityIdsForYearAsync(
                        EmployeeYearAssignmentRoles.Student, yearId.Value, _context);
                    var activeG = await _yearAssignments.GetActiveEntityIdsForYearAsync(
                        EmployeeYearAssignmentRoles.Guardian, yearId.Value, _context);

                    teachers = teachers.Where(t => activeT.Contains(t.TeacherID)).ToList();
                    managers = managers.Where(m => activeM.Contains(m.ManagerID)).ToList();
                    schoolStaff = schoolStaff.Where(s => activeS.Contains(s.SchoolStaffID)).ToList();
                    students = students.Where(s => activeSt.Contains(s.StudentID)).ToList();
                    guardians = guardians.Where(g => activeG.Contains(g.GuardianID)).ToList();
                }
            }

            var userIds = teachers.Select(t => t.UserID)
                .Concat(managers.Select(m => m.UserID))
                .Concat(schoolStaff.Select(s => s.UserID))
                .Concat(students.Select(s => s.UserID))
                .Concat(guardians.Select(g => g.UserID))
                .Where(uid => !string.IsNullOrEmpty(uid))
                .Distinct()
                .ToList();

            var userById = new Dictionary<string, ApplicationUser?>(StringComparer.Ordinal);
            foreach (var uid in userIds)
                userById[uid] = await _userRepository.GetUserByIdAsync(uid);

            var result = new List<EmployeeDTO>(
                teachers.Count + managers.Count + schoolStaff.Count + students.Count + guardians.Count);

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

            foreach (var s in schoolStaff)
            {
                userById.TryGetValue(s.UserID ?? "", out var user);
                result.Add(MapSchoolStaffToDto(s, user));
            }

            foreach (var s in students)
            {
                userById.TryGetValue(s.UserID ?? "", out var user);
                result.Add(MapStudentToDto(s, user));
            }

            foreach (var g in guardians)
            {
                userById.TryGetValue(g.UserID ?? "", out var user);
                result.Add(MapGuardianToDto(g, user));
            }

            return result
                .OrderBy(e => e.LastName ?? "")
                .ThenBy(e => e.FirstName ?? "")
                .ThenBy(e => e.JopName)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting all employees: {ex.Message}");
            return new List<EmployeeDTO>();
        }
    }

    public async Task<EmployeeDTO?> GetEmployeeByIdAsync(int employeeId, string? jobTypeHint = null)
    {
        if (!string.IsNullOrWhiteSpace(jobTypeHint))
        {
            var hint = jobTypeHint.Trim();
            if (string.Equals(hint, SchoolUserRoleKeys.Teacher, StringComparison.OrdinalIgnoreCase))
            {
                var teacher = await _context.Teachers.AsNoTracking().FirstOrDefaultAsync(t => t.TeacherID == employeeId);
                if (teacher == null) return null;
                var u = string.IsNullOrEmpty(teacher.UserID) ? null : await _userRepository.GetUserByIdAsync(teacher.UserID);
                return MapTeacherToDto(teacher, u);
            }

            if (string.Equals(hint, SchoolUserRoleKeys.Manager, StringComparison.OrdinalIgnoreCase))
            {
                var manager = await _context.Managers.AsNoTracking().FirstOrDefaultAsync(m => m.ManagerID == employeeId);
                if (manager == null) return null;
                var u = string.IsNullOrEmpty(manager.UserID) ? null : await _userRepository.GetUserByIdAsync(manager.UserID);
                return MapManagerToDto(manager, u);
            }

            if (string.Equals(hint, SchoolUserRoleKeys.Student, StringComparison.OrdinalIgnoreCase))
            {
                var st = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.StudentID == employeeId);
                if (st == null) return null;
                var u = string.IsNullOrEmpty(st.UserID) ? null : await _userRepository.GetUserByIdAsync(st.UserID);
                return MapStudentToDto(st, u);
            }

            if (string.Equals(hint, SchoolUserRoleKeys.Guardian, StringComparison.OrdinalIgnoreCase))
            {
                var g = await _context.Guardians.AsNoTracking().FirstOrDefaultAsync(x => x.GuardianID == employeeId);
                if (g == null) return null;
                var u = string.IsNullOrEmpty(g.UserID) ? null : await _userRepository.GetUserByIdAsync(g.UserID);
                return MapGuardianToDto(g, u);
            }

            foreach (var r in SchoolUserRoleKeys.ManagerTableRoles)
            {
                if (!string.Equals(hint, r, StringComparison.OrdinalIgnoreCase)) continue;
                var row = await _context.SchoolStaff.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SchoolStaffID == employeeId);
                if (row == null) return null;
                if (!string.Equals(row.StaffRole?.Trim(), r, StringComparison.OrdinalIgnoreCase))
                    return null;
                var u = string.IsNullOrEmpty(row.UserID) ? null : await _userRepository.GetUserByIdAsync(row.UserID);
                return MapSchoolStaffToDto(row, u);
            }

            return null;
        }

        var t2 = await _context.Teachers.AsNoTracking().FirstOrDefaultAsync(t => t.TeacherID == employeeId);
        if (t2 != null)
        {
            var u = string.IsNullOrEmpty(t2.UserID) ? null : await _userRepository.GetUserByIdAsync(t2.UserID);
            return MapTeacherToDto(t2, u);
        }

        var m2 = await _context.Managers.AsNoTracking().FirstOrDefaultAsync(m => m.ManagerID == employeeId);
        if (m2 != null)
        {
            var u = string.IsNullOrEmpty(m2.UserID) ? null : await _userRepository.GetUserByIdAsync(m2.UserID);
            return MapManagerToDto(m2, u);
        }

        var ss = await _context.SchoolStaff.AsNoTracking().FirstOrDefaultAsync(s => s.SchoolStaffID == employeeId);
        if (ss != null)
        {
            var u = string.IsNullOrEmpty(ss.UserID) ? null : await _userRepository.GetUserByIdAsync(ss.UserID);
            return MapSchoolStaffToDto(ss, u);
        }

        var st2 = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.StudentID == employeeId);
        if (st2 != null)
        {
            var u = string.IsNullOrEmpty(st2.UserID) ? null : await _userRepository.GetUserByIdAsync(st2.UserID);
            return MapStudentToDto(st2, u);
        }

        var g2 = await _context.Guardians.AsNoTracking().FirstOrDefaultAsync(g => g.GuardianID == employeeId);
        if (g2 != null)
        {
            var u = string.IsNullOrEmpty(g2.UserID) ? null : await _userRepository.GetUserByIdAsync(g2.UserID);
            return MapGuardianToDto(g2, u);
        }

        return null;
    }

    public async Task<EmployeeDTO?> UpdateEmployeeAsync(int id, EmployeeDTO dto)
    {
        var kind = (dto.JopName ?? "").Trim();
        if (string.IsNullOrEmpty(kind))
        {
            if (await _context.Teachers.AnyAsync(t => t.TeacherID == id))
                kind = SchoolUserRoleKeys.Teacher;
            else if (await _context.Managers.AnyAsync(m => m.ManagerID == id))
                kind = SchoolUserRoleKeys.Manager;
            else if (await _context.SchoolStaff.AnyAsync(s => s.SchoolStaffID == id))
                kind = (await _context.SchoolStaff.AsNoTracking().FirstAsync(s => s.SchoolStaffID == id)).StaffRole;
            else if (await _context.Students.AnyAsync(s => s.StudentID == id))
                kind = SchoolUserRoleKeys.Student;
            else if (await _context.Guardians.AnyAsync(g => g.GuardianID == id))
                kind = SchoolUserRoleKeys.Guardian;
            else
                return null;
        }

        if (string.Equals(kind, SchoolUserRoleKeys.Teacher, StringComparison.OrdinalIgnoreCase))
        {
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.TeacherID == id);
            if (teacher == null) return null;
            if (teacher.FullName == null) teacher.FullName = new Name();
            teacher.FullName.FirstName = dto.FirstName;
            teacher.FullName.LastName = dto.LastName;
            teacher.DOB = dto.DOB;
            teacher.ImageURL = dto.ImageURL;
            teacher.ManagerID = dto.ManagerID ?? 1;
            _context.Entry(teacher).State = EntityState.Modified;
            await _context.SaveChangesAsync();
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

            return await GetEmployeeByIdAsync(id, SchoolUserRoleKeys.Teacher);
        }

        if (string.Equals(kind, SchoolUserRoleKeys.Manager, StringComparison.OrdinalIgnoreCase))
        {
            var manager = await _context.Managers.FirstOrDefaultAsync(m => m.ManagerID == id);
            if (manager == null) return null;
            if (manager.FullName == null) manager.FullName = new Name();
            manager.FullName.FirstName = dto.FirstName;
            manager.FullName.LastName = dto.LastName;
            manager.DOB = dto.DOB;
            manager.ImageURL = dto.ImageURL;
            if (dto.SchoolID is > 0)
                manager.SchoolID = dto.SchoolID.Value;
            _context.Entry(manager).State = EntityState.Modified;
            await _context.SaveChangesAsync();
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

            return await GetEmployeeByIdAsync(id, SchoolUserRoleKeys.Manager);
        }

        if (SchoolUserRoleKeys.ManagerTableRoles.Contains(kind, StringComparer.OrdinalIgnoreCase))
        {
            var staff = await _context.SchoolStaff.FirstOrDefaultAsync(s => s.SchoolStaffID == id && s.StaffRole == kind);
            if (staff == null) return null;
            if (staff.FullName == null) staff.FullName = new Name();
            staff.FullName.FirstName = dto.FirstName;
            staff.FullName.LastName = dto.LastName;
            staff.DOB = dto.DOB;
            staff.ImageURL = dto.ImageURL;
            if (dto.SchoolID is > 0)
                staff.SchoolID = dto.SchoolID.Value;
            _context.Entry(staff).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            if (!string.IsNullOrEmpty(staff.UserID))
            {
                var existingUser = await _userRepository.GetUserByIdAsync(staff.UserID);
                if (existingUser != null)
                {
                    existingUser.Email = dto.Email;
                    existingUser.Address = dto.Address;
                    existingUser.PhoneNumber = dto.Mobile;
                    existingUser.Gender = dto.Gender;
                    await _userRepository.UpdateAsync(existingUser);
                }
            }

            return await GetEmployeeByIdAsync(id, kind);
        }

        if (string.Equals(kind, SchoolUserRoleKeys.Student, StringComparison.OrdinalIgnoreCase))
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentID == id);
            if (student == null) return null;
            if (student.FullName == null) student.FullName = new Name();
            student.FullName.FirstName = dto.FirstName;
            student.FullName.LastName = dto.LastName;
            student.StudentDOB = dto.DOB;
            student.ImageURL = dto.ImageURL;
            if (dto.DivisionID is > 0)
                student.DivisionID = dto.DivisionID.Value;
            if (dto.GuardianID is > 0)
                student.GuardianID = dto.GuardianID.Value;
            _context.Entry(student).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            if (!string.IsNullOrEmpty(student.UserID))
            {
                var existingUser = await _userRepository.GetUserByIdAsync(student.UserID);
                if (existingUser != null)
                {
                    existingUser.Email = dto.Email;
                    existingUser.Address = dto.Address;
                    existingUser.PhoneNumber = dto.Mobile;
                    existingUser.Gender = dto.Gender;
                    await _userRepository.UpdateAsync(existingUser);
                }
            }

            return await GetEmployeeByIdAsync(id, SchoolUserRoleKeys.Student);
        }

        if (string.Equals(kind, SchoolUserRoleKeys.Guardian, StringComparison.OrdinalIgnoreCase))
        {
            var guardian = await _context.Guardians.FirstOrDefaultAsync(g => g.GuardianID == id);
            if (guardian == null) return null;
            guardian.FullName = $"{dto.FirstName} {dto.LastName}".Trim();
            guardian.GuardianDOB = dto.DOB;
            _context.Entry(guardian).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            if (!string.IsNullOrEmpty(guardian.UserID))
            {
                var existingUser = await _userRepository.GetUserByIdAsync(guardian.UserID);
                if (existingUser != null)
                {
                    existingUser.Email = dto.Email;
                    existingUser.Address = dto.Address;
                    existingUser.PhoneNumber = dto.Mobile;
                    existingUser.Gender = dto.Gender;
                    await _userRepository.UpdateAsync(existingUser);
                }
            }

            return await GetEmployeeByIdAsync(id, SchoolUserRoleKeys.Guardian);
        }

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
            JopName = SchoolUserRoleKeys.Teacher,
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
            JopName = SchoolUserRoleKeys.Manager,
            Address = user?.Address,
            Gender = user?.Gender,
            Mobile = user?.PhoneNumber,
            HireDate = user?.HireDate,
            DOB = u.DOB,
            Email = user?.Email,
            ImageURL = u.ImageURL,
            ManagerID = u.ManagerID,
            SchoolID = u.SchoolID,
        };
    }

    private static EmployeeDTO MapSchoolStaffToDto(SchoolStaff u, ApplicationUser? user = null)
    {
        return new EmployeeDTO
        {
            EmployeeID = u.SchoolStaffID,
            FirstName = u.FullName.FirstName,
            MiddleName = u.FullName.MiddleName,
            LastName = u.FullName.LastName,
            JopName = u.StaffRole,
            Address = user?.Address,
            Gender = user?.Gender,
            Mobile = user?.PhoneNumber,
            HireDate = user?.HireDate,
            DOB = u.DOB,
            Email = user?.Email,
            ImageURL = u.ImageURL,
            SchoolID = u.SchoolID,
        };
    }

    private static EmployeeDTO MapStudentToDto(Student s, ApplicationUser? user = null)
    {
        return new EmployeeDTO
        {
            EmployeeID = s.StudentID,
            FirstName = s.FullName.FirstName,
            MiddleName = s.FullName.MiddleName,
            LastName = s.FullName.LastName,
            JopName = SchoolUserRoleKeys.Student,
            Address = user?.Address,
            Gender = user?.Gender,
            Mobile = user?.PhoneNumber,
            HireDate = user?.HireDate,
            DOB = s.StudentDOB,
            Email = user?.Email,
            ImageURL = s.ImageURL,
            DivisionID = s.DivisionID,
            GuardianID = s.GuardianID,
        };
    }

    private static EmployeeDTO MapGuardianToDto(Guardian g, ApplicationUser? user = null)
    {
        var parts = (g.FullName ?? "").Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return new EmployeeDTO
        {
            EmployeeID = g.GuardianID,
            FirstName = parts.ElementAtOrDefault(0) ?? g.FullName ?? "",
            LastName = parts.ElementAtOrDefault(1) ?? "",
            JopName = SchoolUserRoleKeys.Guardian,
            Address = user?.Address,
            Gender = user?.Gender,
            Mobile = user?.PhoneNumber,
            HireDate = user?.HireDate,
            DOB = g.GuardianDOB,
            Email = user?.Email,
        };
    }
}
