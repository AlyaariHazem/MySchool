using Backend.Data;
using Backend.DTOS.School.Students;
using Backend.Models;
using Backend.Repository.School.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class StudentRepository : IStudentRepository
{
    private readonly DatabaseContext _context;

    public StudentRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Student> AddStudentAsync(Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        return student;
    }
}
