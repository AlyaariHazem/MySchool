using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS;
using Backend.DTOS.IdentityDTO;
using Backend.Models;

namespace Backend.Services.IServices
{
    public interface IAuthServices
    {
        Task<Accounts> AddAccountAsync(Accounts account);
        Task<IdentityResponseDTO> Login(LoginDTO login);
        Task<IdentityResponseDTO> Register(RegisterDTO register);


    }

}

