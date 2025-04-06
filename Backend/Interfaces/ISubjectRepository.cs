using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Subjects;

namespace Backend.Repository.School.Implements;

public interface ISubjectRepository
{
    Task AddSubjectAsync(SubjectsDTO subject);
    Task<List<SubjectsDTO>> GetSubjectsPaginatedAsync(int pageNumber, int pageSize);
    Task<int> GetTotalSubjectsCountAsync();
    Task<SubjectsDTO> GetSubjectByIdAsync(int id);
    Task UpdateSubjectAsync(SubjectsDTO subject);
    Task DeleteSubjectAsync(int id);
}