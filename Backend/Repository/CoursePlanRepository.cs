using AutoMapper;
using Backend.Data;
using Backend.DTOS.School.CoursePlan;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class CoursePlanRepository : ICoursePlanRepository
{
    private readonly TenantDbContext _context;
    private readonly IMapper _mapper;

    public CoursePlanRepository(TenantDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<CoursePlanDTO> AddAsync(CoursePlanDTO dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "CoursePlanDTO cannot be null.");

        // Validate that YearID is provided and exists
        if (dto.YearID <= 0)
            throw new ArgumentException("YearID must be provided and greater than 0.", nameof(dto));

        // Verify the year exists
        var year = await _context.Years.FirstOrDefaultAsync(y => y.YearID == dto.YearID);
        if (year == null)
            throw new InvalidOperationException($"Year with ID {dto.YearID} not found. Please select a valid year.");

        // Step 1: Add the CoursePlan to the database using the YearID from DTO
        var entity = _mapper.Map<CoursePlan>(dto);
        entity.YearID = dto.YearID; // Use YearID from DTO (user-selected year)
        _context.CoursePlans.Add(entity);
        await _context.SaveChangesAsync(); // Save the CoursePlan and get the ID

        // dto.CoursePlanID = entity.CoursePlanID; // Set the CoursePlanID back to DTO

        // Step 2: Assign the subject to the students of the specified class
        // Use the YearID from DTO (user-selected year) instead of active year
        var students = await _context.Students
        .Include(s => s.Division)
            .ThenInclude(d => d.Class)
                .ThenInclude(c => c.Year)
        .Include(s => s.Division)
            .ThenInclude(d => d.Class)
                .ThenInclude(c => c.Stage)
                    .ThenInclude(st => st.Year)
        .Where(s => s.DivisionID == dto.DivisionID && 
                   s.Division.Class.ClassID == dto.ClassID &&
                   ((s.Division.Class.Year != null && s.Division.Class.Year.YearID == dto.YearID) ||
                    (s.Division.Class.Stage != null && s.Division.Class.Stage.Year != null && s.Division.Class.Stage.Year.YearID == dto.YearID)))
        .Select(s => new
        {
            s.StudentID,
            YearID = dto.YearID,
            dto.ClassID,
            dto.SubjectID
        })
        .ToListAsync();


        // Step 3: Create the list of subjects for each student and save them to MonthlyGrades (or another related entity)
        var subjectAssignments = new List<MonthlyGrade>(); // Assuming MonthlyGrade holds the subject assignments for students
        var TermlyGradesData = new List<TermlyGrade>(); // Assuming MonthlyGrade holds the subject assignments for students

        // Fetch Months and GradeTypes for the selected year
        var months = await _context.YearTermMonths
            .Where(m => m.YearID == dto.YearID && m.TermID == dto.TermID)
            .Select(m => m.MonthID)
            .ToListAsync();

        var gradeTypes = await _context.GradeTypes
            .Where(g => g.IsActive == true)
            .Select(g => g.GradeTypeID)
            .ToListAsync();

        // First, get all existing grades in bulk to avoid multiple database queries
        var studentIDs = students.Select(s => s.StudentID).ToList();
        
        var existingTermlyGrades = await _context.TermlyGrades
            .Where(tg => studentIDs.Contains(tg.StudentID) &&
                        tg.SubjectID == dto.SubjectID &&
                        tg.ClassID == dto.ClassID &&
                        tg.TermID == dto.TermID &&
                        tg.YearID == dto.YearID)
            .Select(tg => new { tg.StudentID, tg.SubjectID, tg.ClassID, tg.TermID, tg.YearID })
            .ToListAsync();

        var existingMonthlyGrades = await _context.MonthlyGrades
            .Where(mg => studentIDs.Contains(mg.StudentID) &&
                        mg.SubjectID == dto.SubjectID &&
                        mg.ClassID == dto.ClassID &&
                        mg.TermID == dto.TermID &&
                        mg.YearID == dto.YearID &&
                        months.Contains(mg.MonthID) &&
                        gradeTypes.Contains(mg.GradeTypeID))
            .Select(mg => new { mg.StudentID, mg.SubjectID, mg.ClassID, mg.TermID, mg.YearID, mg.MonthID, mg.GradeTypeID })
            .ToListAsync();

        foreach (var student in students)
        {
            // Check if TermlyGrade already exists for this student
            var termlyExists = existingTermlyGrades.Any(tg => 
                tg.StudentID == student.StudentID &&
                tg.SubjectID == dto.SubjectID &&
                tg.ClassID == dto.ClassID &&
                tg.TermID == dto.TermID &&
                tg.YearID == dto.YearID);

            if (!termlyExists)
            {
                var newTermlyGrade = new TermlyGrade
                {
                    StudentID = student.StudentID,
                    SubjectID = dto.SubjectID,  // The subject associated with the course plan
                    ClassID = dto.ClassID,
                    YearID = dto.YearID,  // Use YearID from DTO (user-selected year)
                    TermID = dto.TermID,
                    Grade = null, // Default grade (0 or null, depending on your requirement)
                    Note = null // Default note (null or empty string, depending on your requirement)
                };
                TermlyGradesData.Add(newTermlyGrade);
            }

            // Step 4: For each student, loop through all months and grade types to create new grades
            foreach (var month in months)
            {
                foreach (var gradeType in gradeTypes)
                {
                    // Check if MonthlyGrade already exists (composite key check)
                    var monthlyExists = existingMonthlyGrades.Any(mg => 
                        mg.StudentID == student.StudentID &&
                        mg.YearID == dto.YearID &&
                        mg.SubjectID == dto.SubjectID &&
                        mg.ClassID == dto.ClassID &&
                        mg.TermID == dto.TermID &&
                        mg.MonthID == month &&
                        mg.GradeTypeID == gradeType);

                    if (!monthlyExists)
                    {
                        var newGrade = new MonthlyGrade
                        {
                            StudentID = student.StudentID,
                            YearID = dto.YearID,  // Use YearID from DTO (user-selected year)
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
        }

        // Step 5: Save all the NEW subject assignments (i.e., MonthlyGrades and TermlyGrades)
        // Only create new records, never update existing ones
        // Save both in a single transaction to ensure consistency
        if (subjectAssignments.Count > 0)
        {
            _context.MonthlyGrades.AddRange(subjectAssignments);
        }

        if (TermlyGradesData.Count > 0)
        {
            _context.TermlyGrades.AddRange(TermlyGradesData);
        }

        // Save all changes in a single transaction
        if (subjectAssignments.Count > 0 || TermlyGradesData.Count > 0)
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // If there's a duplicate key error, it means some grades already exist
                // This shouldn't happen due to our checks, but handle it gracefully
                throw new InvalidOperationException($"Error saving grades: {ex.Message}. Some grades may already exist.", ex);
            }
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
                SubjectID = item.SubjectID,
                ClassID = item.ClassID,
                DivisionID = item.DivisionID,
                TeacherID = item.TeacherID,
                TermID = item.TermID,
                YearID = item.YearID
            })
            .ToListAsync();
        
        return list;
    }

    public async Task<CoursePlanDTO?> GetByIdAsync(int yearID, int teacherID, int classID, int divisionID, int subjectID)
    {
        var entity = await _context.CoursePlans.FindAsync(yearID, teacherID, classID, divisionID, subjectID);
        return entity == null ? null : _mapper.Map<CoursePlanDTO>(entity);
    }

    public async Task UpdateAsync(CoursePlanDTO dto, int oldYearID, int oldTeacherID, int oldClassID, int oldDivisionID, int oldSubjectID)
    {
        var entity = await _context.CoursePlans.FindAsync(oldYearID, oldTeacherID, oldClassID, oldDivisionID, oldSubjectID);
        if (entity == null)
            throw new KeyNotFoundException("Course plan not found.");

        // Get the active year - if multiple active years exist, take the first one
        var activeYear = await _context.Years
            .Where(y => y.Active == true)
            .OrderBy(y => y.YearID) // Order by YearID to ensure consistent selection
            .FirstOrDefaultAsync();

        if (activeYear == null)
            throw new InvalidOperationException("No active year found. Please activate a year before updating a course plan.");

        // Override the YearID from the DTO with the active year's ID
        dto.YearID = activeYear.YearID;

        // If the key values changed, we need to remove the old entity and add a new one
        bool keyChanged = oldYearID != dto.YearID || oldTeacherID != dto.TeacherID || 
                         oldClassID != dto.ClassID || oldDivisionID != dto.DivisionID || 
                         oldSubjectID != dto.SubjectID;

        if (keyChanged)
        {
            // Check if new key already exists
            var existingEntity = await _context.CoursePlans.FindAsync(dto.YearID, dto.TeacherID, dto.ClassID, dto.DivisionID, dto.SubjectID);
            if (existingEntity != null)
                throw new InvalidOperationException("A course plan with the new key combination already exists.");

            // Remove old entity
            _context.CoursePlans.Remove(entity);
            await _context.SaveChangesAsync();

            // Create new entity with updated key
            var newEntity = _mapper.Map<CoursePlan>(dto);
            newEntity.YearID = activeYear.YearID; // Ensure entity uses active year
            _context.CoursePlans.Add(newEntity);
        }
        else
        {
            // Just update the existing entity properties (except key fields)
            // Since the entity is already tracked by FindAsync, we can modify it directly
            entity.TermID = dto.TermID;
            // Note: YearID, TeacherID, ClassID, DivisionID, SubjectID are part of the composite key, 
            // so they shouldn't change when keyChanged is false
            // But we ensure YearID matches active year (should already be the case)
            if (entity.YearID != activeYear.YearID)
            {
                // This shouldn't happen, but if it does, we need to handle it as a key change
                throw new InvalidOperationException("Cannot update YearID without changing the composite key. Please delete and recreate the course plan.");
            }
            
            // Explicitly mark the entity as modified to ensure EF tracks the changes
            _context.Entry(entity).Property(e => e.TermID).IsModified = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int yearID, int teacherID, int classID, int divisionID, int subjectID)
    {
        var entity = await _context.CoursePlans.FindAsync(yearID, teacherID, classID, divisionID, subjectID);
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
