using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS;
using Backend.DTOS.School.Stages;
using Backend.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace Backend.Repository.School
{
    public interface IClassesRepository:IgenericRepository<Class>
    {
        public Task Add(AddClassDTO obj);
        public Task Update(AddClassDTO obj);
        public Task<List<ClassDTO>> GetAllAsync();
        Task<bool> UpdatePartial(int id, JsonPatchDocument<UpdateClassDTO> partialClass);
    }
}