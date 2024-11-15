using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Years;
using Backend.Models;
using Backend.Repository.School.Interfaces;

namespace Backend.Repository.School.Classes;

public class YearRepository : IYearRepository
{
    public void Add(YearDTO obj)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(int id)
    {
        throw new  NotImplementedException();
    }

    public async Task<Year> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }
}
