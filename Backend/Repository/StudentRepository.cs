using Backend.Data;
using Backend.DTOS;

using Backend.DTOS.StudentsDTO;
using Backend.Models;
using Backend.Repository;
using Backend.Repository.IRepository;
using Backend.Repository.School.Interfaces;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class StudentRepository : Repository<Student>, IStudentRepository
{
    private readonly DatabaseContext _db;

    public StudentRepository(DatabaseContext db) : base(db)
    {
        _db = db;

    }

    public Task<GetStudentForUpdateDTO?> GetUpdateStudentWithGuardianRequestData(int studentData)
    {
        throw new NotImplementedException();
    }

    public async Task<int> MaxValue()
    {
        var maxValue = await _db.Students.MaxAsync(s => (int?)s.StudentID) ?? 0;
        return maxValue;
    }

    public Task UpdateAsync(Student obj)
    {
        _db.Students.Update(obj);
        return SaveAsync();
    }

}
