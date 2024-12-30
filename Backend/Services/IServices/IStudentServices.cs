using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;
using Backend.DTOS;
using System.Linq.Expressions;
using Backend.DTOS.StudentsDTO;
using Backend.DTOS.RegisterStudentsDTO;

namespace Backend.Services.IServices
{
    public interface IStudentServices
    {
        Task<Student> AddAsync(Student student);
        Task<RegisterResult> RegisterWithGuardianAsync(RegisterStudentWithGuardianDTO model);
        Task<RegisterResult> RegisterAsync(RegisterStudentDTO model);
        Task<List<StudentDetailsDTO>> GetAllWithDetailsAsync(Expression<Func<Student, bool>> fillter = null);
        Task<StudentDetailsDTO?> GetWithDetailsAsync(Expression<Func<Student, bool>> fillter);
        Task<Student?> GetAsync(Expression<Func<Student, bool>> fillter);
        Task<bool> DeleteAsync(int Id);

    }

}

