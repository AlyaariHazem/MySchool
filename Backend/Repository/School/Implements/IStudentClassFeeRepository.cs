using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.StudentClassFee;

namespace Backend.Repository.School.Implements;

public interface IStudentClassFeeRepository
{
            // Task<List<StudentClassFeeDTO>> GetAllAsync();
        // Task<StudentClassFeeDTO> GetByIdAsync(int classId, int feeId);
        Task AddAsync(StudentClassFeeDTO studentClassFee);

        Task UpdateAsync(StudentClassFeeDTO studentClassFee);
        Task DeleteAsync(int classId, int feeId);
        Task<bool> checkIfExist(int classId, int feeId);
}