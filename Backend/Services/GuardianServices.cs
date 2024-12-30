using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS.GuardiansDTO;

using Backend.Models;
using Backend.Repository;
using Backend.Repository.IRepository;
using Backend.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class GuardianServices : IGuardianServices
    {

        private readonly IGuardianRepository _guardianRepository;

        public GuardianServices(IGuardianRepository guardianRepository)
        {

            _guardianRepository = guardianRepository;
        }


        public async Task<Guardian> AddAsync(Guardian guardian)
        {
            await _guardianRepository.CreateAsync(guardian);
            return guardian;
        }

        public async Task<bool> DeleteAsync(int guardianId)
        {
            var guardian = await _guardianRepository.GetAsync(g => g.GuardianID == guardianId);
            if (guardian != null)
            {
                await _guardianRepository.RemoveAsync(guardian);
                return true;
            }
            return false;

        }


        public async Task<List<Guardian>?> GetAllAsync(Expression<Func<Guardian, bool>> filter = null)
        {
            var guardians = await _guardianRepository.GetAllAsync(filter);
            if (guardians == null)
            {
                return null;
            }
            return guardians;
        }

        public async Task<Guardian> GetAsync(Expression<Func<Guardian, bool>> filter)
        {
            var guardian = await _guardianRepository.GetAsync(filter);
            if (guardian == null)
            {
                return null;
            }
            return guardian;
        }




        public async Task<UpdateGuardianDTO> GetForUpdateAsync(int guardianId)
        {
            var guardian = await _guardianRepository.GetAsync(g => g.GuardianID == guardianId, includeProperties: "ApplicationUser");
            if (guardian == null)
            {
                return null;
            }
            var updateGuardianDTO = new UpdateGuardianDTO
            {
                GuardianID = guardian.GuardianID,
                GuardianFullName = guardian.FullName,
                Gender = guardian.ApplicationUser.Gender,
                Type = guardian.Type,
                UserID = guardian.UserID,
                GuardianAddress = guardian.ApplicationUser.Address!,
                GuardianDOB = guardian.GuardianDOB,
                GuardianEmail = guardian.ApplicationUser.Email!,
                GuardianPhone = guardian.ApplicationUser.PhoneNumber
            };
            return updateGuardianDTO;
        }

        public async Task<bool> UpdateAsync(UpdateGuardianDTO guardian)
        {
            var guardianExist = await _guardianRepository.GetAsync(g => g.GuardianID == guardian.GuardianID, tracked: false);
            if (guardianExist != null)
            {
                guardianExist.FullName = guardian.GuardianFullName;
                guardianExist.GuardianDOB = guardian.GuardianDOB;
                guardianExist.Type = guardian.Type;
                guardianExist.ApplicationUser.Address = guardian.GuardianAddress;
                guardianExist.ApplicationUser.Email = guardian.GuardianEmail;
                guardianExist.ApplicationUser.PhoneNumber = guardian.GuardianPhone;
                await _guardianRepository.UpdateAsync(guardianExist);
                return true;
            }
            return false;
        }


    }

}

