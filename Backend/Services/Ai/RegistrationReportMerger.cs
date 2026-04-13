using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Backend.DTOS;
using Backend.DTOS.School.Students;

namespace Backend.Services.Ai;

/// <summary>
/// Merges REGISTRATION_FORM template HTML with student data — mirrors placeholder logic in
/// <c>registration-report.component.ts</c> (applyPlaceholders / replaceMap).
/// Integration: templates come from <see cref="Backend.Interfaces.IReportRepository.GetTemplateByCodeAsync"/> (code REGISTRATION_FORM).
/// </summary>
public static class RegistrationReportMerger
{
    /// <summary>Same default as Angular <c>defaultTemplate()</c> when API template is missing.</summary>
    public static string DefaultTemplateHtml => """
<div dir="rtl" style="line-height:1.8;padding:16px;font-family:Tajawal,Arial,sans-serif">
<p style="text-align:center">بسم الله الرحمن الرحيم</p>
<h2 style="text-align:center;color:#a86b00">استمارة تسجيل الطالب للعام الدراسي</h2>
<p><b>اسم الطالب:</b> #FullName#</p>
<p><b>المرحلة:</b> #PhaseName# &nbsp; <b>الشعبة:</b> #DivisionName#</p>
<p><b>الصف:</b> #ClassName# &nbsp; <b>العمر:</b> #Age#</p>
<p><b>النوع:</b> #Sex# &nbsp; <b>مكان الميلاد:</b> #Birthplace#</p>
<p><b>الهاتف:</b> #Phone# &nbsp; <b>العنوان:</b> #Address#</p>
</div>
""";

    public static string Merge(
        string templateHtml,
        GetStudentForUpdateDTO student,
        DivisionDTO? division,
        string schoolYearText)
    {
        var html = string.IsNullOrWhiteSpace(templateHtml) ? DefaultTemplateHtml : templateHtml;

        var fn = $"{student.StudentFirstName} {student.StudentMiddleName} {student.StudentLastName}"
            .Replace("  ", " ", StringComparison.Ordinal)
            .Trim();

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["FullName"] = fn,
            ["StudentId"] = student.StudentID.ToString(CultureInfo.InvariantCulture),
            ["SID"] = student.StudentID.ToString(CultureInfo.InvariantCulture),
            ["ClassName"] = division?.ClassesName ?? string.Empty,
            ["DivisionName"] = division?.DivisionName ?? string.Empty,
            ["PhaseName"] = division?.StageName ?? string.Empty,
            ["SchoolYear"] = schoolYearText,
            ["Age"] = AgeFromDob(student.StudentDOB)?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            ["Sex"] = GenderAr(student.StudentGender),
            ["Birthplace"] = student.PlaceBirth ?? string.Empty,
            ["Phone"] = student.StudentPhone ?? string.Empty,
            ["Address"] = student.StudentAddress ?? string.Empty,
        };

        return ReplaceMap(html, map);
    }

    private static string ReplaceMap(string html, IReadOnlyDictionary<string, string> map)
    {
        var outHtml = html;
        foreach (var kv in map)
        {
            var re = new Regex("#" + Regex.Escape(kv.Key) + "#", RegexOptions.IgnoreCase);
            outHtml = re.Replace(outHtml, kv.Value ?? string.Empty);
        }

        return outHtml;
    }

    private static int? AgeFromDob(DateTime dob)
    {
        if (dob == default || dob == DateTime.MinValue)
            return null;
        var diff = DateTime.UtcNow.Date - dob.Date;
        if (diff.TotalDays < 0)
            return null;
        return (int)(diff.TotalDays / 365.25);
    }

    private static string GenderAr(string? g)
    {
        if (string.IsNullOrWhiteSpace(g))
            return string.Empty;
        var x = g.Trim().ToLowerInvariant();
        if (x is "male" or "m" or "ذكر")
            return "ذكر";
        if (x is "female" or "f" or "أنثى")
            return "أنثى";
        return g;
    }
}
