using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS.School.StudentClassFee;
using Backend.Models;
using Backend.Repository.School.Implements;

namespace Backend.Repository.School.Classes;

public class StudentClassFeeRepository : IStudentClassFeeRepository
{
    private readonly DatabaseContext _db;
    private readonly IMapper _mapper;
    public StudentClassFeeRepository(DatabaseContext db,IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task AddAsync(StudentClassFeeDTO studentClassFee)
    {
        var NewStudentClassFee = _mapper.Map<StudentClassFees>(studentClassFee);
        await _db.StudentClassFees.AddAsync(NewStudentClassFee);
        await _db.SaveChangesAsync();
    }

    public Task<bool> checkIfExist(int classId, int feeId)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(int classId, int feeId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(StudentClassFeeDTO studentClassFee)
    {
        throw new NotImplementedException();
    }
}
