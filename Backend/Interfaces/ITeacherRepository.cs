using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Common;
using Backend.DTOS.School.Teachers;

namespace Backend.Interfaces;

public interface ITeacherRepository
{
    Task<TeacherDTO> AddTeacherAsync(TeacherDTO teacher);
    Task<List<TeacherDTO>> GetAllTeachersAsync();
    Task<(List<TeacherDTO> Items, int TotalCount)> GetTeachersPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<(List<TeacherDTO> Items, int TotalCount)> GetTeachersPageWithFiltersAsync(int pageNumber, int pageSize, Dictionary<string, FilterValue> filters, CancellationToken cancellationToken = default);
    Task<TeacherDTO> UpdateTeacherAsync(int id, TeacherDTO teacher);
}
