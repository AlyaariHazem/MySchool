using AutoMapper;
using Backend.Data;
using Backend.DTOS.School.CoursePlan;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class CoursePlanRepository : ICoursePlanRepository
{
    private readonly DatabaseContext _context;
    private readonly IMapper _mapper;

    public CoursePlanRepository(DatabaseContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<CoursePlanDTO> AddAsync(CoursePlanDTO dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "CoursePlanDTO cannot be null.");

        // Step 1: Add the CoursePlan to the database
        var entity = _mapper.Map<CoursePlan>(dto);
        _context.CoursePlans.Add(entity);
        await _context.SaveChangesAsync(); // Save the CoursePlan and get the ID

        // dto.CoursePlanID = entity.CoursePlanID; // Set the CoursePlanID back to DTO

        // Step 2: Assign the subject to the students of the specified class
        var students = await _context.Students
        .Include(s => s.Division)
        .Where(s => s.DivisionID == dto.DivisionID && s.Division.Class.Year.YearID == dto.YearID && s.Division.Class.ClassID == dto.ClassID)
        .Select(s => new
        {
            s.StudentID,
            dto.YearID,
            dto.ClassID,
            dto.SubjectID
        })
        .ToListAsync();


        // Step 3: Create the list of subjects for each student and save them to MonthlyGrades (or another related entity)
        var subjectAssignments = new List<MonthlyGrade>(); // Assuming MonthlyGrade holds the subject assignments for students
        var TermlyGradesData = new List<TermlyGrade>(); // Assuming MonthlyGrade holds the subject assignments for students

        // Fetch Months and GradeTypes
        var months = await _context.YearTermMonths
            .Where(m => m.YearID == dto.YearID && m.TermID == dto.TermID)
            .Select(m => m.MonthID)
            .ToListAsync();

        var gradeTypes = await _context.GradeTypes
            .Where(g => g.IsActive == true)
            .Select(g => g.GradeTypeID)
            .ToListAsync();

        foreach (var student in students)
        {
            var newTermlyGrade = new TermlyGrade
            {
                StudentID = student.StudentID,
                SubjectID = dto.SubjectID,  // The subject associated with the course plan
                ClassID = dto.ClassID,
                YearID = student.YearID,
                TermID = dto.TermID,
                Grade = null, // Default grade (0 or null, depending on your requirement)
                Note = null // Default note (null or empty string, depending on your requirement)
            };
            TermlyGradesData.Add(newTermlyGrade);

            // Step 4: For each student, loop through all months and grade types to create new grades
            foreach (var month in months)
            {
                foreach (var gradeType in gradeTypes)
                {
                    var newGrade = new MonthlyGrade
                    {
                        StudentID = student.StudentID,
                        YearID = student.YearID,
                        SubjectID = dto.SubjectID,  // The subject associated with the course plan
                        ClassID = dto.ClassID,
                        TermID = dto.TermID,
                        MonthID = month,  // MonthID from the Months list
                        GradeTypeID = gradeType, // GradeTypeID from the GradeTypes list
                        Grade = null // Default grade (null, depending on your requirement)
                    };

                    subjectAssignments.Add(newGrade);
                }

            }
        }

        // Step 5: Save all the subject assignments (i.e., MonthlyGrades)
        if (subjectAssignments.Count > 0)
        {
            _context.MonthlyGrades.AddRange(subjectAssignments);
            await _context.SaveChangesAsync();
        }

        if (TermlyGradesData.Count > 0)
        {
            _context.TermlyGrades.AddRange(TermlyGradesData);
            await _context.SaveChangesAsync();
        }

        return dto;
    }


    public async Task<List<CoursePlanReturnDTO>> GetAllAsync()
    {
        var list = await _context.CoursePlans
            .Include(p => p.Subject)
            .Include(p => p.Teacher)
            .Include(p => p.Class)
            .Include(p => p.Division)
            .Include(p => p.Term)
            .Include(p => p.Year)
            .Select(item => new CoursePlanReturnDTO
            {
                CoursePlanName = $"{item.Subject.SubjectName}-{item.Class.ClassName}",
                TeacherName = $"{item.Teacher.FullName.FirstName} {item.Teacher.FullName.MiddleName} {item.Teacher.FullName.LastName}",
                DivisionName = $"{item.Division.DivisionName}",
                TermName = $"{item.Term.Name}",
                Year = $"{item.Year.YearDateStart.Year}-{item.Year.YearDateEnd!.Value.Year}",
            })
            .ToListAsync();
        
        return list;
    }

    public async Task<CoursePlanDTO?> GetByIdAsync(int id)
    {
        var entity = await _context.CoursePlans.FindAsync(id);
        return entity == null ? null : _mapper.Map<CoursePlanDTO>(entity);
    }

    public async Task UpdateAsync(CoursePlanDTO dto)
    {
        var entity = await _context.CoursePlans.FindAsync(dto.CoursePlanID);
        if (entity == null)
            throw new KeyNotFoundException("Course plan not found.");

        _mapper.Map(dto, entity);
        _context.CoursePlans.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.CoursePlans.FindAsync(id);
        if (entity == null)
            throw new KeyNotFoundException("Course plan not found.");

        _context.CoursePlans.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<List<SubjectCoursePlanDTO>> GetAllSubjectsAsync()
    {
        var subjects = await _context.CoursePlans
            .Include(s => s.Subject)
            .Select(s => new SubjectCoursePlanDTO
            {
                SubjectID = s.SubjectID,
                SubjectName = s.Subject.SubjectName,
            })
            .Distinct()
            .ToListAsync();
        return subjects;
    }
}
