using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS;
using Backend.Models;

namespace Backend.Repository.School
{
    public interface IDivisionRepository : IgenericRepository<Division>
    {
       public List<DivisionDTO> DisplayDivisiones();
       public void ChangeState(int id, bool state);
       public void Add(DivisionDTO model);
       public void Update(DivisionDTO model);
    }
}