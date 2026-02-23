using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS.School.WeeklySchedule;
using Backend.Models;
using Backend.Repository.School.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School
{
    public class WeeklyScheduleRepository : IWeeklyScheduleRepository
    {
        private readonly TenantDbContext _db;
        private readonly IMapper _mapper;

        // Arabic day names
        private static readonly string[] DayNames = { "السبت", "الأحد", "الإثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة" };
        
        // Arabic period names
        private static readonly string[] PeriodNames = { "", "الأولى", "الثانية", "الثالثة", "الرابعة", "الخامسة", "السادسة", "السابعة", "الثامنة", "التاسعة", "العاشرة" };

        public WeeklyScheduleRepository(TenantDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task Add(AddWeeklyScheduleDTO obj)
        {
            var schedule = _mapper.Map<WeeklySchedule>(obj);
            schedule.CreatedDate = DateTime.Now;
            
            await _db.WeeklySchedules.AddAsync(schedule);
            await _db.SaveChangesAsync();
        }

        public async Task Update(UpdateWeeklyScheduleDTO obj)
        {
            var existing = await _db.WeeklySchedules
                .FirstOrDefaultAsync(s => s.WeeklyScheduleID == obj.WeeklyScheduleID);

            if (existing == null)
                throw new InvalidOperationException($"WeeklySchedule with ID {obj.WeeklyScheduleID} not found.");

            existing.DayOfWeek = obj.DayOfWeek;
            existing.PeriodNumber = obj.PeriodNumber;
            existing.StartTime = obj.StartTime;
            existing.EndTime = obj.EndTime;
            existing.ClassID = obj.ClassID;
            existing.TermID = obj.TermID;
            existing.SubjectID = obj.SubjectID;
            existing.TeacherID = obj.TeacherID;
            existing.YearID = obj.YearID;
            existing.DivisionID = obj.DivisionID;
            existing.UpdatedDate = DateTime.Now;

            _db.Entry(existing).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var schedule = await GetByIdAsync(id);
            if (schedule != null)
            {
                _db.WeeklySchedules.Remove(schedule);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<WeeklySchedule> GetByIdAsync(int id)
        {
            return await _db.WeeklySchedules
                .Include(s => s.Class)
                .Include(s => s.Term)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .Include(s => s.Year)
                .Include(s => s.Division)
                .FirstOrDefaultAsync(s => s.WeeklyScheduleID == id) ?? null!;
        }

        public async Task<List<WeeklyScheduleDTO>> GetAllAsync()
        {
            var schedules = await _db.WeeklySchedules
                .Include(s => s.Class)
                .Include(s => s.Term)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .Include(s => s.Year)
                .Include(s => s.Division)
                .Where(s => s.Year.Active == true)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.PeriodNumber)
                .ToListAsync();

            return schedules.Select(s => MapToDTO(s)).ToList();
        }

        public async Task<List<WeeklyScheduleDTO>> GetByClassAndTermAsync(int classId, int termId, int? divisionId = null)
        {
            var query = _db.WeeklySchedules
                .Include(s => s.Class)
                .Include(s => s.Term)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .Include(s => s.Year)
                .Include(s => s.Division)
                .Where(s => s.ClassID == classId && s.TermID == termId && s.Year.Active == true);

            // Filter by division if provided
            if (divisionId.HasValue)
            {
                query = query.Where(s => s.DivisionID == divisionId.Value);
            }

            var schedules = await query
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.PeriodNumber)
                .ToListAsync();

            return schedules.Select(s => MapToDTO(s)).ToList();
        }

        public async Task<WeeklyScheduleGridDTO> GetScheduleGridAsync(int classId, int termId, int? divisionId = null)
        {
            var classEntity = await _db.Classes
                .Include(c => c.Year)
                .FirstOrDefaultAsync(c => c.ClassID == classId);

            var termEntity = await _db.Terms.FirstOrDefaultAsync(t => t.TermID == termId);

            if (classEntity == null || termEntity == null)
                throw new InvalidOperationException("Class or Term not found.");

            var schedules = await GetByClassAndTermAsync(classId, termId, divisionId);

            // Get unique periods from schedules
            var periods = schedules
                .Select(s => new PeriodDTO
                {
                    PeriodNumber = s.PeriodNumber,
                    PeriodName = s.PeriodName,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime
                })
                .GroupBy(p => p.PeriodNumber)
                .Select(g => g.First())
                .OrderBy(p => p.PeriodNumber)
                .ToList();

            return new WeeklyScheduleGridDTO
            {
                ClassID = classId,
                ClassName = classEntity.ClassName,
                TermID = termId,
                TermName = termEntity.Name,
                YearID = classEntity.YearID ?? 0,
                ScheduleItems = schedules,
                Periods = periods
            };
        }

        public async Task BulkUpdateAsync(List<AddWeeklyScheduleDTO> schedules)
        {
            if (schedules == null || !schedules.Any())
                return;

            // Get the first schedule to determine class, term, and division
            var firstSchedule = schedules.First();
            var classId = firstSchedule.ClassID;
            var termId = firstSchedule.TermID;
            var divisionId = firstSchedule.DivisionID;

            // Delete existing schedules for this class, term, and division
            var query = _db.WeeklySchedules
                .Where(s => s.ClassID == classId && s.TermID == termId);

            // If divisionId is provided, filter by it; otherwise delete all for class/term
            if (divisionId.HasValue)
            {
                query = query.Where(s => s.DivisionID == divisionId.Value);
            }

            var existingSchedules = await query.ToListAsync();

            _db.WeeklySchedules.RemoveRange(existingSchedules);

            // Add new schedules
            var newSchedules = schedules.Select(s => 
            {
                var schedule = _mapper.Map<WeeklySchedule>(s);
                schedule.CreatedDate = DateTime.Now;
                return schedule;
            }).ToList();

            await _db.WeeklySchedules.AddRangeAsync(newSchedules);
            await _db.SaveChangesAsync();
        }

        private WeeklyScheduleDTO MapToDTO(WeeklySchedule schedule)
        {
            return new WeeklyScheduleDTO
            {
                WeeklyScheduleID = schedule.WeeklyScheduleID,
                DayOfWeek = schedule.DayOfWeek,
                DayName = DayNames[schedule.DayOfWeek],
                PeriodNumber = schedule.PeriodNumber,
                PeriodName = schedule.PeriodNumber > 0 && schedule.PeriodNumber < PeriodNames.Length 
                    ? PeriodNames[schedule.PeriodNumber] 
                    : $"الحصة {schedule.PeriodNumber}",
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                ClassID = schedule.ClassID,
                ClassName = schedule.Class?.ClassName ?? string.Empty,
                TermID = schedule.TermID,
                TermName = schedule.Term?.Name ?? string.Empty,
                SubjectID = schedule.SubjectID,
                SubjectName = schedule.Subject?.SubjectName,
                TeacherID = schedule.TeacherID,
                TeacherName = schedule.Teacher != null 
                    ? $"{schedule.Teacher.FullName?.FirstName} {schedule.Teacher.FullName?.LastName}".Trim()
                    : null,
                YearID = schedule.YearID,
                DivisionID = schedule.DivisionID,
                DivisionName = schedule.Division?.DivisionName
            };
        }
    }
}
