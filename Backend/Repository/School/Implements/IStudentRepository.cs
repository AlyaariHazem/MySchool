using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.Repository.School.Interfaces;

public interface IStudentRepository
{
    Task<Student> AddStudentAsync(Student student);

}
