using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Backend.Data;
using Backend.DTOS;
using Backend.DTOS.ClassesDTO;

using Backend.Models;
using Backend.Repository.IRepository;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository
{
    public class ClassesRepository : Repository<Class>, IClassesRepository
    {
        private readonly DatabaseContext _db;

        public ClassesRepository(DatabaseContext db) : base(db)
        {
            _db = db;

        }



        public async Task UpdateAsync(Class obj)
        {
            _db.Classes.Update(obj);
            await SaveAsync();
        }

        public Task<bool> UpdatePartial(int id, JsonPatchDocument<UpdateClassDTO> partialClass)
        {
            throw new NotImplementedException();
        }
    }
}
