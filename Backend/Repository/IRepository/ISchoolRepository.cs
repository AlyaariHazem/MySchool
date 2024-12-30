using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;


namespace Backend.Repository.IRepository;

public interface ISchoolRepository : IRepository<Backend.Models.School>
{

        Task UpdateAsync(Backend.Models.School school);
}
