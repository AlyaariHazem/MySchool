
using Backend.DTOS.StudentClassFeesDTO;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOS.StudentsDTO
{
    public class UpdateStudentWithGuardianRequestDTO
    {
        [Required]
        public int StudentID { get; set; }

        // Student Fields
        public string StudentEmail { get; set; }
        public string StudentPhone { get; set; }
        public string StudentAddress { get; set; }
        public string StudentPassword { get; set; }

        public string StudentFirstName { get; set; }
        public string StudentMiddleName { get; set; }
        public string StudentLastName { get; set; }
        public string StudentFirstNameEng { get; set; }
        public string StudentMiddleNameEng { get; set; }
        public string StudentLastNameEng { get; set; }
        public int DivisionID { get; set; }
        public string PlaceBirth { get; set; }
        public DateTime StudentDOB { get; set; }
        public string? StudentImageURL { get; set; }
        public string? StudentGender { get; set; }
        public DateTime HireDate { get; set; } = DateTime.Now;

        // Guardian Fields
        public int GuardianID { get; set; }
        public int? ExistingGuardianId { get; set; }
        public string GuardianEmail { get; set; }
        public string GuardianPhone { get; set; }
        public string GuardianAddress { get; set; }
        public string GuardianFullName { get; set; }
        public string GuardianGender { get; set; }
        public DateTime GuardianDOB { get; set; }
        public string GuardianType { get; set; }

        // Attachments
        public List<string>? Attachments { get; set; }
        public List<IFormFile>? Files { get; set; }

        // Discounts
        public List<StudentClassFeeDTO>? UpdateDiscounts { get; set; }
    }
}
