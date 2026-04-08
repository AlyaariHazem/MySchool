using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS.School;
using school = Backend.Models.School; // Alias for the model class
using Backend.Repository.School.Implements;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Backend.DTOS.School.Years;
using Backend.Repository.School.Interfaces;

namespace Backend.Repository.School.Classes
{
    public class SchoolRepository : ISchoolRepository
    {
        private readonly TenantDbContext _db;
        private readonly DatabaseContext _masterDb;
        private readonly IMapper _mapper;
        private readonly IYearRepository _yearRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TenantInfo _tenantInfo;

        public SchoolRepository(
            TenantDbContext db,
            DatabaseContext masterDb,
            IYearRepository yearRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            TenantInfo tenantInfo)
        {
            _db = db;
            _masterDb = masterDb;
            _mapper = mapper;
            _yearRepository = yearRepository;
            _httpContextAccessor = httpContextAccessor;
            _tenantInfo = tenantInfo;
        }

        /// <summary>
        /// Platform admin listing schools without a JWT TenantId: catalog comes from master Tenants.
        /// In that mode, <see cref="SchoolDTO.SchoolID"/> in list/detail/update/delete refers to <see cref="Models.Tenant.TenantId"/>.
        /// </summary>
        private bool UseMasterSchoolCatalog()
        {
            if (!string.IsNullOrEmpty(_tenantInfo.ConnectionString))
                return false;

            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return false;

            if (user.IsInRole("ADMIN"))
                return true;

            var ut = user.FindFirst("UserType")?.Value;
            return string.Equals(ut, "ADMIN", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<TenantDbContext> CreateTenantDbForTenantIdAsync(int tenantId)
        {
            var row = await _masterDb.Tenants.AsNoTracking()
                .FirstOrDefaultAsync(t => t.TenantId == tenantId);
            if (row == null || string.IsNullOrWhiteSpace(row.ConnectionString))
                throw new KeyNotFoundException(
                    $"Tenant {tenantId} was not found or has no connection string in the master database.");

            var ti = new TenantInfo { TenantId = tenantId, ConnectionString = row.ConnectionString };
            var ob = new DbContextOptionsBuilder<TenantDbContext>();
            ob.UseSqlServer(row.ConnectionString, sql =>
            {
                sql.CommandTimeout(180);
                sql.MigrationsAssembly(typeof(TenantDbContext).Assembly.FullName);
            });
            return new TenantDbContext(ob.Options, ti);
        }

        public async Task<SchoolDTO> GetByIdAsync(int id)
        {
            if (UseMasterSchoolCatalog())
            {
                await using var db = await CreateTenantDbForTenantIdAsync(id);
                var school = await db.Schools.AsNoTracking().FirstOrDefaultAsync();
                if (school == null)
                    throw new KeyNotFoundException($"School for tenant {id} not found.");

                var schoolDTO = _mapper.Map<SchoolDTO>(school);
                schoolDTO.ImageURL = $"https://localhost:7258/uploads/School/School_" + school.SchoolID + "_" + schoolDTO.ImageURL;
                return schoolDTO;
            }

            var schools = await _db.Schools.FirstOrDefaultAsync(x => x.SchoolID == id);
            if (schools == null)
            {
                throw new KeyNotFoundException($"School with ID {id} not found.");
            }
            var dto = _mapper.Map<SchoolDTO>(schools);
            dto.ImageURL = $"https://localhost:7258/uploads/School/School_" + id + "_" + dto.ImageURL;
            return dto;
        }

        public async Task<List<SchoolDTO>> GetAllAsync()
        {
            if (UseMasterSchoolCatalog())
            {
                var tenants = await _masterDb.Tenants.AsNoTracking()
                    .OrderBy(t => t.SchoolName)
                    .ToListAsync();
                return tenants.Select(t => new SchoolDTO
                {
                    SchoolID = t.TenantId,
                    SchoolName = t.SchoolName ?? "",
                    SchoolNameEn = t.SchoolName ?? "",
                    HireDate = DateTime.UtcNow,
                    SchoolGoal = "",
                    Country = "",
                    City = "",
                    SchoolPhone = 0,
                    SchoolType = "",
                    Email = "",
                }).ToList();
            }

            var schools = await _db.Schools.ToListAsync();
            var schoolDTO = _mapper.Map<List<SchoolDTO>>(schools);
            return schoolDTO;
        }

        public async Task AddAsync(SchoolDTO school)
        {
            var newSchool = _mapper.Map<school>(school);
            await _db.Schools.AddAsync(newSchool);
            await _db.SaveChangesAsync();
            var currentYear = new YearDTO
            {
                YearDateStart = DateTime.Now,
                YearDateEnd = DateTime.Now.AddYears(1),
                HireDate = DateTime.Now,
                Active = true,
                SchoolID = newSchool.SchoolID
            };
            await _yearRepository.Add(currentYear);
        }

        public async Task UpdateAsync(SchoolDTO schoolDTO)
        {
            if (UseMasterSchoolCatalog())
            {
                if (schoolDTO.SchoolID is not int tenantId)
                    throw new ArgumentException("SchoolID (tenant id) is required for update.", nameof(schoolDTO));

                await using var db = await CreateTenantDbForTenantIdAsync(tenantId);
                var existingSchool = await db.Schools.FirstOrDefaultAsync();
                if (existingSchool == null)
                    throw new KeyNotFoundException($"School for tenant {tenantId} not found.");

                var realId = existingSchool.SchoolID;
                _mapper.Map(schoolDTO, existingSchool);
                existingSchool.SchoolID = realId;
                db.Schools.Update(existingSchool);
                await db.SaveChangesAsync();
                return;
            }

            var existing = await _db.Schools.FindAsync(schoolDTO.SchoolID);
            if (existing == null)
            {
                throw new KeyNotFoundException($"School with ID {schoolDTO.SchoolID} not found.");
            }

            _mapper.Map(schoolDTO, existing); // Update the entity with new values
            _db.Schools.Update(existing);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int schoolId)
        {
            if (UseMasterSchoolCatalog())
            {
                await using var db = await CreateTenantDbForTenantIdAsync(schoolId);
                var schoolToDelete = await db.Schools.FirstOrDefaultAsync();
                if (schoolToDelete == null)
                    throw new KeyNotFoundException($"School for tenant {schoolId} not found.");

                db.Schools.Remove(schoolToDelete);
                await db.SaveChangesAsync();
                return;
            }

            var school = await _db.Schools.FindAsync(schoolId);
            if (school == null)
            {
                throw new KeyNotFoundException($"School with ID {schoolId} not found.");
            }

            _db.Schools.Remove(school);
            await _db.SaveChangesAsync();
        }
    }
}
