using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Backend.Models;

namespace Backend.Repository.IRepository;

public interface IFeesRepository : IRepository<Fee>
{

    Task UpdateAsync(Fee obj);


}
