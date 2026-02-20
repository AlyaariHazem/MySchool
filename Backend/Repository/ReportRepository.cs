using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Common;
using Backend.Data;
using Backend.DTOS.School.reports;
using Backend.Interfaces;
using Backend.Models;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class ReportRepository : IReportRepository
{
    private readonly TenantDbContext _context;
    private readonly HtmlSanitizationService _sanitizer;
    
    public ReportRepository(TenantDbContext context, HtmlSanitizationService sanitizer)
    {
        _context = context;
        _sanitizer = sanitizer;
    }

    public async Task<Result<List<MonthlyResult>>> MonthlyReportsAsync(int yearId, int termId, int monthId, int classId, int divisionId, int studentId)
    {
        try
        {

            var grouped = await _context.MonthlyGrades
                .AsNoTracking()
                .Where(g => g.YearID == yearId && g.TermID == termId && g.MonthID == monthId && g.ClassID == classId && g.Student.DivisionID == divisionId)
                .Include(g => g.Student).ThenInclude(s => s.FullName)
                .Include(g => g.Subject)
                .Include(g => g.Term)
                .Include(g => g.Month)
                .Include(g => g.Class).ThenInclude(c => c.Teacher)
                .Include(g => g.Class).ThenInclude(c => c.Divisions)
                .Include(g => g.Year).ThenInclude(y => y.School)
                .ToListAsync();
            if (studentId != 0)
            {
                grouped = await _context.MonthlyGrades
                .AsNoTracking()
                .Where(g => g.YearID == yearId && g.TermID == termId && g.MonthID == monthId && g.ClassID == classId && g.Student.DivisionID == divisionId && g.StudentID == studentId)
                .Include(g => g.Student).ThenInclude(s => s.FullName)
                .Include(g => g.Subject)
                .Include(g => g.Term)
                .Include(g => g.Month)
                .Include(g => g.Class).ThenInclude(c => c.Teacher)
                .Include(g => g.Class).ThenInclude(c => c.Divisions)
                .Include(g => g.Year).ThenInclude(y => y.School)
                .ToListAsync();
            }
            if (grouped == null || !grouped.Any())
            {
                return Result<List<MonthlyResult>>.Fail("No data found for the specified criteria.");
            }
            var monthlyResults = grouped
            .GroupBy(g => new
            {
                g.StudentID,
                g.MonthID
            }).Select(g => new MonthlyResult
            {
                StudentID = g.Key.StudentID,
                StudentName = g.First().Student.FullName.FirstName + " " +
                              g.First().Student.FullName.MiddleName + " " +
                              g.First().Student.FullName.LastName,

                SchoolName = g.First().Year.School.SchoolName,
                SchoolURL = g.First().Year.School.Website,

                Year = $"{g.First().Year.YearDateStart:yyyy}-{(g.First().Year.YearDateEnd ?? DateTime.MinValue).ToString("yyyy")}",
                Term = g.First().Term.Name,
                Month = g.First().Month.Name,
                Class = g.First().Class.ClassName,

                Division = g.First().Class.Divisions
                    .Where(d => d.DivisionID == g.First().Student.DivisionID)
                    .Select(d => d.DivisionName)
                    .FirstOrDefault(),

                Teacher = g.First().Class.Teacher != null
                            ? g.First().Class.Teacher.FullName.FirstName + " " +
                              g.First().Class.Teacher.FullName.MiddleName + " " +
                              g.First().Class.Teacher.FullName.LastName
                            : null,

                Grade = g.Sum(x => x.Grade ?? 0),

                gradeSubjects = g
                    .GroupBy(x => new { x.SubjectID, x.Subject.SubjectName })
                    .Select(subGroup => new GradeSubject
                    {
                        SubjectID = subGroup.Key.SubjectID,
                        SubjectName = subGroup.Key.SubjectName,
                        Grade = subGroup.Sum(s => s.Grade ?? 0)
                    })
                    .ToList()
            }).ToList();

            return Result<List<MonthlyResult>>.Success(monthlyResults);
        }
        catch (Exception ex)
        {
            return Result<List<MonthlyResult>>.Fail($"Error generating monthly report: {ex.Message}");
        }
    }

    public async Task<Result<ReportTemplateGetDTO>> GetTemplateByCodeAsync(string code, int? schoolId)
    {
        try
        {
            var query = _context.ReportTemplates.AsQueryable();

            // First try to find school-specific template
            if (schoolId.HasValue)
            {
                var schoolTemplate = await query
                    .Where(rt => rt.Code == code && rt.SchoolId == schoolId)
                    .FirstOrDefaultAsync();

                if (schoolTemplate != null)
                {
                    return Result<ReportTemplateGetDTO>.Success(new ReportTemplateGetDTO
                    {
                        Id = schoolTemplate.Id,
                        Name = schoolTemplate.Name,
                        Code = schoolTemplate.Code,
                        SchoolId = schoolTemplate.SchoolId,
                        TemplateHtml = schoolTemplate.TemplateHtml,
                        CreatedAt = schoolTemplate.CreatedAt,
                        UpdatedAt = schoolTemplate.UpdatedAt
                    });
                }
            }

            // Fallback to global template (SchoolId is null)
            var globalTemplate = await query
                .Where(rt => rt.Code == code && rt.SchoolId == null)
                .FirstOrDefaultAsync();

            if (globalTemplate == null)
            {
                return Result<ReportTemplateGetDTO>.Fail($"Template with code '{code}' not found.");
            }

            return Result<ReportTemplateGetDTO>.Success(new ReportTemplateGetDTO
            {
                Id = globalTemplate.Id,
                Name = globalTemplate.Name,
                Code = globalTemplate.Code,
                SchoolId = globalTemplate.SchoolId,
                TemplateHtml = globalTemplate.TemplateHtml,
                CreatedAt = globalTemplate.CreatedAt,
                UpdatedAt = globalTemplate.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            return Result<ReportTemplateGetDTO>.Fail($"Error retrieving template: {ex.Message}");
        }
    }

    public async Task<Result<ReportTemplateGetDTO>> SaveTemplateAsync(ReportTemplateSaveDTO dto, int? schoolId)
    {
        try
        {
            // Use the schoolId from parameter (passed from controller) or from DTO
            var finalSchoolId = schoolId ?? dto.SchoolId;

            // Check if template already exists
            var existingTemplate = await _context.ReportTemplates
                .Where(rt => rt.Code == dto.Code && rt.SchoolId == finalSchoolId)
                .FirstOrDefaultAsync();

            ReportTemplate template;

            // Sanitize HTML before saving
            var sanitizedHtml = _sanitizer.Sanitize(dto.TemplateHtml);

            if (existingTemplate != null)
            {
                // Update existing template
                existingTemplate.Name = dto.Name;
                existingTemplate.TemplateHtml = sanitizedHtml;
                existingTemplate.UpdatedAt = DateTime.UtcNow;
                template = existingTemplate;
            }
            else
            {
                // Create new template
                template = new ReportTemplate
                {
                    Name = dto.Name,
                    Code = dto.Code,
                    SchoolId = finalSchoolId,
                    TemplateHtml = sanitizedHtml,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _context.ReportTemplates.AddAsync(template);
            }

            await _context.SaveChangesAsync();

            return Result<ReportTemplateGetDTO>.Success(new ReportTemplateGetDTO
            {
                Id = template.Id,
                Name = template.Name,
                Code = template.Code,
                SchoolId = template.SchoolId,
                TemplateHtml = template.TemplateHtml,
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt
            });
        }
        catch (DbUpdateException ex)
        {
            // Handle unique constraint violations
            if (ex.InnerException?.Message.Contains("UNIQUE") == true || 
                ex.InnerException?.Message.Contains("duplicate") == true)
            {
                return Result<ReportTemplateGetDTO>.Fail($"A template with code '{dto.Code}' already exists for this school.");
            }
            return Result<ReportTemplateGetDTO>.Fail($"Error saving template: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<ReportTemplateGetDTO>.Fail($"Error saving template: {ex.Message}");
        }
    }
}
