using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Students;

   public class UpdateStudentRequest
    {
        [Required]
        public int StudentID { get; set; }

        // Guardian Details
        public string GuardianEmail { get; set; }
        public string GuardianPassword { get; set; }
        public string GuardianAddress { get; set; }
        public string GuardianGender { get; set; }
        public string GuardianFullName { get; set; }
        public string GuardianType { get; set; }
        public string GuardianPhone { get; set; }
        public DateTime? GuardianDOB { get; set; }

        // Student Details
        public string StudentEmail { get; set; }
        public string StudentPassword { get; set; }
        public string StudentAddress { get; set; }
        public string StudentGender { get; set; }
        public string StudentFirstName { get; set; }
        public string StudentMiddleName { get; set; }
        public string StudentLastName { get; set; }
        public string StudentFirstNameEng { get; set; }
        public string StudentMiddleNameEng { get; set; }
        public string StudentLastNameEng { get; set; }
        public string StudentImageURL { get; set; }
        public int? DivisionID { get; set; }
        public string PlaceBirth { get; set; }
        public string StudentPhone { get; set; }
        public DateTime? StudentDOB { get; set; }
        public DateTime? HireDate { get; set; }

        // Attachments and Discounts
        public List<string> Attachments { get; set; }
        public List<UpdateStudentClassFeeDTO> Discounts { get; set; }
    }

    public class UpdateStudentClassFeeDTO
    {
        [Required]
        public int FeeClassID { get; set; }
        public decimal? AmountDiscount { get; set; }
        public string NoteDiscount { get; set; }
    }