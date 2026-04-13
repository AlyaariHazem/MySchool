namespace Backend.DTOS.School.reports;

/// <summary>
/// A merge field name for report templates (inserted as #Name# in HTML).
/// </summary>
public class ReportTemplatePlaceholderDto
{
    public string Name { get; set; } = "";
    /// <summary>Optional short description for UI tooltips.</summary>
    public string? Description { get; set; }
}
