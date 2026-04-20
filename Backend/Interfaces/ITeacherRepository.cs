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
    Task<int?> GetTeacherIdByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>Resolves the employee profile id for the signed-in teacher (profile <c>UserId</c> or linked <c>TeacherID</c>).</summary>
    Task<int?> GetEmployeeProfileIdForTeacherUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<TeacherDTO> AddTeacherAsync(TeacherDTO teacher);
    Task<List<TeacherDTO>> GetAllTeachersAsync();
    Task<(List<TeacherDTO> Items, int TotalCount)> GetTeachersPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<(List<TeacherDTO> Items, int TotalCount)> GetTeachersPageWithFiltersAsync(int pageNumber, int pageSize, Dictionary<string, FilterValue> filters, CancellationToken cancellationToken = default);
    Task<TeacherDTO> UpdateTeacherAsync(int id, TeacherDTO teacher);

    /// <summary>Paged id + display name for teachers in the active year scope (same as list paging).</summary>
    Task<PagedResult<TeacherNameLookupDto>> GetTeacherNamesPageAsync(int pageIndex, int pageSize, string? search, CancellationToken cancellationToken = default);

    /// <summary>Display name for a teacher by id (not year-filtered) for hydrating existing selections.</summary>
    Task<TeacherNameLookupDto?> GetTeacherNameLookupAsync(int teacherId, CancellationToken cancellationToken = default);
}
