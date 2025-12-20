using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS.School.GradeTypes;
using Backend.Interfaces;
using Backend.Models; // Assuming your GradeType model is inside this namespace
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository
{
    public class GradeTypesRepository : IGradeTypesRepository
    {
        private readonly TenantDbContext _context;
        private readonly IMapper _mapper;

        public GradeTypesRepository(TenantDbContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        // Add a new grade type
        public async Task<GradeTypesDTO> AddAsync(GradeTypesDTO gradeType)
        {
            if (gradeType == null)
                throw new ArgumentNullException(nameof(gradeType), "GradeType DTO cannot be null.");

            var entity = _mapper.Map<GradeType>(gradeType);

            _context.GradeTypes.Add(entity);
            await _context.SaveChangesAsync();

            gradeType.GradeTypeID = entity.GradeTypeID; // Assuming GradeTypesDTO has an Id property
            return gradeType;
        }

        // Delete a grade type by Id
        public async Task DeleteAsync(int id)
        {
            var gradeType = await _context.GradeTypes.FindAsync(id);
            if (gradeType == null)
                throw new KeyNotFoundException("GradeType not found.");

            _context.GradeTypes.Remove(gradeType);
            await _context.SaveChangesAsync();
        }

        // Get all grade types
        public async Task<List<GradeTypesDTO>> GetAllAsync()
        {
            var gradeTypes = await _context.GradeTypes.ToListAsync();
            return gradeTypes.Select(g => new GradeTypesDTO
            {
                GradeTypeID = g.GradeTypeID,
                Name = g.Name,
                MaxGrade = g.MaxGrade,
                IsActive=g.IsActive
            }).ToList();
        }

        // Get a grade type by Id
        public async Task<GradeTypesDTO?> GetByIdAsync(int id)
        {
            var gradeType = await _context.GradeTypes.FindAsync(id);
            if (gradeType == null)
                return null;

            return _mapper.Map<GradeTypesDTO>(gradeType);
        }

        // Update an existing grade type
        public async Task UpdateAsync(GradeTypesDTO gradeType)
        {
            if (gradeType == null)
                throw new ArgumentNullException(nameof(gradeType), "GradeType DTO cannot be null.");

            var existingGradeType = await _context.GradeTypes.FindAsync(gradeType.GradeTypeID);
            if (existingGradeType == null)
                throw new KeyNotFoundException("GradeType not found.");

            existingGradeType.Name = gradeType.Name;
            existingGradeType.MaxGrade = gradeType.MaxGrade;
            existingGradeType.IsActive = gradeType.IsActive;

            _context.GradeTypes.Update(existingGradeType);
            await _context.SaveChangesAsync();
        }
    }
}
