using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS;
using Backend.DTOS.ClassesDTO;

using Backend.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace Backend.Repository.IRepository
{
    public interface IClassesRepository : IRepository<Class>
    {

        public Task UpdateAsync(Class obj);


    }
}