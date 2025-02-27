using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Tenant;
using Backend.Models;

namespace Backend.Repository.School.Implements;

public interface ITenantRepository
{
    Task AddAsync(TenantDTO tenant);
    Task Update(TenantDTO tenant);
    Task DeleteAsync(int id);
    Task<TenantDTO> GetByIdAsync(int id);
    Task<List<TenantDTO>> GetAll();
}
