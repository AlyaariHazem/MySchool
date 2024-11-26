using Backend.Data;
using Backend.Models;
using Backend.Repository.School.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class StudentRepository:IStudentRepository
{
    private readonly DatabaseContext _context;

    public StudentRepository(DatabaseContext context)
    {
        _context = context;
    }

    public Task<bool> Add()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> AddStudentWithGuardianAsync(Student student, Guardian guardian, ApplicationUser guardianUser, ApplicationUser studentUser)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Check if the Guardian User already exists
            var existingGuardianUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == guardianUser.UserName);

            if (existingGuardianUser == null)
            {
                // Add Guardian as a User
                guardianUser.UserType = "Guardian";
                await _context.Users.AddAsync(guardianUser);
                await _context.SaveChangesAsync();

                // Link Guardian UserID
                guardian.UserID = guardianUser.Id;

                // Add Guardian
                await _context.Guardians.AddAsync(guardian);
                await _context.SaveChangesAsync();

                // Create an Account for the Guardian
                var account = new Accounts
                {
                    TypeAccountID =1,
                    GuardianID = guardian.GuardianID,
                    OpenBalance = 0, // Set initial balance
                    State = true
                };
                await _context.Accounts.AddAsync(account);
                await _context.SaveChangesAsync();
            }
            else
            {
                // If Guardian exists, link the GuardianID to the Student
                guardian = await _context.Guardians
                    .FirstOrDefaultAsync(g => g.UserID == existingGuardianUser.Id);
            }

            // Add Student User
            studentUser.UserType = "Student";
            await _context.Users.AddAsync(studentUser);
            await _context.SaveChangesAsync();

            // Link Student UserID and GuardianID
            student.UserID = studentUser.Id;
            student.GuardianID = guardian.GuardianID;

            // Add Student
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception("Error adding student and guardian", ex);
        }
    }

    public Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Student> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }
}
