using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS.School.Tenant;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School.Classes;

public class TenantRepository : ITenantRepository
{
    private readonly DatabaseContext _context;
    private readonly IMapper _mapper;
    
    public TenantRepository(DatabaseContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task AddAsync(TenantDTO tenant)
    {
        if (tenant == null)
         throw new ArgumentNullException(nameof(TenantDTO), "The Tenant cannot be null.");

        var newTenant = new Tenant
        {
            SchoolName = tenant.SchoolName,
            ConnectionString = tenant.ConnectionString
        };
        await _context.Tenants.AddAsync(newTenant);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t=>t.TenantId == id);
        if (tenant == null)
        throw new ArgumentNullException(nameof(Tenant), "The Tenant cannot be null.");

        _context.Tenants.Remove(tenant);
        await _context.SaveChangesAsync();
    }

    public async Task<List<TenantDTO>> GetAll()
    {
        var tenants = await _context.Tenants.ToListAsync();

        if (tenants == null)
            throw new ArgumentNullException(nameof(Tenant), "The Tenant cannot be null.");

        var tenantsDTO = _mapper.Map<List<TenantDTO>>(tenants);
        return tenantsDTO;
    }

    public async Task<TenantDTO> GetByIdAsync(int id)
    {
        var tenant = await _context.Tenants.FirstOrDefaultAsync(x => x.TenantId == id);
        if (tenant == null)
            throw new ArgumentNullException(nameof(Tenant), "The Tenant cannot be null.");

        var tenantDTO = _mapper.Map<TenantDTO>(tenant);
        return tenantDTO;
    }

    public async Task Update(TenantDTO tenant)
    {
        var tenantEntity = await _context.Tenants.FirstOrDefaultAsync(x => x.TenantId == tenant.TenantID);
        if (tenantEntity == null)
            throw new ArgumentNullException(nameof(Tenant), "The Tenant cannot be null.");

        tenantEntity.SchoolName = tenant.SchoolName;
        tenantEntity.ConnectionString = tenant.ConnectionString;

        await _context.SaveChangesAsync();
    }
}
