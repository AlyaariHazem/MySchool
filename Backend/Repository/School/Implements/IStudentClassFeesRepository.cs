using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.Repository.School.Classes
{
    public interface IStudentClassFeesRepository
    {
        Task AddStudentFeesAsync(List<StudentClassFees> studentClassFees);
        Task<List<FeeClass>> GetFeesForClassAsync(int classId);
    }
}
