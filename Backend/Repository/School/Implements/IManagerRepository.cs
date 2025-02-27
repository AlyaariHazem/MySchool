using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Manager;
using Backend.Models;

namespace Backend.Repository.School.Implements;

public interface IManagerRepository
{
    Task<List<GetManagerDTO>> GetManagers();
    Task<ManagerDTO> GetManager(int id);
    Task<string> AddManager(AddManagerDTO manager);
    Task UpdateManager(ManagerDTO manager);
    Task DeleteManager(int id);
}
