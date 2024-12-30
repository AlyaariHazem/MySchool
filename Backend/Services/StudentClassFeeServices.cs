using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;

using Backend.DTOS.StudentClassFeesDTO;
using Backend.Models;

using Backend.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class StudentClassFeeServices : IStudentClassFeeServices
    {
        private readonly DatabaseContext _db;
        private readonly IMapper _mapper;
        public StudentClassFeeServices(DatabaseContext db, IMapper mapper)
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
        public async Task<IEnumerable<StudentClassFees>> GetFeesByStudentIdAsync(int studentId)

        {

            return await _db.StudentClassFees

                .Where(fee => fee.StudentID == studentId)

                .ToListAsync();

        }
    }
}

