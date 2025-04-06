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
    Task<GetManagerDTO> GetManager(int id);
    Task<string> AddManager(AddManagerDTO manager);
    Task UpdateManager(GetManagerDTO manager);
    Task DeleteManager(int id);
}
