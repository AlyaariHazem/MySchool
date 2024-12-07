using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS;

public class AddStudentWithGuardianRequest
{
     // Guardian details
    public string GuardianUserName { get; set; }
    public string GuardianEmail { get; set; }
    public string GuardianPassword { get; set; }
    public string GuardianAddress { get; set; }
    public string GuardianGender { get; set; }
    public string GuardianFullName { get; set; }
    public string? GuardianType { get; set; }

    // Student details
    public string StudentUserName { get; set; }
    public string StudentEmail { get; set; }
    public string StudentPassword { get; set; }
    public string StudentAddress { get; set; }
    public string StudentGender { get; set; }
    public string StudentFirstName { get; set; }
    public string? StudentMiddleName { get; set; }
    public string StudentLastName { get; set; }
    public string? StudentFirstNameEng { get; set; }
    public string? StudentMiddleNameEng { get; set; }
    public string? StudentLastNameEng { get; set; }
    public int DivisionID { get; set; }
    public string? PlaceBirth { get; set; }
    public decimal Amount { get; set; } = 0;
    //
    }
