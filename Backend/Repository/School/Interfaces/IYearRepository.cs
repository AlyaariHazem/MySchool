using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Years;
using Backend.Models;

namespace Backend.Repository.School.Interfaces;

public interface IYearRepository:IgenericRepository<Year>
{
    public void Add(YearDTO obj);
}
