using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.RegisterStudentsDTO
{
    public class RegisterResult
    {
        public bool IsSuccess { get; set; }

        public int StudentId { get; set; }

        public string Error { get; set; }
    }
}