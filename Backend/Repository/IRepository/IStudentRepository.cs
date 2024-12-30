using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;

using Backend.DTOS;
using Backend.DTOS.StudentsDTO;

namespace Backend.Repository.IRepository;

public interface IStudentRepository : IRepository<Student>
{


    Task<int> MaxValue();
    Task UpdateAsync(Student obj);
    Task<GetStudentForUpdateDTO?> GetUpdateStudentWithGuardianRequestData(int studentData);
}
