using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS;
using Backend.DTOS.School.Classes;
using Backend.DTOS.School.Stages;
using Backend.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace Backend.Repository.School
{
    public interface IClassesRepository:IgenericRepository<Class>
    {
        Task Add(AddClassDTO obj);
        Task Update(AddClassDTO obj);
        Task<List<ClassDTO>> GetAllAsync();
        Task<List<AllClassesDTO>> GetAllNamesAsync();
        Task<bool> UpdatePartial(int id, JsonPatchDocument<UpdateClassDTO> partialClass);
    }
}