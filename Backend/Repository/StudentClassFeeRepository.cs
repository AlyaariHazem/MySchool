using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;

using Backend.Models;
using Backend.Repository.IRepository;

using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class StudentClassFeeRepository : Repository<StudentClassFees>, IStudentClassFeeRepository
{
    private readonly DatabaseContext _db;

    public StudentClassFeeRepository(DatabaseContext db) : base(db)
    {
        _db = db;

    }

    public Task<bool> checkIfExist(int classId, int feeId)
    {
        throw new NotImplementedException();
    }


    public async Task UpdateAsync(StudentClassFees obj)
    {
        _db.StudentClassFees.Update(obj);
        await SaveAsync();

    }


}
