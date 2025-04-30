using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Common;
using Backend.Data;
using Backend.DTOS.School.reports;
using Backend.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class ReportRepository : IReportRepository
{
    private readonly DatabaseContext _context;
    public ReportRepository(DatabaseContext context)
    {
        _context = context;
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
}
