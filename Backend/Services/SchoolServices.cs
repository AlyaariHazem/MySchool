using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using school = Backend.Models.School; // Alias for the model class

using Microsoft.EntityFrameworkCore;
using Backend.Services.IServices;
using Backend.Repository.IRepository;
using System.Linq.Expressions;
using Backend.DTOS.SchoolsDTO;

namespace Backend.Services
{
    public class SchoolServices : ISchoolServices
    {


        private readonly ISchoolRepository _schoolRepository;

        public SchoolServices(ISchoolRepository schoolRepository)
        {

            _schoolRepository = schoolRepository;
        }
        public async Task<List<SchoolDTO>> GetAllAsync(Expression<Func<school, bool>> filter = null)
        {

            var schools = await _schoolRepository.GetAllAsync(filter);
            var schoolsDTO = schools.Select(school => new SchoolDTO()
            {
                SchoolID = school.SchoolID,
                SchoolName = school.SchoolName,
                SchoolPhone = school.SchoolPhone,
                SchoolType = school.SchoolType,
                Email = school.Email,
                SchoolNameEn = school.SchoolNameEn,
                SchoolGoal = school.SchoolGoal,
                SchoolMission = school.SchoolMission,
                SchoolVison = school.SchoolVison,
                City = school.City,
                Country = school.Country,
                zone = school.zone,
                fax = school.fax,
                HireDate = school.HireDate,
                Notes = school.Notes,
                Street = school.Street
            }).ToList();
            return schoolsDTO;

        }
        public async Task<SchoolDTO> GetAsync(Expression<Func<school, bool>> filter)
        {
            var school = await _schoolRepository.GetAsync(filter);
            var schoolsDTO = new SchoolDTO()
            {
                SchoolID = school.SchoolID,
                SchoolName = school.SchoolName,
                SchoolPhone = school.SchoolPhone,
                SchoolType = school.SchoolType,
                Email = school.Email,
                SchoolNameEn = school.SchoolNameEn,
                SchoolGoal = school.SchoolGoal,
                SchoolMission = school.SchoolMission,
                SchoolVison = school.SchoolVison,
                City = school.City,
                Country = school.Country,
                zone = school.zone,
                fax = school.fax,
                HireDate = school.HireDate,
                Notes = school.Notes,
                Street = school.Street



            };
            return schoolsDTO;
        }

        public async Task<bool> AddAsync(SchoolDTO school)
        {
            var newSchool = new school()
            {

                SchoolName = school.SchoolName,
                SchoolPhone = school.SchoolPhone,
                SchoolType = school.SchoolType,
                Email = school.Email,
                SchoolNameEn = school.SchoolNameEn,
                SchoolGoal = school.SchoolGoal,
                SchoolMission = school.SchoolMission,
                SchoolVison = school.SchoolVison,
                City = school.City,
                Country = school.Country,
                zone = school.zone,
                fax = school.fax,
                HireDate = school.HireDate,
                Notes = school.Notes,
                Street = school.Street
            };
            await _schoolRepository.CreateAsync(newSchool);
            return true;
        }

        public async Task<bool> UpdateAsync(SchoolDTO schoolDTO)
        {
            var existingSchool = await _schoolRepository.GetAsync(s => s.SchoolID == schoolDTO.SchoolID, tracked: false);
            if (existingSchool == null)
            {
                return false;
            }

            existingSchool.SchoolName = schoolDTO.SchoolName;
            existingSchool.SchoolPhone = schoolDTO.SchoolPhone;
            existingSchool.SchoolType = schoolDTO.SchoolType;
            existingSchool.Email = schoolDTO.Email;
            existingSchool.SchoolNameEn = schoolDTO.SchoolNameEn;
            existingSchool.SchoolGoal = schoolDTO.SchoolGoal;
            existingSchool.SchoolMission = schoolDTO.SchoolMission;
            existingSchool.SchoolVison = schoolDTO.SchoolVison;
            existingSchool.City = schoolDTO.City;
            existingSchool.Country = schoolDTO.Country;
            existingSchool.zone = schoolDTO.zone;
            existingSchool.fax = schoolDTO.fax;
            existingSchool.HireDate = schoolDTO.HireDate;
            existingSchool.Notes = schoolDTO.Notes;
            existingSchool.Street = schoolDTO.Street;


            await _schoolRepository.UpdateAsync(existingSchool);
            return true;
        }

        public async Task<bool> DeleteAsync(int schoolId)
        {
            var schoolToDelete = await _schoolRepository.GetAsync(s => s.SchoolID == schoolId);
            if (schoolToDelete == null)
            {
                return false;
            }

            await _schoolRepository.RemoveAsync(schoolToDelete);
            return true;
        }
    }
}
