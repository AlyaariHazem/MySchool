using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Years;
using Backend.Models;

namespace Backend.Repository.School.Interfaces;

public interface IYearRepository : IgenericRepository<YearDTO>
{
    Task Add(YearDTO obj);
    Task Update(YearDTO obj);
    Task<List<YearDTO>> GetAll();
}
