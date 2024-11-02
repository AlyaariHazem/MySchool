using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS;
using Backend.Models;

namespace Backend.Repository.School
{
    public interface IClassesRepository:IgenericRepository<Class>
    {
        public void Add(AddClassDTO obj);
        public void Update(AddClassDTO obj);
        public List<AddClassDTO> DisplayClasses();
    }
}