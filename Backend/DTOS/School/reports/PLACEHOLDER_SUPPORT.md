# Report Template Placeholder Support

## Overview
The Report Template system supports placeholders that can be replaced with actual data when generating reports. Placeholders follow the pattern `#PlaceholderName#`.

## Supported Placeholders

### Student Information
- `#FullName#` - Student's full name
- `#StudentId#` - Student ID number
- `#SchoolYear#` - Current school year (e.g., "2024-2025")
- `#ClassName#` - Student's class name
- `#DivisionName#` - Student's division name

### School Information
- `#SchoolName#` - School name
- `#SchoolAddress#` - School address
- `#SchoolPhone#` - School phone number

### Academic Information
- `#TermName#` - Current term name
- `#MonthName#` - Current month name
- `#Grade#` - Student's grade/score
- `#SubjectName#` - Subject name

### Date Information
- `#CurrentDate#` - Current date (formatted)
- `#ReportDate#` - Date when report is generated

## Example Template

```html
<div>
  <h1>Student Monthly Report</h1>
  <p><strong>Student:</strong> #FullName#</p>
  <p><strong>Class:</strong> #ClassName# - #DivisionName#</p>
  <p><strong>School Year:</strong> #SchoolYear#</p>
  <p><strong>Term:</strong> #TermName#</p>
  <p><strong>Month:</strong> #MonthName#</p>
  <p><strong>Grade:</strong> #Grade#</p>
  <p><strong>Date:</strong> #CurrentDate#</p>
</div>
```

## Where to Implement Replacement Logic

### Backend Implementation Location

The placeholder replacement logic should be implemented in:

**File:** `Backend/Repository/ReportRepository.cs` or a new service `Backend/Services/ReportTemplateProcessorService.cs`

**Recommended Approach:**

1. **Create a new service** `ReportTemplateProcessorService.cs`:
   ```csharp
   public class ReportTemplateProcessorService
   {
       public string ProcessTemplate(string templateHtml, ReportData data)
       {
           // Replace placeholders with actual data
           var processed = templateHtml;
           
           processed = processed.Replace("#FullName#", data.StudentFullName ?? "");
           processed = processed.Replace("#SchoolYear#", data.SchoolYear ?? "");
           // ... more replacements
           
           return processed;
       }
   }
   ```

2. **Create a DTO for report data:**
   ```csharp
   public class ReportData
   {
       public string? StudentFullName { get; set; }
       public string? SchoolYear { get; set; }
       public string? ClassName { get; set; }
       // ... more properties
   }
   ```

3. **Use in ReportRepository:**
   - When generating reports (e.g., in `MonthlyReportsAsync`), load the template
   - Process the template with actual data
   - Return the processed HTML

### Frontend Implementation (Alternative)

If you prefer client-side replacement:

**File:** `MySchool/src/app/components/school/core/services/report-template.service.ts`

Add a method:
```typescript
processTemplate(templateHtml: string, data: ReportData): string {
  let processed = templateHtml;
  processed = processed.replace(/#FullName#/g, data.studentFullName || '');
  processed = processed.replace(/#SchoolYear#/g, data.schoolYear || '');
  // ... more replacements
  return processed;
}
```

## Best Practices

1. **Case Sensitivity:** Placeholders are case-sensitive. Use consistent naming (PascalCase recommended).

2. **Null Handling:** Always provide fallback values (empty string or "N/A") when data is missing.

3. **Regex Pattern:** Use regex for more robust replacement:
   ```csharp
   var pattern = @"#(\w+)#";
   var regex = new Regex(pattern);
   processed = regex.Replace(templateHtml, match => {
       var placeholder = match.Groups[1].Value;
       return GetPlaceholderValue(placeholder, data) ?? "";
   });
   ```

4. **Security:** Since templates are sanitized before saving, processed templates are safe. However, ensure data values are also sanitized if they come from user input.

5. **Performance:** For large templates with many placeholders, consider caching processed templates or using StringBuilder for multiple replacements.

## Future Enhancements

1. **Conditional Logic:** Support `#IF#` / `#ENDIF#` blocks
2. **Loops:** Support `#FOREACH#` for lists (e.g., subjects, grades)
3. **Formatting:** Support format specifiers like `#Date:yyyy-MM-dd#`
4. **Validation:** Validate placeholder names when saving templates
5. **Preview:** Add preview functionality to see template with sample data
