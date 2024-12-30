using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Backend.DTOS;
using Backend.DTOS.ClassesDTO;

using Backend.Models;
using Backend.Repository;
using Microsoft.AspNetCore.JsonPatch;

namespace Backend.Services.IServices
{
    public interface IClassesServices
    {
        Task<bool> AddAsync(AddClassDTO obj);
        Task<bool> UpdateAsync(int Id, UpdateClassDTO obj);
        Task<List<ClassDTO>> GetAllAsync(Expression<Func<Class, bool>> filter = null);
        Task<ClassDTO> GetAsync(Expression<Func<Class, bool>> filter);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdatePartialAsync(int id, JsonPatchDocument<UpdateClassDTO> partialClass);
    }
}