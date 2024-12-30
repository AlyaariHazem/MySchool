using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS;
using Backend.DTOS.DivisionsDTO;

using Backend.Models;
using Backend.Repository;
using Backend.Repository.IRepository;
using Backend.Repository.School;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository
{
    public class DivisionRepository : Repository<Division>, IDivisionRepository
    {
        private readonly DatabaseContext _db;

        public DivisionRepository(DatabaseContext db) : base(db)
        {
            _db = db;

        }

        public async Task UpdateAsync(Division obj)
        {
            _db.Divisions.Update(obj);
            await SaveAsync();
        }

        public Task UpdatePartial(int id, JsonPatchDocument<UpdateDivisionDTO> partialClass)
        {
            throw new NotImplementedException();
        }

        /*         public async Task<bool> UpdatePartial(int id, JsonPatchDocument<UpdateDivisionDTO> partialDivision)
                {
                       if (partialDivision == null || id == 0)
                        return false;

                    // Retrieve the Class entity by its ID
                    var division = await _db.Divisions.SingleOrDefaultAsync(s => s.DivisionID == id);
                    if (division == null)
                        return false;

                    // Map the Class entity to the DTO (this will be modified)
                    var divisionDTO = _mapper.Map<UpdateDivisionDTO>(division);

                    // Apply the patch to the DTO
                    partialDivision.ApplyTo(divisionDTO);

                    // Map the patched DTO back to the entity (class)
                    _mapper.Map(divisionDTO, division);

                    // Mark the entity as modified and save changes
                    _db.Entry(division).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                    return true;
                }


                } */

    }
}
