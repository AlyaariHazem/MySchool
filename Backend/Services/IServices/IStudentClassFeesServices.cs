using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.Services.IServices
{
    public interface IStudentClassFeesServices
    {
        Task AddStudentFeesAsync(List<StudentClassFees> studentClassFees);
        Task<List<FeeClass>> GetFeesForClassAsync(int classId);

    }
}
