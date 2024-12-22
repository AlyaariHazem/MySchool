using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;
using Backend.DTOS.School.Students;
using Backend.DTOS;

namespace Backend.Repository.School.Interfaces;

public interface IStudentRepository
{
    Task<Student> AddStudentAsync(Student student);
    Task<List<StudentDetailsDTO>> GetAllStudentsAsync();
    Task<StudentDetailsDTO?> GetStudentByIdAsync(int id);
    Task<Student?> GetStudentAsync(int id);
    Task<int> MaxValue();
    Task<bool> DeleteStudentAsync(int id);
     Task<AddStudentWithGuardianRequest?> GetUpdateStudentWithGuardianRequestData(int studentData);
}
