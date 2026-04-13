using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Backend.Data;
using Backend.DTOS.School.Students;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Services.Ai;

/// <summary>
/// Validates and executes AI tool calls against tenant data — no direct DB writes; read-only operations only.
/// Reuses <see cref="IUnitOfWork"/> (same as controllers) and mirrors registration merge with
/// <see cref="RegistrationReportMerger"/> + report templates from <see cref="IReportRepository"/>.
/// </summary>
public sealed class SchoolAiToolsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantDbContext _tenantDb;
    private readonly HtmlSanitizationService _htmlSanitizer;
    private readonly ILogger<SchoolAiToolsService> _logger;

    public SchoolAiToolsService(
        IUnitOfWork unitOfWork,
        TenantDbContext tenantDb,
        HtmlSanitizationService htmlSanitizer,
        ILogger<SchoolAiToolsService> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantDb = tenantDb;
        _htmlSanitizer = htmlSanitizer;
        _logger = logger;
    }

    public async Task<string> ExecuteAsync(string toolName, string argumentsJson, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AI tool {Tool} invoked (args length {Len})", toolName, argumentsJson?.Length ?? 0);
        try
        {
            var argsJson = argumentsJson ?? "{}";
            return toolName switch
            {
                "search_student" => await SearchStudentAsync(argsJson, cancellationToken),
                "get_student_by_id" => await GetStudentByIdAsync(argsJson, cancellationToken),
                "generate_student_registration_report" => await GenerateRegistrationReportAsync(argsJson, cancellationToken),
                "summarize_student_profile" => await SummarizeStudentProfileAsync(argsJson, cancellationToken),
                "draft_parent_message" => await DraftParentMessageAsync(argsJson, cancellationToken),
                "search_attendance" => await SearchAttendanceAsync(argsJson, cancellationToken),
                _ => JsonSerializer.Serialize(new { ok = false, error = $"Unknown tool: {toolName}" })
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI tool {Tool} failed", toolName);
            return JsonSerializer.Serialize(new { ok = false, error = "Tool failed.", detail = ex.Message });
        }
    }

    private static JsonSerializerOptions JsonOpts => new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private async Task<string> SearchStudentAsync(string argumentsJson, CancellationToken cancellationToken)
    {
        var args = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson).RootElement;
        var query = args.TryGetProperty("query", out var qEl) ? qEl.GetString()?.Trim() : null;
        if (string.IsNullOrEmpty(query))
            return JsonSerializer.Serialize(new { ok = false, message = "query is required.", students = Array.Empty<object>() }, JsonOpts);

        var req = new StudentNameIdSearchRequestDTO { PageNumber = 1, PageSize = 15 };
        var digits = query.Replace(" ", "", StringComparison.Ordinal);
        if (digits.All(char.IsDigit) && int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sid) && sid > 0)
            req.StudentID = sid;
        else
            req.FullName = query;

        var (items, total) = await _unitOfWork.Students.GetStudentNamesAndIdsPagedAsync(req);
        var list = items.Select(x => new { studentId = x.StudentID, fullName = x.FullName }).ToList();

        var ambiguous = total > 1 && req.StudentID == null;
        return JsonSerializer.Serialize(new
        {
            ok = true,
            totalCount = total,
            ambiguous,
            hint = ambiguous ? "Ask the user to pick a student id or refine the name." : (string?)null,
            students = list
        }, JsonOpts);
    }

    private async Task<string> GetStudentByIdAsync(string argumentsJson, CancellationToken cancellationToken)
    {
        var args = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson).RootElement;
        if (!args.TryGetProperty("studentId", out var sidEl) || !sidEl.TryGetInt32(out var studentId) || studentId <= 0)
            return JsonSerializer.Serialize(new { ok = false, error = "studentId is required." }, JsonOpts);

        var data = await _unitOfWork.Students.GetUpdateStudentWithGuardianRequestData(studentId);
        if (data == null)
            return JsonSerializer.Serialize(new { ok = false, error = $"No student found for id {studentId}." }, JsonOpts);

        var divisions = await _unitOfWork.Divisions.GetAll();
        var div = divisions.FirstOrDefault(d => d.DivisionID == data.DivisionID);

        // Do not expose passwords or empty credential fields.
        var payload = new
        {
            ok = true,
            student = new
            {
                data.StudentID,
                name = new { data.StudentFirstName, data.StudentMiddleName, data.StudentLastName },
                data.StudentGender,
                studentDob = data.StudentDOB,
                data.PlaceBirth,
                data.StudentPhone,
                data.StudentAddress,
                data.StudentEmail,
                division = div == null
                    ? null
                    : new { div.DivisionID, div.DivisionName, className = div.ClassesName, phaseName = div.StageName },
                guardian = new
                {
                    data.GuardianFullName,
                    data.GuardianPhone,
                    data.GuardianEmail,
                    data.GuardianAddress
                }
            }
        };

        return JsonSerializer.Serialize(payload, JsonOpts);
    }

    private async Task<string> GenerateRegistrationReportAsync(string argumentsJson, CancellationToken cancellationToken)
    {
        var args = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson).RootElement;
        if (!args.TryGetProperty("studentId", out var sidEl) || !sidEl.TryGetInt32(out var studentId) || studentId <= 0)
            return JsonSerializer.Serialize(new { ok = false, error = "studentId is required." }, JsonOpts);

        var data = await _unitOfWork.Students.GetUpdateStudentWithGuardianRequestData(studentId);
        if (data == null)
            return JsonSerializer.Serialize(new { ok = false, error = $"No student found for id {studentId}." }, JsonOpts);

        var divisions = await _unitOfWork.Divisions.GetAll();
        var div = divisions.FirstOrDefault(d => d.DivisionID == data.DivisionID);

        var schools = await _unitOfWork.Schools.GetAllAsync();
        var schoolId = schools.FirstOrDefault()?.SchoolID;

        var templateResult = await _unitOfWork.Reports.GetTemplateByCodeAsync("REGISTRATION_FORM", schoolId);
        var templateHtml = templateResult.Ok && templateResult.Value != null
            ? templateResult.Value.TemplateHtml ?? string.Empty
            : string.Empty;

        var yearText = await GetActiveSchoolYearLabelAsync(cancellationToken);
        var merged = RegistrationReportMerger.Merge(templateHtml, data, div, yearText);
        var safe = _htmlSanitizer.Sanitize(merged);

        return JsonSerializer.Serialize(new
        {
            ok = true,
            studentId,
            templateSource = templateResult.Ok ? "database_or_global" : "default_fallback",
            html = safe
        }, JsonOpts);
    }

    private async Task<string> SummarizeStudentProfileAsync(string argumentsJson, CancellationToken cancellationToken)
    {
        var args = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson).RootElement;
        if (!args.TryGetProperty("studentId", out var sidEl) || !sidEl.TryGetInt32(out var studentId) || studentId <= 0)
            return JsonSerializer.Serialize(new { ok = false, error = "studentId is required." }, JsonOpts);

        var data = await _unitOfWork.Students.GetUpdateStudentWithGuardianRequestData(studentId);
        if (data == null)
            return JsonSerializer.Serialize(new { ok = false, error = $"No student found for id {studentId}." }, JsonOpts);

        var divisions = await _unitOfWork.Divisions.GetAll();
        var div = divisions.FirstOrDefault(d => d.DivisionID == data.DivisionID);

        var from = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-90));
        var rows = await _tenantDb.Attendances.AsNoTracking()
            .Where(a => a.StudentID == studentId && a.AttendanceDate >= from)
            .ToListAsync(cancellationToken);

        var stats = new
        {
            Total = rows.Count,
            Absent = rows.Count(a => a.Status == AttendanceStatus.Absent),
            Late = rows.Count(a => a.Status == AttendanceStatus.Late),
            Excused = rows.Count(a => a.Status == AttendanceStatus.Excused),
            Present = rows.Count(a => a.Status == AttendanceStatus.Present)
        };

        return JsonSerializer.Serialize(new
        {
            ok = true,
            summary = new
            {
                studentId,
                studentName = $"{data.StudentFirstName} {data.StudentMiddleName} {data.StudentLastName}".Replace("  ", " ", StringComparison.Ordinal).Trim(),
                className = div?.ClassesName,
                divisionName = div?.DivisionName,
                stageName = div?.StageName,
                guardianName = data.GuardianFullName,
                guardianPhone = data.GuardianPhone,
                last90DaysAttendance = stats
            }
        }, JsonOpts);
    }

    private async Task<string> DraftParentMessageAsync(string argumentsJson, CancellationToken cancellationToken)
    {
        var args = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson).RootElement;
        if (!args.TryGetProperty("studentId", out var sidEl) || !sidEl.TryGetInt32(out var studentId) || studentId <= 0)
            return JsonSerializer.Serialize(new { ok = false, error = "studentId is required." }, JsonOpts);

        var reason = args.TryGetProperty("reason", out var rEl) ? rEl.GetString()?.Trim() : null;
        if (string.IsNullOrEmpty(reason))
            return JsonSerializer.Serialize(new { ok = false, error = "reason is required." }, JsonOpts);

        var data = await _unitOfWork.Students.GetUpdateStudentWithGuardianRequestData(studentId);
        if (data == null)
            return JsonSerializer.Serialize(new { ok = false, error = $"No student found for id {studentId}." }, JsonOpts);

        var studentName = $"{data.StudentFirstName} {data.StudentMiddleName} {data.StudentLastName}".Replace("  ", " ", StringComparison.Ordinal).Trim();
        var guardian = data.GuardianFullName ?? "ولي الأمر الكريم";

        var body = $"""
السلام عليكم ورحمة الله وبركاته،
تحية طيبة وبعد،

نود مخاطبة {guardian} بخصوص الطالب/ة: {studentName} (رقم الطالب: {studentId}).

الموضوع: {reason}

نرجو منكم التواصل مع الإدارة عند أقرب فرصة لبحث الأمر والتنسيق بما يحقق مصلحة الطالب.

وتفضلوا بقبول فائق الاحترام،
إدارة المدرسة
""";

        return JsonSerializer.Serialize(new { ok = true, messageAr = body, studentId, studentName }, JsonOpts);
    }

    private async Task<string> SearchAttendanceAsync(string argumentsJson, CancellationToken cancellationToken)
    {
        var args = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson).RootElement;

        var highAbsence = args.TryGetProperty("highAbsenceOnly", out var ha) && ha.ValueKind == JsonValueKind.True;
        var minAbsences = 5;
        if (args.TryGetProperty("minAbsences", out var minEl) && minEl.TryGetInt32(out var m) && m > 0)
            minAbsences = m;

        var limit = 50;
        if (args.TryGetProperty("limit", out var limEl) && limEl.TryGetInt32(out var l) && l > 0 && l <= 200)
            limit = l;

        DateOnly? from = null;
        DateOnly? to = null;
        if (args.TryGetProperty("from", out var fromEl) && fromEl.ValueKind == JsonValueKind.String &&
            DateOnly.TryParse(fromEl.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var fd))
            from = fd;
        if (args.TryGetProperty("to", out var toEl) && toEl.ValueKind == JsonValueKind.String &&
            DateOnly.TryParse(toEl.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var td))
            to = td;

        if (highAbsence)
        {
            var windowStart = from ?? DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-120));
            var grouped = await _tenantDb.Attendances.AsNoTracking()
                .Where(a => a.Status == AttendanceStatus.Absent && a.AttendanceDate >= windowStart)
                .GroupBy(a => a.StudentID)
                .Select(g => new { StudentId = g.Key, Count = g.Count() })
                .Where(x => x.Count >= minAbsences)
                .OrderByDescending(x => x.Count)
                .Take(limit)
                .ToListAsync(cancellationToken);

            var ids = grouped.Select(x => x.StudentId).ToList();
            var students = await _tenantDb.Students.AsNoTracking()
                .Where(s => ids.Contains(s.StudentID))
                .ToListAsync(cancellationToken);

            string FormatHighAbsenceName(Student s) =>
                $"{s.FullName.FirstName} {s.FullName.MiddleName} {s.FullName.LastName}".Replace("  ", " ", StringComparison.Ordinal).Trim();

            var rows = grouped.Select(g =>
            {
                var s = students.FirstOrDefault(x => x.StudentID == g.StudentId);
                return new
                {
                    g.StudentId,
                    absentCount = g.Count,
                    fullName = s == null ? "" : FormatHighAbsenceName(s)
                };
            }).ToList();

            return JsonSerializer.Serialize(new { ok = true, mode = "high_absence", windowStart = windowStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), rows }, JsonOpts);
        }

        int? studentId = null;
        if (args.TryGetProperty("studentId", out var sidEl) && sidEl.ValueKind == JsonValueKind.Number && sidEl.TryGetInt32(out var sid) && sid > 0)
            studentId = sid;

        AttendanceStatus? statusFilter = null;
        if (args.TryGetProperty("status", out var stEl) && stEl.ValueKind == JsonValueKind.String)
        {
            var s = stEl.GetString()?.Trim();
            statusFilter = s?.ToLowerInvariant() switch
            {
                "present" => AttendanceStatus.Present,
                "absent" => AttendanceStatus.Absent,
                "late" => AttendanceStatus.Late,
                "excused" => AttendanceStatus.Excused,
                "all" or null or "" => null,
                _ => null
            };
        }

        var q = _tenantDb.Attendances.AsNoTracking().Include(a => a.Student).Include(a => a.Class).AsQueryable();
        if (studentId.HasValue)
            q = q.Where(a => a.StudentID == studentId.Value);
        if (from.HasValue)
            q = q.Where(a => a.AttendanceDate >= from.Value);
        if (to.HasValue)
            q = q.Where(a => a.AttendanceDate <= to.Value);
        if (statusFilter.HasValue)
            q = q.Where(a => a.Status == statusFilter.Value);

        var raw = await q
            .OrderByDescending(a => a.AttendanceDate)
            .Take(limit)
            .ToListAsync(cancellationToken);

        static string FormatAttendanceStudentName(Student? s)
        {
            if (s?.FullName == null) return string.Empty;
            return $"{s.FullName.FirstName} {s.FullName.MiddleName} {s.FullName.LastName}".Replace("  ", " ", StringComparison.Ordinal).Trim();
        }

        var list = raw.Select(a => new
        {
            a.AttendanceId,
            a.StudentID,
            studentName = FormatAttendanceStudentName(a.Student),
            a.ClassID,
            className = a.Class?.ClassName ?? "",
            date = a.AttendanceDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            status = a.Status.ToString(),
            a.Remarks
        }).ToList();

        return JsonSerializer.Serialize(new { ok = true, mode = "records", count = list.Count, records = list }, JsonOpts);
    }

    private async Task<string> GetActiveSchoolYearLabelAsync(CancellationToken cancellationToken)
    {
        var y = await _tenantDb.Years.AsNoTracking()
            .Where(x => x.Active)
            .OrderBy(x => x.YearID)
            .FirstOrDefaultAsync(cancellationToken);

        if (y == null)
            return string.Empty;

        var end = y.YearDateEnd ?? y.YearDateStart;
        return $"{y.YearDateStart.Year}/{end.Year}";
    }
}
