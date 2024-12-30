using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Backend.Models;

namespace Backend.Repository.IRepository;

public interface IGuardianRepository : IRepository<Guardian>
{

    Task UpdateAsync(Guardian guardian);
}
