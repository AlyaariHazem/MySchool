using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Subjects;
using Backend.Models;

namespace Backend.Interfaces;

public interface ISubjectsRepository
{
    Task<SubjectsDTO> AddSubjectAsync(SubjectsDTO subject);
    Task<List<SubjectsDTO>> GetSubjectsPaginatedAsync(int pageNumber, int pageSize);
    Task<List<SubjectsNameDTO>> GetAllSubjectsAsync();
    Task<int> GetTotalSubjectsCountAsync();
    Task<SubjectsDTO> GetSubjectByIdAsync(int id);
    Task UpdateSubjectAsync(SubjectsDTO subject);
    Task DeleteSubjectAsync(int id);
}
