using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.IdentityDTO
{
    public class IdentityResponseDTO
    {
        public UserDTO User { get; set; }
        public string Token { get; set; }
    }
}