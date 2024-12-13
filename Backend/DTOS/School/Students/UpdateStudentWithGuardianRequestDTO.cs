using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Students;

public class UpdateStudentWithGuardianRequestDTO
{
  public int StudentId { get; set; }
    public string StudentFirstName { get; set; }
    public string StudentMiddleName { get; set; }
    public string StudentLastName { get; set; }
    public string StudentFirstNameEng { get; set; }
    public string StudentMiddleNameEng { get; set; }
    public string StudentLastNameEng { get; set; }
    public string StudentEmail { get; set; }
    public string StudentAddress { get; set; }
    public string StudentGender { get; set; }
    public DateTime StudentDOB { get; set; }
    public string StudentPhone { get; set; }
    public int DivisionID { get; set; }
    public string PlaceBirth { get; set; }

    public string GuardianFullName { get; set; }
    public string GuardianType { get; set; }
    public string GuardianEmail { get; set; }
    public string GuardianAddress { get; set; }
    public string GuardianGender { get; set; }
    public DateTime GuardianDOB { get; set; }
    public string GuardianPhone { get; set; }

    public List<string> Attachments { get; set; }
    public List<DiscountRequest> Discounts { get; set; }   
}

public class DiscountRequest
{
    public int ClassID { get; set; }
    public int FeeID { get; set; }
    public decimal AmountDiscount { get; set; }
    public string NoteDiscount { get; set; }
}