using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.RegisterStudentsDTO
{
    public class RegisterStudentDTO
    {
        public string? StudentEmail { get; set; }
        public string StudentPassword { get; set; } = "123456";
        public string StudentAddress { get; set; }
        public string StudentGender { get; set; }
        public string StudentFirstName { get; set; }
        public string? StudentMiddleName { get; set; }
        public string StudentLastName { get; set; }
        public string? StudentFirstNameEng { get; set; }
        public string? StudentMiddleNameEng { get; set; }
        public string? StudentLastNameEng { get; set; }
        public string? StudentImageURL { get; set; }
        public int GuardianID { get; set; }
        public int DivisionID { get; set; }
        public int? ClassID { get; set; }
        public string? PlaceBirth { get; set; }
        public string StudentPhone { get; set; } = string.Empty;
        public DateTime StudentDOB { get; set; }
        public DateTime HireDate { get; set; } = DateTime.Now;

    }
}