using Newtonsoft.Json;

namespace Backend.DTOS.School.Employee;

public class EmployeeDTO
{
    public int? EmployeeID { get; set; }
    public string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string LastName { get; set; }

    /// <summary>Teacher or Manager (camelCase JSON: <c>jopName</c>).</summary>
    public string JopName { get; set; }

    /// <summary>Maps <c>jobName</c> from clients onto <see cref="JopName"/>; omitted when serializing.</summary>
    [JsonProperty("jobName", NullValueHandling = NullValueHandling.Ignore)]
    public string? JobNameCompat
    {
        get => null;
        set
        {
            if (!string.IsNullOrWhiteSpace(value))
                JopName = value;
        }
    }

    public string? Address { get; set; }
    public string? Mobile { get; set; }
    public string? Gender { get; set; }
    public DateTime? DOB { get; set; }
    public DateTime? HireDate { get; set; }
    public string? Email { get; set; }
    public string? ImageURL { get; set; }
    public int? ManagerID { get; set; }

    /// <summary>Tenant <see cref="Models.School.SchoolID"/>; used when adding <see cref="Models.SchoolStaff"/> or <see cref="Models.Manager"/>.</summary>
    public int? SchoolID { get; set; }

    /// <summary>Initial password for new identity users (student, guardian, school staff). Optional — a random password is used if omitted.</summary>
    public string? Password { get; set; }

    /// <summary>Required when <see cref="JopName"/> is <c>Student</c>.</summary>
    public int? DivisionID { get; set; }

    /// <summary>Required when <see cref="JopName"/> is <c>Student</c> (existing guardian).</summary>
    public int? GuardianID { get; set; }
}
