using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Backend.Models;

namespace Backend.Repository.IRepository;

public interface IStudentClassFeeRepository : IRepository<StudentClassFees>
{
    // Task<List<StudentClassFeeDTO>> GetAllAsync();
    // Task<StudentClassFeeDTO> GetByIdAsync(int classId, int feeId);

    Task UpdateAsync(StudentClassFees obj);

    Task<bool> checkIfExist(int classId, int feeId);

}