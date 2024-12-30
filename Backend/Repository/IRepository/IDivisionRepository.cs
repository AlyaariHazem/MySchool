using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS;
using Backend.DTOS.DivisionsDTO;

using Backend.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace Backend.Repository.IRepository
{
    public interface IDivisionRepository : IRepository<Division>
    {


        public Task UpdateAsync(Division obj);
        Task UpdatePartial(int id, JsonPatchDocument<UpdateDivisionDTO> partialClass);
    }
}