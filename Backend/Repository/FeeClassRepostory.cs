using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;

using Backend.Models;
using Backend.Repository.IRepository;

using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class FeeClassRepostory : Repository<FeeClass>, IFeeClassRepository
{
    private readonly DatabaseContext _db;


    public FeeClassRepostory(DatabaseContext db) : base(db)
    {
        _db = db;

    }


    public async Task<bool> checkIfExist(int feeClassID)
    {
        var FeeClass = await _db.FeeClass.FirstOrDefaultAsync(fc => fc.FeeClassID == feeClassID);
        if (FeeClass == null)
            return false;
        return true;
    }



    public async Task UpdateAsync(FeeClass obj)
    {
        _db.FeeClass.Update(obj);
        await SaveAsync();
    }

}
