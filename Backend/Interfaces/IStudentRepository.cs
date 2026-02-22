using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Common;
using Backend.Models;
using Backend.DTOS.School.Students;
using Backend.DTOS;

namespace Backend.Repository.School.Interfaces;

public interface IStudentRepository
{
    Task<Student> AddStudentAsync(Student student);
    Task<List<StudentDetailsDTO>> GetAllStudentsAsync();
    Task<(List<StudentDetailsDTO> Items, int TotalCount)> GetStudentsPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<(List<StudentDetailsDTO> Items, int TotalCount)> GetStudentsPageWithFiltersAsync(int pageNumber, int pageSize, Dictionary<string, Backend.Common.FilterValue> filters, CancellationToken cancellationToken = default);
    Task<StudentDetailsDTO?> GetStudentByIdAsync(int id);
    Task<Student?> GetStudentAsync(int id);
    Task<int> MaxValue();
    Task<bool> DeleteStudentAsync(int id);
    Task<GetStudentForUpdateDTO?> GetUpdateStudentWithGuardianRequestData(int studentData);
    Task<(List<UnregisteredStudentDTO> Items, int TotalCount)> GetUnregisteredStudentsAsync(int? targetYearID, int pageNumber, int pageSize, string? studentName, int? stageID, int? classID, CancellationToken cancellationToken = default);
        Task<PromoteStudentsResponseDTO> PromoteStudentsAsync(List<PromoteStudentRequestDTO> students, int? targetYearID = null, bool copyCoursePlansFromCurrentYear = false);
}
