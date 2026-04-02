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
}
