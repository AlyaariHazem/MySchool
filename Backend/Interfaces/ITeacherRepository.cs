using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Teachers;

namespace Backend.Interfaces;

public interface ITeacherRepository
{
    Task<TeacherDTO> AddTeacherAsync(TeacherDTO teacher);
    Task<List<TeacherDTO>> GetAllTeachersAsync();
}
