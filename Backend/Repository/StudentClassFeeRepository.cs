using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS.School.StudentClassFee;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.EntityFrameworkCore;

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

    public async Task UpdateAsync(StudentClassFeeDTO studentClassFee)
    {
        if (studentClassFee == null)
            throw new ArgumentNullException(nameof(studentClassFee));
            
        var existingStudentClassFee =await _db.StudentClassFees.FindAsync(studentClassFee.StudentClassFeesID);
        if (existingStudentClassFee == null)
            throw new KeyNotFoundException($"StudentClassFee with ID {studentClassFee.StudentClassFeesID} not found.");

        existingStudentClassFee.StudentID = studentClassFee.StudentID;
        existingStudentClassFee.FeeClassID = studentClassFee.FeeClassID;
        existingStudentClassFee.AmountDiscount = studentClassFee.AmountDiscount;
        existingStudentClassFee.NoteDiscount = studentClassFee.NoteDiscount;
        existingStudentClassFee.Mandatory = studentClassFee.Mandatory;

        _db.Entry(existingStudentClassFee).State = EntityState.Modified;
       await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<StudentClassFees>> GetFeesByStudentIdAsync(int studentId)

        {

            return await _db.StudentClassFees

                .Where(fee => fee.StudentID == studentId)

                .ToListAsync();

        }
}
