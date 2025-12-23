using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Common;
using Backend.DTOS.School.Years;
using Backend.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace Backend.Repository.School.Interfaces;

public interface IYearRepository : IgenericRepository<YearDTO>
{
    Task Add(YearDTO obj);
    Task Update(YearDTO obj);
    Task<List<YearDTO>> GetAll();
    Task<bool> UpdatePartial(int id, JsonPatchDocument<YearDTO> partialStage);
    Task<(List<YearDTO> Items, int TotalCount)> GetYearsPageWithFiltersAsync(int pageNumber, int pageSize, Dictionary<string, FilterValue> filters, CancellationToken cancellationToken = default);
}
