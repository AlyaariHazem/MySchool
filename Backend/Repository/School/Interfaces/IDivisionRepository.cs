using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS;
using Backend.DTOS.School.Stages;
using Backend.Models;

namespace Backend.Repository.School
{
    public interface IDivisionRepository : IgenericRepository<Division>
    {
       public Task<List<DivisionDTO>> GetAll();
       public Task<bool> Add(AddDivisionDTO model);
       public Task<bool> Update(DivisionDTO model);
    }
}