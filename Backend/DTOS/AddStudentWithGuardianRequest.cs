using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS;

public class AddStudentWithGuardianRequest
{
    public int? ExistingGuardianId { get; set; }
    public int StudentID { get; set; }
     // Guardian details
    public string GuardianEmail { get; set; }
    public string GuardianPassword { get; set; } = string.Empty;
    public string GuardianAddress { get; set; }
    public string GuardianGender { get; set; } = "Guardian";
    public string GuardianFullName { get; set; }
    public string? GuardianType { get; set; }
    public string GuardianPhone { get; set; } = string.Empty;
    public DateTime? GuardianDOB { get; set; }

    // Student details
    public string? StudentEmail { get; set; }
    public string StudentPassword { get; set; }=string.Empty;
    public string StudentAddress { get; set; }
    public string StudentGender { get; set; }
    public string StudentFirstName { get; set; }
    public string? StudentMiddleName { get; set; }
    public string StudentLastName { get; set; }
    public string? StudentFirstNameEng { get; set; }
    public string? StudentMiddleNameEng { get; set; }
    public string? StudentLastNameEng { get; set; }
    public string? StudentImageURL { get; set; }
    public int DivisionID { get; set; }
    public string? PlaceBirth { get; set; }
     public string StudentPhone { get; set; } = string.Empty;
    public DateTime StudentDOB { get; set; }
    public DateTime HireDate { get; set; }= DateTime.Now;
    public List<IFormFile>? Files { get; set; }// I want to use this in angular how can I handle it?

    public decimal Amount { get; set; } = 0;
    public List<string>? Attachments { get; set; }
    public List<DisCount>? Discounts { get; set; }

    //
    }

public class DisCount{
    public int FeeClassID  { get; set; }
    public decimal? AmountDiscount { get; set; }
    public string? NoteDiscount { get; set; }
}